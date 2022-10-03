// Program: OE_B451_PROCESS_DMDC_RESPONSE, ID: 371296323, model: 746.
// Short name: SWE03592
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B451_PROCESS_DMDC_RESPONSE.
/// </summary>
[Serializable]
public partial class OeB451ProcessDmdcResponse: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B451_PROCESS_DMDC_RESPONSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB451ProcessDmdcResponse(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB451ProcessDmdcResponse.
  /// </summary>
  public OeB451ProcessDmdcResponse(IContext context, Import import,
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
    // ***********************************************************************************************
    // DATE		Developer	Description
    // 03/01/2006      DDupree   	Initial Creation
    // 05/05/2006      Raj S   	CQ237 - Modified to extract Sponsor information 
    // and child
    //                                 
    // relationship from DMDC FCR
    // extract file.  The new change
    //                                 
    // will generate alerts and create
    // history records based on
    //                                 
    // sponsor and child relationship.
    //                                 
    // Affected Objects due to this new
    // chnage:
    //                                 
    // -	DMDC_PRO_MATCH_RESPONSE  -
    // Work Set (New Attributes)
    //                                 
    // -	SWEEB451 -
    // PROCESS_DMDC_RESPONSE  -  Cool:
    // Gen Pstep
    // 				-	SWE03592  -   OE_B451_PROCESS_DMDC_RESPONSE    - Cool:Gen AB
    // 				-	SWEXER07 -  OE_EAB_RECEIVE_FPLS_DMDC_RESPON   -  EAB
    //                                    	
    // SWCSZE40  -   Include Member for
    // File Control
    //                                    	
    // SWCSZE41  -   Include Member for
    // File Description
    //                                    	
    // SWCSZE42  -   Include Member for
    // DMDC Record Layout
    // 				-	SRC11809   -   DMDC REPORT PROGRAM  - COBOL Program.
    // 04/21/2009      Raj S   	CQ10630 - Modified to fix the reason code from
    //                                                 
    // NCPINMILIT  to NCPINMILT and
    //                                                 
    // NCPOUTMILIT to NCPOUTMILT.
    // ***********************************************************************************************
    // 06/05/209   DDupree     Added check all ssns and the person number 
    // against the invalid ssn table. Part of CQ7189.
    // __________________________________________________________________________________
    // ***********************************************************************************************
    // This cab is design the way it is because of how it has to handle reason 
    // codes. Everytime DMDC sends a file  they will send  us the entire file
    // everytime, which means most of the records we get everytime will have
    // already been processed but we only want to put out new history records
    // and alters when something is new or changed. The reason code that have
    // SYS on the end of them are basicaly dupilcate of another  reason code but
    // the SYS will not create a alert for it like it would for reason code it
    // is coping. But to this program and in everthing else it means the same
    // thing, In this program we will handle it like it is just one alter not
    // two. For example NCPCOVBEGIN means the same thing as NCPCOVBEGINSYS does
    // since we do not want to have the same history message being written over
    // and over, we will treat the records like they are one. A person being
    // inactive on a case or a date of death determines if the SYS is used in a
    // reason code.
    // ***********************************************************************************************
    export.DmdcProcessed.Count = import.DmdcProcessed.Count;
    export.HistoryRecordsCreated.Count = import.HistoryRecordsCreated.Count;

    if (AsChar(import.DmdcProMatchResponse.ChMedicalCoverageIndicator) == 'Y'
      && AsChar(import.DmdcProMatchResponse.ChMedicalCoverageSponsorCode) != '4'
      && AsChar(import.DmdcProMatchResponse.NcpInTheMilitaryIndicator) != 'Y'
      && AsChar(import.DmdcProMatchResponse.CpInTheMilitaryIndicator) != 'Y'
      && AsChar(import.DmdcProMatchResponse.PfInTheMilitaryIndicator) != 'Y')
    {
      return;
    }

    // added this check as part of cq7189.
    local.Message1.Text8 = "Bad SSN";
    local.Message1.Text6 = ", Per";
    local.Message1.Text10 = ": For the";
    local.Message2.Text10 = " on case #";

    if (!IsEmpty(import.DmdcProMatchResponse.ChSsn))
    {
      if (Verify(import.DmdcProMatchResponse.ChSsn, "0123456789") != 0)
      {
        goto Test1;
      }

      local.ConvertMessage.SsnTextPart1 =
        Substring(import.DmdcProMatchResponse.ChSsn, 1, 3);
      local.ConvertMessage.SsnTextPart2 =
        Substring(import.DmdcProMatchResponse.ChSsn, 4, 2);
      local.ConvertMessage.SsnTextPart3 =
        Substring(import.DmdcProMatchResponse.ChSsn, 6, 4);
      local.Message1.Text11 = local.ConvertMessage.SsnTextPart1 + "-" + local
        .ConvertMessage.SsnTextPart2 + "-" + local.ConvertMessage.SsnTextPart3;
      local.WorkArea.Text15 =
        NumberToString(import.DmdcProMatchResponse.ChMemberId, 15);
      local.Infrastructure.CsePersonNumber =
        Substring(local.WorkArea.Text15, 6, 10);
      local.Convert.SsnNum9 =
        (int)StringToNumber(import.DmdcProMatchResponse.ChSsn);

      if (ReadInvalidSsn())
      {
        local.Message1.Text2 = "CH";
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = local.Message1.Text8 + local
          .Message1.Text11 + local.Message1.Text6 + (
            local.Infrastructure.CsePersonNumber ?? "") + local
          .Message1.Text10 + local.Message1.Text2 + local.Message2.Text10 + import
          .DmdcProMatchResponse.CaseId;
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
      else
      {
        // this is fine, there is not invalid ssn record for this combination of
        // cse person number and ssn number
      }
    }

Test1:

    if (!IsEmpty(import.DmdcProMatchResponse.CpSsn))
    {
      if (Verify(import.DmdcProMatchResponse.CpSsn, "0123456789") != 0)
      {
        goto Test2;
      }

      local.ConvertMessage.SsnTextPart1 =
        Substring(import.DmdcProMatchResponse.CpSsn, 1, 3);
      local.ConvertMessage.SsnTextPart2 =
        Substring(import.DmdcProMatchResponse.CpSsn, 4, 2);
      local.ConvertMessage.SsnTextPart3 =
        Substring(import.DmdcProMatchResponse.CpSsn, 6, 4);
      local.Message1.Text11 = local.ConvertMessage.SsnTextPart1 + "-" + local
        .ConvertMessage.SsnTextPart2 + "-" + local.ConvertMessage.SsnTextPart3;
      local.WorkArea.Text15 =
        NumberToString(import.DmdcProMatchResponse.CpMemberId, 15);
      local.Infrastructure.CsePersonNumber =
        Substring(local.WorkArea.Text15, 6, 10);
      local.Convert.SsnNum9 =
        (int)StringToNumber(import.DmdcProMatchResponse.CpSsn);

      if (ReadInvalidSsn())
      {
        local.Message1.Text2 = "CP";
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = local.Message1.Text8 + local
          .Message1.Text11 + local.Message1.Text6 + (
            local.Infrastructure.CsePersonNumber ?? "") + local
          .Message1.Text10 + local.Message1.Text2 + local.Message2.Text10 + import
          .DmdcProMatchResponse.CaseId;
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
      else
      {
        // this is fine, there is not invalid ssn record for this combination of
        // cse person number and ssn number
      }
    }

Test2:

    if (!IsEmpty(import.DmdcProMatchResponse.NcpSsn))
    {
      if (Verify(import.DmdcProMatchResponse.NcpSsn, "0123456789") != 0)
      {
        goto Test3;
      }

      local.ConvertMessage.SsnTextPart1 =
        Substring(import.DmdcProMatchResponse.NcpSsn, 1, 3);
      local.ConvertMessage.SsnTextPart2 =
        Substring(import.DmdcProMatchResponse.NcpSsn, 4, 2);
      local.ConvertMessage.SsnTextPart3 =
        Substring(import.DmdcProMatchResponse.NcpSsn, 6, 4);
      local.Message1.Text11 = local.ConvertMessage.SsnTextPart1 + "-" + local
        .ConvertMessage.SsnTextPart2 + "-" + local.ConvertMessage.SsnTextPart3;
      local.WorkArea.Text15 =
        NumberToString(import.DmdcProMatchResponse.NcpMemberId, 15);
      local.Infrastructure.CsePersonNumber =
        Substring(local.WorkArea.Text15, 6, 10);
      local.Convert.SsnNum9 =
        (int)StringToNumber(import.DmdcProMatchResponse.NcpSsn);

      if (ReadInvalidSsn())
      {
        local.Message1.Text2 = "AP";
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = local.Message1.Text8 + local
          .Message1.Text11 + local.Message1.Text6 + (
            local.Infrastructure.CsePersonNumber ?? "") + local
          .Message1.Text10 + local.Message1.Text2 + local.Message2.Text10 + import
          .DmdcProMatchResponse.CaseId;
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
      else
      {
        // this is fine, there is not invalid ssn record for this combination of
        // cse person number and ssn number
      }
    }

Test3:

    if (!IsEmpty(import.DmdcProMatchResponse.PfSsn))
    {
      if (Verify(import.DmdcProMatchResponse.PfSsn, "0123456789") != 0)
      {
        goto Test4;
      }

      local.ConvertMessage.SsnTextPart1 =
        Substring(import.DmdcProMatchResponse.PfSsn, 1, 3);
      local.ConvertMessage.SsnTextPart2 =
        Substring(import.DmdcProMatchResponse.PfSsn, 4, 2);
      local.ConvertMessage.SsnTextPart3 =
        Substring(import.DmdcProMatchResponse.PfSsn, 6, 4);
      local.Message1.Text11 = local.ConvertMessage.SsnTextPart1 + "-" + local
        .ConvertMessage.SsnTextPart2 + "-" + local.ConvertMessage.SsnTextPart3;
      local.WorkArea.Text15 =
        NumberToString(import.DmdcProMatchResponse.PfMemberId, 15);
      local.Infrastructure.CsePersonNumber =
        Substring(local.WorkArea.Text15, 6, 10);
      local.Convert.SsnNum9 =
        (int)StringToNumber(import.DmdcProMatchResponse.PfSsn);

      if (ReadInvalidSsn())
      {
        local.Message1.Text2 = "AP";
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = local.Message1.Text8 + local
          .Message1.Text11 + local.Message1.Text6 + (
            local.Infrastructure.CsePersonNumber ?? "") + local
          .Message1.Text10 + local.Message1.Text2 + local.Message2.Text10 + import
          .DmdcProMatchResponse.CaseId;
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
      else
      {
        // this is fine, there is not invalid ssn record for this combination of
        // cse person number and ssn number
      }
    }

Test4:

    local.Group.Index = -1;
    local.Group.Count = 0;
    local.ChildInactive.Flag = "";
    local.ApInactive.Flag = "";
    local.PfInactive.Flag = "";
    local.ReadCase.Number = import.DmdcProMatchResponse.CaseId;
    local.Current.Date = Now().Date;
    local.Blank.Date = new DateTime(1, 1, 1);
    local.Blank.Timestamp = new DateTime(1, 1, 1);
    local.Max.Date = new DateTime(2099, 12, 31);

    if (import.DmdcProMatchResponse.NcpMemberId > 0)
    {
      local.WorkArea.Text15 =
        NumberToString(import.DmdcProMatchResponse.NcpMemberId, 15);
      local.Infrastructure.CsePersonNumber =
        Substring(local.WorkArea.Text15, 6, 10);

      if (ReadCaseCaseRole2())
      {
        if (Lt(local.Blank.Date, entities.CaseRole.EndDate) && Lt
          (entities.CaseRole.EndDate, local.Max.Date) || Lt
          (local.Current.Date, entities.CaseRole.StartDate))
        {
          local.ApInactive.Flag = "Y";
        }
      }
    }

    if (import.DmdcProMatchResponse.PfMemberId > 0)
    {
      local.WorkArea.Text15 =
        NumberToString(import.DmdcProMatchResponse.PfMemberId, 15);
      local.Infrastructure.CsePersonNumber =
        Substring(local.WorkArea.Text15, 6, 10);

      if (ReadCaseCaseRole2())
      {
        if (Lt(local.Blank.Date, entities.CaseRole.EndDate) && Lt
          (entities.CaseRole.EndDate, local.Max.Date) || Lt
          (local.Current.Date, entities.CaseRole.StartDate))
        {
          local.PfInactive.Flag = "Y";
        }
      }
    }

    local.WorkArea.Text15 =
      NumberToString(import.DmdcProMatchResponse.ChMemberId, 15);
    local.ChNumber.CsePersonNumber = Substring(local.WorkArea.Text15, 6, 10);
    local.WorkArea.Text15 =
      NumberToString(import.DmdcProMatchResponse.CpMemberId, 15);
    local.ArNumber.CsePersonNumber = Substring(local.WorkArea.Text15, 6, 10);
    local.WorkArea.Text15 =
      NumberToString(import.DmdcProMatchResponse.NcpMemberId, 15);
    local.ApNumber.CsePersonNumber = Substring(local.WorkArea.Text15, 6, 10);
    local.WorkArea.Text15 =
      NumberToString(import.DmdcProMatchResponse.PfMemberId, 15);
    local.PfNumber.CsePersonNumber = Substring(local.WorkArea.Text15, 6, 10);
    local.PersonQualified.Count = 0;

    if (ReadCsePerson2())
    {
      ++export.DmdcProcessed.Count;

      if (ReadCaseCaseRole1())
      {
        if (Lt(local.Blank.Date, entities.CaseRole.EndDate) && Lt
          (entities.CaseRole.EndDate, local.Max.Date) || Lt
          (local.Current.Date, entities.CaseRole.StartDate))
        {
          local.ChildInactive.Flag = "Y";
        }

        if (ReadInterstateRequest())
        {
          if (AsChar(entities.InterstateRequest.KsCaseInd) == 'Y')
          {
            local.Infrastructure.InitiatingStateCode = "KS";
          }
          else
          {
            local.Infrastructure.InitiatingStateCode = "OS";
          }
        }
        else
        {
          local.Infrastructure.InitiatingStateCode = "KS";
        }
      }

      if (import.DmdcProMatchResponse.ChMemberId > 0)
      {
        if (AsChar(import.DmdcProMatchResponse.ChMedicalCoverageSponsorCode) ==
          '4')
        {
          local.Infrastructure.CsePersonNumber = "";
        }

        local.WorkArea.Text15 =
          NumberToString(import.DmdcProMatchResponse.ChMemberId, 15);
        local.SecondRead.Number = Substring(local.WorkArea.Text15, 6, 10);

        if (!ReadCsePerson1())
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            TrimEnd(local.Infrastructure.CsePersonNumber) + " DMDC LOAD: CSE PERSON NOT FOUND.";
            
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
          }

          return;
        }

        if (AsChar(import.DmdcProMatchResponse.ChDeathIndicator) == 'Y')
        {
          ++local.Group.Index;
          local.Group.CheckSize();

          local.Group.Update.DmdcProMatchResponse.ChFirstName =
            import.DmdcProMatchResponse.ChFirstName;
          local.Group.Update.DmdcProMatchResponse.ChMiddleName =
            import.DmdcProMatchResponse.ChMiddleName;
          local.Group.Update.DmdcProMatchResponse.ChLastName =
            import.DmdcProMatchResponse.ChLastName;
          local.Group.Update.DmdcProMatchResponse.ChDeathIndicator =
            import.DmdcProMatchResponse.ChDeathIndicator;
          local.Group.Update.DmdcProMatchResponse.ChMedicalCoverageIndicator =
            import.DmdcProMatchResponse.ChMedicalCoverageIndicator;
          local.Group.Update.Infrastructure.DenormNumeric12 =
            import.DmdcProMatchResponse.ChMemberId;
          local.Group.Update.Infrastructure.EventId = 10;
          local.Group.Update.Infrastructure.DenormDate = Now().Date;
          local.Group.Update.DmdcProMatchResponse.ChMemberId =
            import.DmdcProMatchResponse.ChMemberId;
          local.Group.Update.Infrastructure.CsePersonNumber =
            local.ChNumber.CsePersonNumber ?? "";

          if (Lt(local.Blank.Date, entities.N2dRead.DateOfDeath))
          {
            local.Group.Update.Infrastructure.ReasonCode = "DEATHBEGINSYS";
            local.Group.Update.Grp2NdCode.ReasonCode = "DEATHBEGIN";
          }
          else
          {
            local.Group.Update.Grp2NdCode.ReasonCode = "DEATHBEGINSYS";
            local.Group.Update.Infrastructure.ReasonCode = "DEATHBEGIN";
          }
        }
        else
        {
          ++local.Group.Index;
          local.Group.CheckSize();

          local.Group.Update.DmdcProMatchResponse.ChFirstName =
            import.DmdcProMatchResponse.ChFirstName;
          local.Group.Update.DmdcProMatchResponse.ChMiddleName =
            import.DmdcProMatchResponse.ChMiddleName;
          local.Group.Update.DmdcProMatchResponse.ChLastName =
            import.DmdcProMatchResponse.ChLastName;
          local.Group.Update.DmdcProMatchResponse.ChDeathIndicator =
            import.DmdcProMatchResponse.ChDeathIndicator;
          local.Group.Update.DmdcProMatchResponse.ChMedicalCoverageIndicator =
            import.DmdcProMatchResponse.ChMedicalCoverageIndicator;
          local.Group.Update.Infrastructure.DenormNumeric12 =
            import.DmdcProMatchResponse.ChMemberId;
          local.Group.Update.Infrastructure.EventId = 10;
          local.Group.Update.Infrastructure.DenormDate = Now().Date;
          local.Group.Update.DmdcProMatchResponse.ChMemberId =
            import.DmdcProMatchResponse.ChMemberId;
          local.Group.Update.Infrastructure.CsePersonNumber =
            local.ChNumber.CsePersonNumber ?? "";

          if (ReadInfrastructure6())
          {
            ++local.PersonUpdated.Count;
          }

          if (Equal(entities.SecondPass.ReasonCode, "DEATHBEGINSYS"))
          {
            local.Group.Update.Infrastructure.ReasonCode = "DEATHENDSYS";
            local.Group.Update.Grp2NdCode.ReasonCode = "DEATHEND";
          }
          else
          {
            local.Group.Update.Infrastructure.ReasonCode = "DEATHEND";
            local.Group.Update.Grp2NdCode.ReasonCode = "DEATHENDSYS";
          }
        }

        // ********************************************************************
        // REF:CQ237   Who: Raj S   When: 05/02/2008    Begin Change
        // ********************************************************************
        // ********************************************************************
        // Build FCR Sponsor Child Relationship table for processing.  The FCR
        // sends values 1 thru 8 at column 521.  The following values are set
        // as per the FCR document.  In addition to this 2 character short form
        // also will be filled-in for further processing.
        // 1 - Child  (CH)
        // 2 - Foster Child (FC)
        // 3 - Pre-Adoptive Child (PA)
        // 4 - Ward (WA)
        // 5 - StepChile (SC)
        // 6 - Self (SF)   (an adult child who is a military member)
        // 7 - Spouse (SP) (an adult child who is married to a military member)
        // 8 - Other/Unknown
        // ********************************************************************
        for(local.OtherCovChildRelList.Index = 0; local
          .OtherCovChildRelList.Index < Local
          .OtherCovChildRelListGroup.Capacity; ++
          local.OtherCovChildRelList.Index)
        {
          if (!local.OtherCovChildRelList.CheckSize())
          {
            break;
          }

          switch(local.OtherCovChildRelList.Index + 1)
          {
            case 1:
              local.OtherCovChildRelList.Update.OtherCovChldRelShort.Text2 =
                "CH";
              local.OtherCovChildRelList.Update.OtherCovChildRelation.Text14 =
                "OTHCOVCH";

              break;
            case 2:
              local.OtherCovChildRelList.Update.OtherCovChldRelShort.Text2 =
                "FC";
              local.OtherCovChildRelList.Update.OtherCovChildRelation.Text14 =
                "OTHCOVFC";

              break;
            case 3:
              local.OtherCovChildRelList.Update.OtherCovChldRelShort.Text2 =
                "PA";
              local.OtherCovChildRelList.Update.OtherCovChildRelation.Text14 =
                "OTHCOVPA";

              break;
            case 4:
              local.OtherCovChildRelList.Update.OtherCovChldRelShort.Text2 =
                "WA";
              local.OtherCovChildRelList.Update.OtherCovChildRelation.Text14 =
                "OTHCOVWA";

              break;
            case 5:
              local.OtherCovChildRelList.Update.OtherCovChldRelShort.Text2 =
                "SC";
              local.OtherCovChildRelList.Update.OtherCovChildRelation.Text14 =
                "OTHCOVSC";

              break;
            case 6:
              local.OtherCovChildRelList.Update.OtherCovChldRelShort.Text2 =
                "SF";
              local.OtherCovChildRelList.Update.OtherCovChildRelation.Text14 =
                "OTHCOVSF";

              break;
            case 7:
              local.OtherCovChildRelList.Update.OtherCovChldRelShort.Text2 =
                "SP";
              local.OtherCovChildRelList.Update.OtherCovChildRelation.Text14 =
                "OTHCOVSP";

              break;
            case 8:
              local.OtherCovChildRelList.Update.OtherCovChldRelShort.Text2 =
                "OT";
              local.OtherCovChildRelList.Update.OtherCovChildRelation.Text14 =
                "OTHCOV";

              break;
            default:
              break;
          }
        }

        local.OtherCovChildRelList.CheckIndex();

        // ********************************************************************
        // Read the most recent history record from infrastructure table for the
        // selected case, AP and child.   After reading, set the end reason code
        // based on the read begin reason code.
        // ********************************************************************
        local.RecordFound.Flag = "";
        local.SelOtherStartReason.Text20 = "";
        local.SelOtherEndReason.Text20 = "";

        if (ReadInfrastructure3())
        {
          local.SelOtherStartReason.Text20 = entities.Infrastructure.ReasonCode;

          if (Equal(entities.Infrastructure.ReasonCode, 1, 6, "OTHCOV"))
          {
            if (Equal(entities.Infrastructure.ReasonCode, 7, 5, "BEGIN"))
            {
              local.SelOtherEndReason.Text20 =
                Substring(entities.Infrastructure.ReasonCode, 1, 6);
              local.SelOtherEndReason.Text20 =
                TrimEnd(local.SelOtherEndReason.Text20) + "END" + Substring
                (entities.Infrastructure.ReasonCode,
                Infrastructure.ReasonCode_MaxLength, 12, 3);
            }
            else
            {
              local.SelOtherEndReason.Text20 =
                Substring(entities.Infrastructure.ReasonCode, 1, 8);
              local.SelOtherEndReason.Text20 =
                TrimEnd(local.SelOtherEndReason.Text20) + "END" + Substring
                (entities.Infrastructure.ReasonCode,
                Infrastructure.ReasonCode_MaxLength, 12, 3);
            }
          }

          local.RecordFound.Flag = "Y";
        }

        // ********************************************************************
        // REF:CQ237   Who: Raj S   When: 05/02/2008    End Change
        // ********************************************************************
        if (AsChar(import.DmdcProMatchResponse.ChMedicalCoverageIndicator) == 'Y'
          )
        {
          if (AsChar(local.RecordFound.Flag) == 'Y')
          {
            local.RecordFound.Flag = "";

            switch(TrimEnd(entities.Infrastructure.ReasonCode))
            {
              case "NCPCOVBEGIN":
                if (ReadInfrastructure41())
                {
                  local.RecordFound.Flag = "Y";
                }

                if (AsChar(local.RecordFound.Flag) == 'Y')
                {
                  if (!Lt(entities.SecondPass.CreatedTimestamp,
                    entities.Infrastructure.CreatedTimestamp))
                  {
                    // THIS RECORD IS CLOSED SO NO NEED TO DO ANYTHING
                  }
                  else
                  {
                    // RECORD IS NOT CLOSED
                    switch(AsChar(import.DmdcProMatchResponse.
                      ChMedicalCoverageSponsorCode))
                    {
                      case '1':
                        // DOTHING, THIS IS THE ONE WE WANT OPEN
                        goto Test5;
                      case '2':
                        break;
                      case '3':
                        break;
                      case '4':
                        break;
                      default:
                        break;
                    }

                    ++local.Group.Index;
                    local.Group.CheckSize();

                    local.Group.Update.DmdcProMatchResponse.ChFirstName =
                      import.DmdcProMatchResponse.ChFirstName;
                    local.Group.Update.DmdcProMatchResponse.ChMiddleName =
                      import.DmdcProMatchResponse.ChMiddleName;
                    local.Group.Update.DmdcProMatchResponse.ChLastName =
                      import.DmdcProMatchResponse.ChLastName;
                    local.Group.Update.DmdcProMatchResponse.ChDeathIndicator =
                      import.DmdcProMatchResponse.ChDeathIndicator;
                    local.Group.Update.DmdcProMatchResponse.
                      ChMedicalCoverageIndicator =
                        import.DmdcProMatchResponse.ChMedicalCoverageIndicator;
                    local.Group.Update.Infrastructure.DenormNumeric12 =
                      import.DmdcProMatchResponse.ChMemberId;
                    local.Group.Update.Infrastructure.EventId = 10;
                    local.Group.Update.DmdcProMatchResponse.ChMemberId =
                      import.DmdcProMatchResponse.NcpMemberId;
                    local.Group.Update.Infrastructure.ReasonCode = "NCPCOVEND";
                    local.Group.Update.Grp2NdCode.ReasonCode = "NCPCOVENDSYS";
                    local.Group.Update.Infrastructure.CsePersonNumber =
                      local.ApNumber.CsePersonNumber ?? "";
                  }
                }
                else
                {
                  switch(AsChar(import.DmdcProMatchResponse.
                    ChMedicalCoverageSponsorCode))
                  {
                    case '1':
                      // DOTHING, THIS IS THE ONE WE WANT OPEN
                      goto Test5;
                    case '2':
                      break;
                    case '3':
                      break;
                    case '4':
                      break;
                    default:
                      break;
                  }

                  ++local.Group.Index;
                  local.Group.CheckSize();

                  local.Group.Update.DmdcProMatchResponse.ChFirstName =
                    import.DmdcProMatchResponse.ChFirstName;
                  local.Group.Update.DmdcProMatchResponse.ChMiddleName =
                    import.DmdcProMatchResponse.ChMiddleName;
                  local.Group.Update.DmdcProMatchResponse.ChLastName =
                    import.DmdcProMatchResponse.ChLastName;
                  local.Group.Update.DmdcProMatchResponse.ChDeathIndicator =
                    import.DmdcProMatchResponse.ChDeathIndicator;
                  local.Group.Update.DmdcProMatchResponse.
                    ChMedicalCoverageIndicator =
                      import.DmdcProMatchResponse.ChMedicalCoverageIndicator;
                  local.Group.Update.Infrastructure.DenormNumeric12 =
                    import.DmdcProMatchResponse.ChMemberId;
                  local.Group.Update.Infrastructure.EventId = 10;
                  local.Group.Update.DmdcProMatchResponse.ChMemberId =
                    import.DmdcProMatchResponse.NcpMemberId;
                  local.Group.Update.Infrastructure.ReasonCode = "NCPCOVEND";
                  local.Group.Update.Grp2NdCode.ReasonCode = "NCPCOVENDSYS";
                  local.Group.Update.Infrastructure.CsePersonNumber =
                    local.ApNumber.CsePersonNumber ?? "";
                }

                break;
              case "NCPCOVBEGINSYS":
                local.WorkArea.Text15 =
                  NumberToString(import.DmdcProMatchResponse.NcpMemberId, 15);
                local.SecondRead.Number =
                  Substring(local.WorkArea.Text15, 6, 10);

                if (ReadInfrastructure43())
                {
                  local.RecordFound.Flag = "Y";
                }

                if (AsChar(local.RecordFound.Flag) == 'Y')
                {
                  if (!Lt(entities.SecondPass.CreatedTimestamp,
                    entities.Infrastructure.CreatedTimestamp))
                  {
                    // THIS RECORD IS CLOSED SO NO NEED TO DO ANYTHING
                  }
                  else
                  {
                    // RECORD IS NOT CLOSED
                    switch(AsChar(import.DmdcProMatchResponse.
                      ChMedicalCoverageSponsorCode))
                    {
                      case '1':
                        // DOTHING, THIS IS THE ONE WE WANT OPEN
                        goto Test5;
                      case '2':
                        break;
                      case '3':
                        break;
                      case '4':
                        break;
                      default:
                        break;
                    }

                    ++local.Group.Index;
                    local.Group.CheckSize();

                    local.Group.Update.DmdcProMatchResponse.ChFirstName =
                      import.DmdcProMatchResponse.ChFirstName;
                    local.Group.Update.DmdcProMatchResponse.ChMiddleName =
                      import.DmdcProMatchResponse.ChMiddleName;
                    local.Group.Update.DmdcProMatchResponse.ChLastName =
                      import.DmdcProMatchResponse.ChLastName;
                    local.Group.Update.DmdcProMatchResponse.ChDeathIndicator =
                      import.DmdcProMatchResponse.ChDeathIndicator;
                    local.Group.Update.DmdcProMatchResponse.
                      ChMedicalCoverageIndicator =
                        import.DmdcProMatchResponse.ChMedicalCoverageIndicator;
                    local.Group.Update.Infrastructure.DenormNumeric12 =
                      import.DmdcProMatchResponse.ChMemberId;
                    local.Group.Update.Infrastructure.EventId = 10;
                    local.Group.Update.DmdcProMatchResponse.ChMemberId =
                      import.DmdcProMatchResponse.NcpMemberId;
                    local.Group.Update.Infrastructure.ReasonCode =
                      "NCPCOVENDSYS";
                    local.Group.Update.Grp2NdCode.ReasonCode = "NCPCOVEND";
                    local.Group.Update.Infrastructure.CsePersonNumber =
                      local.ApNumber.CsePersonNumber ?? "";
                  }
                }
                else
                {
                  switch(AsChar(import.DmdcProMatchResponse.
                    ChMedicalCoverageSponsorCode))
                  {
                    case '1':
                      // DOTHING, THIS IS THE ONE WE WANT OPEN
                      goto Test5;
                    case '2':
                      break;
                    case '3':
                      break;
                    case '4':
                      break;
                    default:
                      break;
                  }

                  ++local.Group.Index;
                  local.Group.CheckSize();

                  local.Group.Update.DmdcProMatchResponse.ChFirstName =
                    import.DmdcProMatchResponse.ChFirstName;
                  local.Group.Update.DmdcProMatchResponse.ChMiddleName =
                    import.DmdcProMatchResponse.ChMiddleName;
                  local.Group.Update.DmdcProMatchResponse.ChLastName =
                    import.DmdcProMatchResponse.ChLastName;
                  local.Group.Update.DmdcProMatchResponse.ChDeathIndicator =
                    import.DmdcProMatchResponse.ChDeathIndicator;
                  local.Group.Update.DmdcProMatchResponse.
                    ChMedicalCoverageIndicator =
                      import.DmdcProMatchResponse.ChMedicalCoverageIndicator;
                  local.Group.Update.Infrastructure.DenormNumeric12 =
                    import.DmdcProMatchResponse.ChMemberId;
                  local.Group.Update.Infrastructure.EventId = 10;
                  local.Group.Update.DmdcProMatchResponse.ChMemberId =
                    import.DmdcProMatchResponse.NcpMemberId;
                  local.Group.Update.Infrastructure.ReasonCode = "NCPCOVENDSYS";
                  local.Group.Update.Grp2NdCode.ReasonCode = "NCPCOVEND";
                  local.Group.Update.Infrastructure.CsePersonNumber =
                    local.ApNumber.CsePersonNumber ?? "";
                }

                break;
              case "CPCOVBEGIN":
                local.WorkArea.Text15 =
                  NumberToString(import.DmdcProMatchResponse.CpMemberId, 15);
                local.SecondRead.Number =
                  Substring(local.WorkArea.Text15, 6, 10);

                if (ReadInfrastructure42())
                {
                  local.RecordFound.Flag = "Y";
                }

                if (AsChar(local.RecordFound.Flag) == 'Y')
                {
                  if (!Lt(entities.SecondPass.CreatedTimestamp,
                    entities.Infrastructure.CreatedTimestamp))
                  {
                    // THIS RECORD IS CLOSED SO NO NEED TO DO ANYTHING
                  }
                  else
                  {
                    // RECORD IS NOT CLOSED
                    switch(AsChar(import.DmdcProMatchResponse.
                      ChMedicalCoverageSponsorCode))
                    {
                      case '1':
                        break;
                      case '2':
                        // DO NOTHING, THIS IS THE ONE WE WANT OPEN
                        goto Test5;
                      case '3':
                        break;
                      case '4':
                        break;
                      default:
                        break;
                    }

                    ++local.Group.Index;
                    local.Group.CheckSize();

                    local.Group.Update.DmdcProMatchResponse.ChFirstName =
                      import.DmdcProMatchResponse.ChFirstName;
                    local.Group.Update.DmdcProMatchResponse.ChMiddleName =
                      import.DmdcProMatchResponse.ChMiddleName;
                    local.Group.Update.DmdcProMatchResponse.ChLastName =
                      import.DmdcProMatchResponse.ChLastName;
                    local.Group.Update.DmdcProMatchResponse.ChDeathIndicator =
                      import.DmdcProMatchResponse.ChDeathIndicator;
                    local.Group.Update.DmdcProMatchResponse.
                      ChMedicalCoverageIndicator =
                        import.DmdcProMatchResponse.ChMedicalCoverageIndicator;
                    local.Group.Update.Infrastructure.DenormNumeric12 =
                      import.DmdcProMatchResponse.ChMemberId;
                    local.Group.Update.Infrastructure.EventId = 10;
                    local.Group.Update.Infrastructure.ReasonCode = "CPCOVEND";
                    local.Group.Update.DmdcProMatchResponse.ChMemberId =
                      import.DmdcProMatchResponse.CpMemberId;
                    local.Group.Update.Infrastructure.CsePersonNumber =
                      local.ArNumber.CsePersonNumber ?? "";
                  }
                }
                else
                {
                  switch(AsChar(import.DmdcProMatchResponse.
                    ChMedicalCoverageSponsorCode))
                  {
                    case '1':
                      break;
                    case '2':
                      // DO NOTHING, THIS IS THE ONE WE WANT OPEN
                      goto Test5;
                    case '3':
                      break;
                    case '4':
                      break;
                    default:
                      break;
                  }

                  ++local.Group.Index;
                  local.Group.CheckSize();

                  local.Group.Update.DmdcProMatchResponse.ChFirstName =
                    import.DmdcProMatchResponse.ChFirstName;
                  local.Group.Update.DmdcProMatchResponse.ChMiddleName =
                    import.DmdcProMatchResponse.ChMiddleName;
                  local.Group.Update.DmdcProMatchResponse.ChLastName =
                    import.DmdcProMatchResponse.ChLastName;
                  local.Group.Update.DmdcProMatchResponse.ChDeathIndicator =
                    import.DmdcProMatchResponse.ChDeathIndicator;
                  local.Group.Update.DmdcProMatchResponse.
                    ChMedicalCoverageIndicator =
                      import.DmdcProMatchResponse.ChMedicalCoverageIndicator;
                  local.Group.Update.Infrastructure.DenormNumeric12 =
                    import.DmdcProMatchResponse.ChMemberId;
                  local.Group.Update.Infrastructure.EventId = 10;
                  local.Group.Update.Infrastructure.ReasonCode = "CPCOVEND";
                  local.Group.Update.DmdcProMatchResponse.ChMemberId =
                    import.DmdcProMatchResponse.CpMemberId;
                  local.Group.Update.Infrastructure.CsePersonNumber =
                    local.ArNumber.CsePersonNumber ?? "";
                }

                break;
              case "PFCOVBEGIN":
                local.WorkArea.Text15 =
                  NumberToString(import.DmdcProMatchResponse.PfMemberId, 15);
                local.SecondRead.Number =
                  Substring(local.WorkArea.Text15, 6, 10);

                if (ReadInfrastructure44())
                {
                  local.RecordFound.Flag = "Y";
                }

                if (AsChar(local.RecordFound.Flag) == 'Y')
                {
                  if (!Lt(entities.SecondPass.CreatedTimestamp,
                    entities.Infrastructure.CreatedTimestamp))
                  {
                    // THIS RECORD IS CLOSED SO NO NEED TO DO ANYTHING
                  }
                  else
                  {
                    // RECORD IS NOT CLOSED
                    switch(AsChar(import.DmdcProMatchResponse.
                      ChMedicalCoverageSponsorCode))
                    {
                      case '1':
                        break;
                      case '2':
                        break;
                      case '3':
                        // DO NOTHING, THIS IS THE ONE WE WANT OPEN
                        goto Test5;
                      case '4':
                        break;
                      default:
                        break;
                    }

                    ++local.Group.Index;
                    local.Group.CheckSize();

                    local.Group.Update.DmdcProMatchResponse.ChFirstName =
                      import.DmdcProMatchResponse.ChFirstName;
                    local.Group.Update.DmdcProMatchResponse.ChMiddleName =
                      import.DmdcProMatchResponse.ChMiddleName;
                    local.Group.Update.DmdcProMatchResponse.ChLastName =
                      import.DmdcProMatchResponse.ChLastName;
                    local.Group.Update.DmdcProMatchResponse.ChDeathIndicator =
                      import.DmdcProMatchResponse.ChDeathIndicator;
                    local.Group.Update.DmdcProMatchResponse.
                      ChMedicalCoverageIndicator =
                        import.DmdcProMatchResponse.ChMedicalCoverageIndicator;
                    local.Group.Update.Infrastructure.DenormNumeric12 =
                      import.DmdcProMatchResponse.ChMemberId;
                    local.Group.Update.Infrastructure.EventId = 10;
                    local.Group.Update.DmdcProMatchResponse.ChMemberId =
                      import.DmdcProMatchResponse.PfMemberId;
                    local.Group.Update.Infrastructure.ReasonCode = "PFCOVEND";
                    local.Group.Update.Grp2NdCode.ReasonCode = "PFCOVENDSYS";
                    local.Group.Update.Infrastructure.CsePersonNumber =
                      local.PfNumber.CsePersonNumber ?? "";
                  }
                }
                else
                {
                  switch(AsChar(import.DmdcProMatchResponse.
                    ChMedicalCoverageSponsorCode))
                  {
                    case '1':
                      break;
                    case '2':
                      break;
                    case '3':
                      // DO NOTHING, THIS IS THE ONE WE WANT OPEN
                      goto Test5;
                    case '4':
                      break;
                    default:
                      break;
                  }

                  ++local.Group.Index;
                  local.Group.CheckSize();

                  local.Group.Update.DmdcProMatchResponse.ChFirstName =
                    import.DmdcProMatchResponse.ChFirstName;
                  local.Group.Update.DmdcProMatchResponse.ChMiddleName =
                    import.DmdcProMatchResponse.ChMiddleName;
                  local.Group.Update.DmdcProMatchResponse.ChLastName =
                    import.DmdcProMatchResponse.ChLastName;
                  local.Group.Update.DmdcProMatchResponse.ChDeathIndicator =
                    import.DmdcProMatchResponse.ChDeathIndicator;
                  local.Group.Update.DmdcProMatchResponse.
                    ChMedicalCoverageIndicator =
                      import.DmdcProMatchResponse.ChMedicalCoverageIndicator;
                  local.Group.Update.Infrastructure.DenormNumeric12 =
                    import.DmdcProMatchResponse.ChMemberId;
                  local.Group.Update.Infrastructure.EventId = 10;
                  local.Group.Update.DmdcProMatchResponse.ChMemberId =
                    import.DmdcProMatchResponse.PfMemberId;
                  local.Group.Update.Infrastructure.ReasonCode = "PFCOVEND";
                  local.Group.Update.Grp2NdCode.ReasonCode = "PFCOVENDSYS";
                  local.Group.Update.Infrastructure.CsePersonNumber =
                    local.PfNumber.CsePersonNumber ?? "";
                }

                break;
              case "PFCOVBEGINSYS":
                local.WorkArea.Text15 =
                  NumberToString(import.DmdcProMatchResponse.PfMemberId, 15);
                local.SecondRead.Number =
                  Substring(local.WorkArea.Text15, 6, 10);

                if (ReadInfrastructure45())
                {
                  local.RecordFound.Flag = "Y";
                }

                if (AsChar(local.RecordFound.Flag) == 'Y')
                {
                  if (!Lt(entities.SecondPass.CreatedTimestamp,
                    entities.Infrastructure.CreatedTimestamp))
                  {
                    // THIS RECORD IS CLOSED SO NO NEED TO DO ANYTHING
                  }
                  else
                  {
                    // RECORD IS NOT CLOSED
                    switch(AsChar(import.DmdcProMatchResponse.
                      ChMedicalCoverageSponsorCode))
                    {
                      case '1':
                        break;
                      case '2':
                        break;
                      case '3':
                        // DO NOTHING, THIS IS THE ONE WE WANT OPEN
                        goto Test5;
                      case '4':
                        break;
                      default:
                        break;
                    }

                    ++local.Group.Index;
                    local.Group.CheckSize();

                    local.Group.Update.DmdcProMatchResponse.ChFirstName =
                      import.DmdcProMatchResponse.ChFirstName;
                    local.Group.Update.DmdcProMatchResponse.ChMiddleName =
                      import.DmdcProMatchResponse.ChMiddleName;
                    local.Group.Update.DmdcProMatchResponse.ChLastName =
                      import.DmdcProMatchResponse.ChLastName;
                    local.Group.Update.DmdcProMatchResponse.ChDeathIndicator =
                      import.DmdcProMatchResponse.ChDeathIndicator;
                    local.Group.Update.DmdcProMatchResponse.
                      ChMedicalCoverageIndicator =
                        import.DmdcProMatchResponse.ChMedicalCoverageIndicator;
                    local.Group.Update.Infrastructure.DenormNumeric12 =
                      import.DmdcProMatchResponse.ChMemberId;
                    local.Group.Update.Infrastructure.EventId = 10;
                    local.Group.Update.DmdcProMatchResponse.ChMemberId =
                      import.DmdcProMatchResponse.PfMemberId;
                    local.Group.Update.Infrastructure.ReasonCode =
                      "PFCOVENDSYS";
                    local.Group.Update.Grp2NdCode.ReasonCode = "PFCOVEND";
                    local.Group.Update.Infrastructure.CsePersonNumber =
                      local.PfNumber.CsePersonNumber ?? "";
                  }
                }
                else
                {
                  switch(AsChar(import.DmdcProMatchResponse.
                    ChMedicalCoverageSponsorCode))
                  {
                    case '1':
                      break;
                    case '2':
                      break;
                    case '3':
                      // DO NOTHING, THIS IS THE ONE WE WANT OPEN
                      goto Test5;
                    case '4':
                      break;
                    default:
                      break;
                  }

                  ++local.Group.Index;
                  local.Group.CheckSize();

                  local.Group.Update.DmdcProMatchResponse.ChFirstName =
                    import.DmdcProMatchResponse.ChFirstName;
                  local.Group.Update.DmdcProMatchResponse.ChMiddleName =
                    import.DmdcProMatchResponse.ChMiddleName;
                  local.Group.Update.DmdcProMatchResponse.ChLastName =
                    import.DmdcProMatchResponse.ChLastName;
                  local.Group.Update.DmdcProMatchResponse.ChDeathIndicator =
                    import.DmdcProMatchResponse.ChDeathIndicator;
                  local.Group.Update.DmdcProMatchResponse.
                    ChMedicalCoverageIndicator =
                      import.DmdcProMatchResponse.ChMedicalCoverageIndicator;
                  local.Group.Update.Infrastructure.DenormNumeric12 =
                    import.DmdcProMatchResponse.ChMemberId;
                  local.Group.Update.Infrastructure.EventId = 10;
                  local.Group.Update.DmdcProMatchResponse.ChMemberId =
                    import.DmdcProMatchResponse.PfMemberId;
                  local.Group.Update.Infrastructure.ReasonCode = "PFCOVENDSYS";
                  local.Group.Update.Grp2NdCode.ReasonCode = "PFCOVEND";
                  local.Group.Update.Infrastructure.CsePersonNumber =
                    local.PfNumber.CsePersonNumber ?? "";
                }

                break;
              case "OTHCOVBEGIN":
                if (ReadInfrastructure38())
                {
                  local.RecordFound.Flag = "Y";
                }

                if (AsChar(local.RecordFound.Flag) == 'Y')
                {
                  if (!Lt(entities.SecondPass.CreatedTimestamp,
                    entities.Infrastructure.CreatedTimestamp))
                  {
                    // THIS RECORD IS CLOSED SO NO NEED TO DO ANYTHING
                  }
                  else
                  {
                    // RECORD IS NOT CLOSED
                    switch(AsChar(import.DmdcProMatchResponse.
                      ChMedicalCoverageSponsorCode))
                    {
                      case '1':
                        break;
                      case '2':
                        break;
                      case '3':
                        break;
                      case '4':
                        // DO NOTHING, THIS IS THE ONE WE WANT OPEN
                        goto Test5;
                      default:
                        break;
                    }

                    ++local.Group.Index;
                    local.Group.CheckSize();

                    local.Group.Update.DmdcProMatchResponse.ChFirstName =
                      import.DmdcProMatchResponse.ChFirstName;
                    local.Group.Update.DmdcProMatchResponse.ChMiddleName =
                      import.DmdcProMatchResponse.ChMiddleName;
                    local.Group.Update.DmdcProMatchResponse.ChLastName =
                      import.DmdcProMatchResponse.ChLastName;
                    local.Group.Update.DmdcProMatchResponse.ChDeathIndicator =
                      import.DmdcProMatchResponse.ChDeathIndicator;
                    local.Group.Update.DmdcProMatchResponse.
                      ChMedicalCoverageIndicator =
                        import.DmdcProMatchResponse.ChMedicalCoverageIndicator;
                    local.Group.Update.Infrastructure.DenormNumeric12 =
                      import.DmdcProMatchResponse.ChMemberId;
                    local.Group.Update.Infrastructure.EventId = 10;
                    local.Group.Update.DmdcProMatchResponse.ChMemberId =
                      import.DmdcProMatchResponse.ChMemberId;
                    local.Group.Update.Infrastructure.ReasonCode = "OTHCOVEND";
                    local.Group.Update.Grp2NdCode.ReasonCode = "OTHCOVENDSYS";
                    local.Group.Update.Infrastructure.CsePersonNumber =
                      local.ChNumber.CsePersonNumber ?? "";
                  }
                }
                else
                {
                  switch(AsChar(import.DmdcProMatchResponse.
                    ChMedicalCoverageSponsorCode))
                  {
                    case '1':
                      break;
                    case '2':
                      break;
                    case '3':
                      break;
                    case '4':
                      // DO NOTHING, THIS IS THE ONE WE WANT OPEN
                      goto Test5;
                    default:
                      break;
                  }

                  ++local.Group.Index;
                  local.Group.CheckSize();

                  local.Group.Update.DmdcProMatchResponse.ChFirstName =
                    import.DmdcProMatchResponse.ChFirstName;
                  local.Group.Update.DmdcProMatchResponse.ChMiddleName =
                    import.DmdcProMatchResponse.ChMiddleName;
                  local.Group.Update.DmdcProMatchResponse.ChLastName =
                    import.DmdcProMatchResponse.ChLastName;
                  local.Group.Update.DmdcProMatchResponse.ChDeathIndicator =
                    import.DmdcProMatchResponse.ChDeathIndicator;
                  local.Group.Update.DmdcProMatchResponse.
                    ChMedicalCoverageIndicator =
                      import.DmdcProMatchResponse.ChMedicalCoverageIndicator;
                  local.Group.Update.Infrastructure.DenormNumeric12 =
                    import.DmdcProMatchResponse.ChMemberId;
                  local.Group.Update.Infrastructure.EventId = 10;
                  local.Group.Update.DmdcProMatchResponse.ChMemberId =
                    import.DmdcProMatchResponse.ChMemberId;
                  local.Group.Update.Infrastructure.ReasonCode = "OTHCOVEND";
                  local.Group.Update.Grp2NdCode.ReasonCode = "OTHCOVENDSYS";
                  local.Group.Update.Infrastructure.CsePersonNumber =
                    local.ChNumber.CsePersonNumber ?? "";
                }

                break;
              case "OTHCOVBEGINSYS":
                if (ReadInfrastructure39())
                {
                  local.RecordFound.Flag = "Y";
                }

                if (AsChar(local.RecordFound.Flag) == 'Y')
                {
                  if (!Lt(entities.SecondPass.CreatedTimestamp,
                    entities.Infrastructure.CreatedTimestamp))
                  {
                    // THIS RECORD IS CLOSED SO NO NEED TO DO ANYTHING
                  }
                  else
                  {
                    // RECORD IS NOT CLOSED
                    switch(AsChar(import.DmdcProMatchResponse.
                      ChMedicalCoverageSponsorCode))
                    {
                      case '1':
                        break;
                      case '2':
                        break;
                      case '3':
                        break;
                      case '4':
                        // DO NOTHING, THIS IS THE ONE WE WANT OPEN
                        goto Test5;
                      default:
                        break;
                    }

                    ++local.Group.Index;
                    local.Group.CheckSize();

                    local.Group.Update.DmdcProMatchResponse.ChFirstName =
                      import.DmdcProMatchResponse.ChFirstName;
                    local.Group.Update.DmdcProMatchResponse.ChMiddleName =
                      import.DmdcProMatchResponse.ChMiddleName;
                    local.Group.Update.DmdcProMatchResponse.ChLastName =
                      import.DmdcProMatchResponse.ChLastName;
                    local.Group.Update.DmdcProMatchResponse.ChDeathIndicator =
                      import.DmdcProMatchResponse.ChDeathIndicator;
                    local.Group.Update.DmdcProMatchResponse.
                      ChMedicalCoverageIndicator =
                        import.DmdcProMatchResponse.ChMedicalCoverageIndicator;
                    local.Group.Update.Infrastructure.DenormNumeric12 =
                      import.DmdcProMatchResponse.ChMemberId;
                    local.Group.Update.Infrastructure.EventId = 10;
                    local.Group.Update.DmdcProMatchResponse.ChMemberId =
                      import.DmdcProMatchResponse.ChMemberId;
                    local.Group.Update.Infrastructure.ReasonCode =
                      "OTHCOVENDSYS";
                    local.Group.Update.Grp2NdCode.ReasonCode = "OTHCOVEND";
                    local.Group.Update.Infrastructure.CsePersonNumber =
                      local.ChNumber.CsePersonNumber ?? "";
                  }
                }
                else
                {
                  switch(AsChar(import.DmdcProMatchResponse.
                    ChMedicalCoverageSponsorCode))
                  {
                    case '1':
                      break;
                    case '2':
                      break;
                    case '3':
                      break;
                    case '4':
                      // DO NOTHING, THIS IS THE ONE WE WANT OPEN
                      goto Test5;
                    default:
                      break;
                  }

                  ++local.Group.Index;
                  local.Group.CheckSize();

                  local.Group.Update.DmdcProMatchResponse.ChFirstName =
                    import.DmdcProMatchResponse.ChFirstName;
                  local.Group.Update.DmdcProMatchResponse.ChMiddleName =
                    import.DmdcProMatchResponse.ChMiddleName;
                  local.Group.Update.DmdcProMatchResponse.ChLastName =
                    import.DmdcProMatchResponse.ChLastName;
                  local.Group.Update.DmdcProMatchResponse.ChDeathIndicator =
                    import.DmdcProMatchResponse.ChDeathIndicator;
                  local.Group.Update.DmdcProMatchResponse.
                    ChMedicalCoverageIndicator =
                      import.DmdcProMatchResponse.ChMedicalCoverageIndicator;
                  local.Group.Update.Infrastructure.DenormNumeric12 =
                    import.DmdcProMatchResponse.ChMemberId;
                  local.Group.Update.Infrastructure.EventId = 10;
                  local.Group.Update.DmdcProMatchResponse.ChMemberId =
                    import.DmdcProMatchResponse.ChMemberId;
                  local.Group.Update.Infrastructure.ReasonCode = "OTHCOVENDSYS";
                  local.Group.Update.Grp2NdCode.ReasonCode = "OTHCOVEND";
                  local.Group.Update.Infrastructure.CsePersonNumber =
                    local.ChNumber.CsePersonNumber ?? "";
                }

                break;
              default:
                // ********************************************************************
                // REF:CQ237   Who: Raj S   When: 05/02/2008    Begin Change
                // ********************************************************************
                if (Equal(entities.Infrastructure.ReasonCode, 1, 6, "OTHCOV"))
                {
                  if (ReadInfrastructure35())
                  {
                    local.RecordFound.Flag = "Y";
                  }

                  if (AsChar(local.RecordFound.Flag) == 'Y')
                  {
                    if (!Lt(entities.SecondPass.CreatedTimestamp,
                      entities.Infrastructure.CreatedTimestamp))
                    {
                      // THIS RECORD IS CLOSED SO NO NEED TO DO ANYTHING
                      goto Test5;
                    }
                  }

                  // RECORD IS NOT CLOSED
                  switch(AsChar(import.DmdcProMatchResponse.
                    ChMedicalCoverageSponsorCode))
                  {
                    case '1':
                      break;
                    case '2':
                      break;
                    case '3':
                      break;
                    case '4':
                      // DO NOTHING, THIS IS THE ONE WE WANT OPEN
                      goto Test5;
                    default:
                      break;
                  }

                  ++local.Group.Index;
                  local.Group.CheckSize();

                  local.Group.Update.DmdcProMatchResponse.ChFirstName =
                    import.DmdcProMatchResponse.ChFirstName;
                  local.Group.Update.DmdcProMatchResponse.ChMiddleName =
                    import.DmdcProMatchResponse.ChMiddleName;
                  local.Group.Update.DmdcProMatchResponse.ChLastName =
                    import.DmdcProMatchResponse.ChLastName;
                  local.Group.Update.DmdcProMatchResponse.ChDeathIndicator =
                    import.DmdcProMatchResponse.ChDeathIndicator;
                  local.Group.Update.DmdcProMatchResponse.
                    ChMedicalCoverageIndicator =
                      import.DmdcProMatchResponse.ChMedicalCoverageIndicator;
                  local.Group.Update.Infrastructure.DenormNumeric12 =
                    import.DmdcProMatchResponse.ChMemberId;
                  local.Group.Update.Infrastructure.EventId = 10;
                  local.Group.Update.DmdcProMatchResponse.ChMemberId =
                    import.DmdcProMatchResponse.ChMemberId;

                  if (Equal(entities.SecondPass.ReasonCode, 12, 3, "SYS"))
                  {
                    local.Group.Update.Infrastructure.ReasonCode =
                      Substring(local.SelOtherEndReason.Text20,
                      WorkArea.Text20_MaxLength, 1, 8) + "ENDSYS";
                    local.Group.Update.Grp2NdCode.ReasonCode =
                      Substring(local.SelOtherEndReason.Text20,
                      WorkArea.Text20_MaxLength, 1, 8) + "END";
                  }
                  else
                  {
                    local.Group.Update.Infrastructure.ReasonCode =
                      Substring(local.SelOtherEndReason.Text20,
                      WorkArea.Text20_MaxLength, 1, 8) + "END";
                    local.Group.Update.Grp2NdCode.ReasonCode =
                      Substring(local.SelOtherEndReason.Text20,
                      WorkArea.Text20_MaxLength, 1, 8) + "ENDSYS";
                  }

                  local.Group.Update.Infrastructure.CsePersonNumber =
                    local.ChNumber.CsePersonNumber ?? "";
                }

                // ********************************************************************
                // REF:CQ237   Who: Raj S   When: 05/02/2008    End Change
                // ********************************************************************
                break;
            }
          }

Test5:

          ++local.Group.Index;
          local.Group.CheckSize();

          local.Group.Update.DmdcProMatchResponse.ChFirstName =
            import.DmdcProMatchResponse.ChFirstName;
          local.Group.Update.DmdcProMatchResponse.ChMiddleName =
            import.DmdcProMatchResponse.ChMiddleName;
          local.Group.Update.DmdcProMatchResponse.ChLastName =
            import.DmdcProMatchResponse.ChLastName;
          local.Group.Update.DmdcProMatchResponse.ChDeathIndicator =
            import.DmdcProMatchResponse.ChDeathIndicator;
          local.Group.Update.DmdcProMatchResponse.ChMedicalCoverageIndicator =
            import.DmdcProMatchResponse.ChMedicalCoverageIndicator;
          local.Group.Update.Infrastructure.EventId = 10;
          local.Group.Update.Infrastructure.DenormDate = Now().Date;

          switch(AsChar(import.DmdcProMatchResponse.ChMedicalCoverageSponsorCode))
            
          {
            case '1':
              if (AsChar(local.ApInactive.Flag) == 'Y' || AsChar
                (local.ChildInactive.Flag) == 'Y')
              {
                local.Group.Update.Infrastructure.ReasonCode = "NCPCOVBEGINSYS";
                local.Group.Update.Grp2NdCode.ReasonCode = "NCPCOVBEGIN";
              }
              else
              {
                local.Group.Update.Infrastructure.ReasonCode = "NCPCOVBEGIN";
                local.Group.Update.Grp2NdCode.ReasonCode = "NCPCOVBEGINSYS";
              }

              local.Group.Update.Infrastructure.CsePersonNumber =
                local.ApNumber.CsePersonNumber ?? "";
              local.Group.Update.Infrastructure.DenormNumeric12 =
                import.DmdcProMatchResponse.ChMemberId;
              local.Group.Update.DmdcProMatchResponse.ChMemberId =
                import.DmdcProMatchResponse.NcpMemberId;

              break;
            case '2':
              local.Group.Update.Infrastructure.ReasonCode = "CPCOVBEGIN";
              local.Group.Update.DmdcProMatchResponse.ChMemberId =
                import.DmdcProMatchResponse.CpMemberId;
              local.Group.Update.Infrastructure.DenormNumeric12 =
                import.DmdcProMatchResponse.ChMemberId;
              local.Group.Update.Infrastructure.CsePersonNumber =
                local.ArNumber.CsePersonNumber ?? "";

              break;
            case '3':
              if (AsChar(local.PfInactive.Flag) == 'Y' || AsChar
                (local.ChildInactive.Flag) == 'Y')
              {
                local.Group.Update.Infrastructure.ReasonCode = "PFCOVBEGINSYS";
                local.Group.Update.Grp2NdCode.ReasonCode = "PFCOVBEGIN";
              }
              else
              {
                local.Group.Update.Infrastructure.ReasonCode = "PFCOVBEGIN";
                local.Group.Update.Grp2NdCode.ReasonCode = "PFCOVBEGINSYS";
              }

              local.Group.Update.DmdcProMatchResponse.ChMemberId =
                import.DmdcProMatchResponse.PfMemberId;
              local.Group.Update.Infrastructure.DenormNumeric12 =
                import.DmdcProMatchResponse.ChMemberId;
              local.Group.Update.Infrastructure.CsePersonNumber =
                local.PfNumber.CsePersonNumber ?? "";

              break;
            case '4':
              // ********************************************************************
              // REF:CQ237   Who: Raj S   When: 05/02/2008    Begin Change
              // ********************************************************************
              if (AsChar(import.DmdcProMatchResponse.ChSponsorRelCode) > '0'
                && AsChar(import.DmdcProMatchResponse.ChSponsorRelCode) <= '7')
              {
                local.OtherCovChildRelList.Index =
                  (int)(StringToNumber(
                    import.DmdcProMatchResponse.ChSponsorRelCode) - 1);
                local.OtherCovChildRelList.CheckSize();

                local.SelOthcovChldRelShrt.Text2 =
                  local.OtherCovChildRelList.Item.OtherCovChldRelShort.Text2;
                local.SelOthcovChildRelation.Text14 =
                  local.OtherCovChildRelList.Item.OtherCovChildRelation.Text14;

                if (AsChar(local.ChildInactive.Flag) == 'Y')
                {
                  local.Group.Update.Infrastructure.ReasonCode =
                    TrimEnd(local.SelOthcovChildRelation.Text14) + "BEGSYS";
                  local.Group.Update.Grp2NdCode.ReasonCode =
                    TrimEnd(local.SelOthcovChildRelation.Text14) + "BEG";
                }
                else
                {
                  local.Group.Update.Infrastructure.ReasonCode =
                    TrimEnd(local.SelOthcovChildRelation.Text14) + "BEG";
                  local.Group.Update.Grp2NdCode.ReasonCode =
                    TrimEnd(local.SelOthcovChildRelation.Text14) + "BEGSYS";
                }
              }
              else if (AsChar(local.ChildInactive.Flag) == 'Y' || AsChar
                (local.ChildInactive.Flag) == 'Y')
              {
                local.Group.Update.Infrastructure.ReasonCode = "OTHCOVBEGINSYS";
                local.Group.Update.Grp2NdCode.ReasonCode = "OTHCOVBEGIN";
              }
              else
              {
                local.Group.Update.Infrastructure.ReasonCode = "OTHCOVBEGIN";
                local.Group.Update.Grp2NdCode.ReasonCode = "OTHCOVBEGINSYS";
              }

              // ********************************************************************
              // REF:CQ237   Who: Raj S   When: 05/02/2008    End Change
              // ********************************************************************
              local.Group.Update.DmdcProMatchResponse.ChMemberId =
                import.DmdcProMatchResponse.ChMemberId;
              local.Group.Update.Infrastructure.DenormNumeric12 =
                import.DmdcProMatchResponse.ChMemberId;
              local.Group.Update.Infrastructure.CsePersonNumber =
                local.ChNumber.CsePersonNumber ?? "";

              break;
            default:
              break;
          }
        }
        else if (AsChar(local.RecordFound.Flag) == 'Y')
        {
          local.RecordFound.Flag = "";

          switch(TrimEnd(entities.Infrastructure.ReasonCode))
          {
            case "NCPCOVBEGIN":
              local.RecordFound.Flag = "";

              if (ReadInfrastructure36())
              {
                local.RecordFound.Flag = "Y";
              }

              if (AsChar(local.RecordFound.Flag) == 'Y')
              {
                if (!Lt(entities.SecondPass.CreatedTimestamp,
                  entities.Infrastructure.CreatedTimestamp))
                {
                  // THIS RECORD IS CLOSED SO NO NEED TO DO ANYTHING
                }
                else
                {
                  // RECORD IS NOT CLOSED IT COULD BE A FUTURE RECORD, BUT WE
                  // WANT TO CLOSE IT NOW INSTEAD IN THE FUTURE
                  ++local.Group.Index;
                  local.Group.CheckSize();

                  local.Group.Update.DmdcProMatchResponse.ChFirstName =
                    import.DmdcProMatchResponse.ChFirstName;
                  local.Group.Update.DmdcProMatchResponse.ChMiddleName =
                    import.DmdcProMatchResponse.ChMiddleName;
                  local.Group.Update.DmdcProMatchResponse.ChLastName =
                    import.DmdcProMatchResponse.ChLastName;
                  local.Group.Update.DmdcProMatchResponse.ChDeathIndicator =
                    import.DmdcProMatchResponse.ChDeathIndicator;
                  local.Group.Update.DmdcProMatchResponse.
                    ChMedicalCoverageIndicator =
                      import.DmdcProMatchResponse.ChMedicalCoverageIndicator;
                  local.Group.Update.Infrastructure.DenormNumeric12 =
                    import.DmdcProMatchResponse.ChMemberId;
                  local.Group.Update.Infrastructure.EventId = 10;
                  local.Group.Update.DmdcProMatchResponse.ChMemberId =
                    import.DmdcProMatchResponse.NcpMemberId;
                  local.Group.Update.Infrastructure.ReasonCode = "NCPCOVEND";
                  local.Group.Update.Grp2NdCode.ReasonCode = "NCPCOVENDSYS";
                  local.Group.Update.Infrastructure.CsePersonNumber =
                    local.ApNumber.CsePersonNumber ?? "";
                }
              }
              else
              {
                // CLOSE THE OPEN RECORD
                ++local.Group.Index;
                local.Group.CheckSize();

                local.Group.Update.DmdcProMatchResponse.ChFirstName =
                  import.DmdcProMatchResponse.ChFirstName;
                local.Group.Update.DmdcProMatchResponse.ChMiddleName =
                  import.DmdcProMatchResponse.ChMiddleName;
                local.Group.Update.DmdcProMatchResponse.ChLastName =
                  import.DmdcProMatchResponse.ChLastName;
                local.Group.Update.DmdcProMatchResponse.ChDeathIndicator =
                  import.DmdcProMatchResponse.ChDeathIndicator;
                local.Group.Update.DmdcProMatchResponse.
                  ChMedicalCoverageIndicator =
                    import.DmdcProMatchResponse.ChMedicalCoverageIndicator;
                local.Group.Update.Infrastructure.DenormNumeric12 =
                  import.DmdcProMatchResponse.ChMemberId;
                local.Group.Update.Infrastructure.EventId = 10;
                local.Group.Update.DmdcProMatchResponse.ChMemberId =
                  import.DmdcProMatchResponse.NcpMemberId;
                local.Group.Update.Infrastructure.ReasonCode = "NCPCOVEND";
                local.Group.Update.Grp2NdCode.ReasonCode = "NCPCOVENDSYS";
                local.Group.Update.Infrastructure.CsePersonNumber =
                  local.ApNumber.CsePersonNumber ?? "";
              }

              break;
            case "NCPCOVBEGINSYS":
              local.RecordFound.Flag = "";

              if (ReadInfrastructure37())
              {
                local.RecordFound.Flag = "Y";
              }

              if (AsChar(local.RecordFound.Flag) == 'Y')
              {
                if (!Lt(entities.SecondPass.CreatedTimestamp,
                  entities.Infrastructure.CreatedTimestamp))
                {
                  // THIS RECORD IS CLOSED SO NO NEED TO DO ANYTHING
                }
                else
                {
                  // RECORD IS NOT CLOSED IT COULD BE A FUTURE RECORD, BUT WE
                  // WANT TO CLOSE IT NOW INSTEAD IN THE FUTURE
                  ++local.Group.Index;
                  local.Group.CheckSize();

                  local.Group.Update.DmdcProMatchResponse.ChFirstName =
                    import.DmdcProMatchResponse.ChFirstName;
                  local.Group.Update.DmdcProMatchResponse.ChMiddleName =
                    import.DmdcProMatchResponse.ChMiddleName;
                  local.Group.Update.DmdcProMatchResponse.ChLastName =
                    import.DmdcProMatchResponse.ChLastName;
                  local.Group.Update.DmdcProMatchResponse.ChDeathIndicator =
                    import.DmdcProMatchResponse.ChDeathIndicator;
                  local.Group.Update.DmdcProMatchResponse.
                    ChMedicalCoverageIndicator =
                      import.DmdcProMatchResponse.ChMedicalCoverageIndicator;
                  local.Group.Update.Infrastructure.DenormNumeric12 =
                    import.DmdcProMatchResponse.ChMemberId;
                  local.Group.Update.Infrastructure.EventId = 10;
                  local.Group.Update.DmdcProMatchResponse.ChMemberId =
                    import.DmdcProMatchResponse.NcpMemberId;
                  local.Group.Update.Infrastructure.ReasonCode = "NCPCOVENDSYS";
                  local.Group.Update.Grp2NdCode.ReasonCode = "NCPCOVEND";
                  local.Group.Update.Infrastructure.CsePersonNumber =
                    local.ApNumber.CsePersonNumber ?? "";
                }
              }
              else
              {
                // CLOSE THE OPEN RECORD
                ++local.Group.Index;
                local.Group.CheckSize();

                local.Group.Update.DmdcProMatchResponse.ChFirstName =
                  import.DmdcProMatchResponse.ChFirstName;
                local.Group.Update.DmdcProMatchResponse.ChMiddleName =
                  import.DmdcProMatchResponse.ChMiddleName;
                local.Group.Update.DmdcProMatchResponse.ChLastName =
                  import.DmdcProMatchResponse.ChLastName;
                local.Group.Update.DmdcProMatchResponse.ChDeathIndicator =
                  import.DmdcProMatchResponse.ChDeathIndicator;
                local.Group.Update.DmdcProMatchResponse.
                  ChMedicalCoverageIndicator =
                    import.DmdcProMatchResponse.ChMedicalCoverageIndicator;
                local.Group.Update.Infrastructure.DenormNumeric12 =
                  import.DmdcProMatchResponse.ChMemberId;
                local.Group.Update.Infrastructure.EventId = 10;
                local.Group.Update.DmdcProMatchResponse.ChMemberId =
                  import.DmdcProMatchResponse.NcpMemberId;
                local.Group.Update.Infrastructure.ReasonCode = "NCPCOVENDSYS";
                local.Group.Update.Grp2NdCode.ReasonCode = "NCPCOVEND";
                local.Group.Update.Infrastructure.CsePersonNumber =
                  local.ApNumber.CsePersonNumber ?? "";
              }

              break;
            case "CPCOVBEGIN":
              local.WorkArea.Text15 =
                NumberToString(import.DmdcProMatchResponse.CpMemberId, 15);
              local.SecondRead.Number = Substring(local.WorkArea.Text15, 6, 10);
              local.RecordFound.Flag = "";

              if (ReadInfrastructure40())
              {
                local.RecordFound.Flag = "Y";
              }

              if (AsChar(local.RecordFound.Flag) == 'Y')
              {
                if (!Lt(entities.SecondPass.CreatedTimestamp,
                  entities.Infrastructure.CreatedTimestamp))
                {
                  // THIS RECORD IS CLOSED SO NO NEED TO DO ANYTHING
                }
                else
                {
                  // RECORD IS NOT CLOSED IT COULD BE A FUTURE RECORD, BUT WE
                  // WANT TO CLOSE IT NOW INSTEAD IN THE FUTURE
                  ++local.Group.Index;
                  local.Group.CheckSize();

                  local.Group.Update.DmdcProMatchResponse.ChFirstName =
                    import.DmdcProMatchResponse.ChFirstName;
                  local.Group.Update.DmdcProMatchResponse.ChMiddleName =
                    import.DmdcProMatchResponse.ChMiddleName;
                  local.Group.Update.DmdcProMatchResponse.ChLastName =
                    import.DmdcProMatchResponse.ChLastName;
                  local.Group.Update.DmdcProMatchResponse.ChDeathIndicator =
                    import.DmdcProMatchResponse.ChDeathIndicator;
                  local.Group.Update.DmdcProMatchResponse.
                    ChMedicalCoverageIndicator =
                      import.DmdcProMatchResponse.ChMedicalCoverageIndicator;
                  local.Group.Update.Infrastructure.DenormNumeric12 =
                    import.DmdcProMatchResponse.ChMemberId;
                  local.Group.Update.Infrastructure.EventId = 10;
                  local.Group.Update.Infrastructure.ReasonCode = "CPCOVEND";
                  local.Group.Update.DmdcProMatchResponse.ChMemberId =
                    import.DmdcProMatchResponse.CpMemberId;
                  local.Group.Update.Infrastructure.CsePersonNumber =
                    local.ArNumber.CsePersonNumber ?? "";
                }
              }
              else
              {
                // CLOSE THE OPEN RECORD
                ++local.Group.Index;
                local.Group.CheckSize();

                local.Group.Update.DmdcProMatchResponse.ChFirstName =
                  import.DmdcProMatchResponse.ChFirstName;
                local.Group.Update.DmdcProMatchResponse.ChMiddleName =
                  import.DmdcProMatchResponse.ChMiddleName;
                local.Group.Update.DmdcProMatchResponse.ChLastName =
                  import.DmdcProMatchResponse.ChLastName;
                local.Group.Update.DmdcProMatchResponse.ChDeathIndicator =
                  import.DmdcProMatchResponse.ChDeathIndicator;
                local.Group.Update.DmdcProMatchResponse.
                  ChMedicalCoverageIndicator =
                    import.DmdcProMatchResponse.ChMedicalCoverageIndicator;
                local.Group.Update.Infrastructure.DenormNumeric12 =
                  import.DmdcProMatchResponse.ChMemberId;
                local.Group.Update.Infrastructure.EventId = 10;
                local.Group.Update.Infrastructure.ReasonCode = "CPCOVEND";
                local.Group.Update.DmdcProMatchResponse.ChMemberId =
                  import.DmdcProMatchResponse.CpMemberId;
                local.Group.Update.Infrastructure.CsePersonNumber =
                  local.ArNumber.CsePersonNumber ?? "";
              }

              break;
            case "PFCOVBEGIN":
              local.WorkArea.Text15 =
                NumberToString(import.DmdcProMatchResponse.PfMemberId, 15);
              local.SecondRead.Number = Substring(local.WorkArea.Text15, 6, 10);
              local.RecordFound.Flag = "";

              if (ReadInfrastructure44())
              {
                local.RecordFound.Flag = "Y";
              }

              if (AsChar(local.RecordFound.Flag) == 'Y')
              {
                if (!Lt(entities.SecondPass.CreatedTimestamp,
                  entities.Infrastructure.CreatedTimestamp))
                {
                  // THIS RECORD IS CLOSED SO NO NEED TO DO ANYTHING
                }
                else
                {
                  // RECORD IS NOT CLOSED IT COULD BE A FUTURE RECORD, BUT WE
                  // WANT TO CLOSE IT NOW INSTEAD IN THE FUTURE
                  ++local.Group.Index;
                  local.Group.CheckSize();

                  local.Group.Update.DmdcProMatchResponse.ChFirstName =
                    import.DmdcProMatchResponse.ChFirstName;
                  local.Group.Update.DmdcProMatchResponse.ChMiddleName =
                    import.DmdcProMatchResponse.ChMiddleName;
                  local.Group.Update.DmdcProMatchResponse.ChLastName =
                    import.DmdcProMatchResponse.ChLastName;
                  local.Group.Update.DmdcProMatchResponse.ChDeathIndicator =
                    import.DmdcProMatchResponse.ChDeathIndicator;
                  local.Group.Update.DmdcProMatchResponse.
                    ChMedicalCoverageIndicator =
                      import.DmdcProMatchResponse.ChMedicalCoverageIndicator;
                  local.Group.Update.Infrastructure.DenormNumeric12 =
                    import.DmdcProMatchResponse.ChMemberId;
                  local.Group.Update.Infrastructure.EventId = 10;
                  local.Group.Update.DmdcProMatchResponse.ChMemberId =
                    import.DmdcProMatchResponse.PfMemberId;
                  local.Group.Update.Infrastructure.ReasonCode = "PFCOVEND";
                  local.Group.Update.Grp2NdCode.ReasonCode = "PFCOVENDSYS";
                  local.Group.Update.Infrastructure.CsePersonNumber =
                    local.PfNumber.CsePersonNumber ?? "";
                }
              }
              else
              {
                // CLOSE THE OPEN RECORD
                ++local.Group.Index;
                local.Group.CheckSize();

                local.Group.Update.DmdcProMatchResponse.ChFirstName =
                  import.DmdcProMatchResponse.ChFirstName;
                local.Group.Update.DmdcProMatchResponse.ChMiddleName =
                  import.DmdcProMatchResponse.ChMiddleName;
                local.Group.Update.DmdcProMatchResponse.ChLastName =
                  import.DmdcProMatchResponse.ChLastName;
                local.Group.Update.DmdcProMatchResponse.ChDeathIndicator =
                  import.DmdcProMatchResponse.ChDeathIndicator;
                local.Group.Update.DmdcProMatchResponse.
                  ChMedicalCoverageIndicator =
                    import.DmdcProMatchResponse.ChMedicalCoverageIndicator;
                local.Group.Update.Infrastructure.DenormNumeric12 =
                  import.DmdcProMatchResponse.ChMemberId;
                local.Group.Update.Infrastructure.EventId = 10;
                local.Group.Update.DmdcProMatchResponse.ChMemberId =
                  import.DmdcProMatchResponse.PfMemberId;
                local.Group.Update.Infrastructure.ReasonCode = "PFCOVEND";
                local.Group.Update.Grp2NdCode.ReasonCode = "PFCOVENDSYS";
                local.Group.Update.Infrastructure.CsePersonNumber =
                  local.PfNumber.CsePersonNumber ?? "";
              }

              break;
            case "PFCOVBEGINSYS":
              local.WorkArea.Text15 =
                NumberToString(import.DmdcProMatchResponse.PfMemberId, 15);
              local.SecondRead.Number = Substring(local.WorkArea.Text15, 6, 10);
              local.RecordFound.Flag = "";

              if (ReadInfrastructure45())
              {
                local.RecordFound.Flag = "Y";
              }

              if (AsChar(local.RecordFound.Flag) == 'Y')
              {
                if (!Lt(entities.SecondPass.CreatedTimestamp,
                  entities.Infrastructure.CreatedTimestamp))
                {
                  // THIS RECORD IS CLOSED SO NO NEED TO DO ANYTHING
                }
                else
                {
                  // RECORD IS NOT CLOSED IT COULD BE A FUTURE RECORD, BUT WE
                  // WANT TO CLOSE IT NOW INSTEAD IN THE FUTURE
                  ++local.Group.Index;
                  local.Group.CheckSize();

                  local.Group.Update.DmdcProMatchResponse.ChFirstName =
                    import.DmdcProMatchResponse.ChFirstName;
                  local.Group.Update.DmdcProMatchResponse.ChMiddleName =
                    import.DmdcProMatchResponse.ChMiddleName;
                  local.Group.Update.DmdcProMatchResponse.ChLastName =
                    import.DmdcProMatchResponse.ChLastName;
                  local.Group.Update.DmdcProMatchResponse.ChDeathIndicator =
                    import.DmdcProMatchResponse.ChDeathIndicator;
                  local.Group.Update.DmdcProMatchResponse.
                    ChMedicalCoverageIndicator =
                      import.DmdcProMatchResponse.ChMedicalCoverageIndicator;
                  local.Group.Update.Infrastructure.DenormNumeric12 =
                    import.DmdcProMatchResponse.ChMemberId;
                  local.Group.Update.Infrastructure.EventId = 10;
                  local.Group.Update.DmdcProMatchResponse.ChMemberId =
                    import.DmdcProMatchResponse.PfMemberId;
                  local.Group.Update.Infrastructure.ReasonCode = "PFCOVENDSYS";
                  local.Group.Update.Grp2NdCode.ReasonCode = "PFCOVEND";
                  local.Group.Update.Infrastructure.CsePersonNumber =
                    local.PfNumber.CsePersonNumber ?? "";
                }
              }
              else
              {
                // CLOSE THE OPEN RECORD
                ++local.Group.Index;
                local.Group.CheckSize();

                local.Group.Update.DmdcProMatchResponse.ChFirstName =
                  import.DmdcProMatchResponse.ChFirstName;
                local.Group.Update.DmdcProMatchResponse.ChMiddleName =
                  import.DmdcProMatchResponse.ChMiddleName;
                local.Group.Update.DmdcProMatchResponse.ChLastName =
                  import.DmdcProMatchResponse.ChLastName;
                local.Group.Update.DmdcProMatchResponse.ChDeathIndicator =
                  import.DmdcProMatchResponse.ChDeathIndicator;
                local.Group.Update.DmdcProMatchResponse.
                  ChMedicalCoverageIndicator =
                    import.DmdcProMatchResponse.ChMedicalCoverageIndicator;
                local.Group.Update.Infrastructure.DenormNumeric12 =
                  import.DmdcProMatchResponse.ChMemberId;
                local.Group.Update.Infrastructure.EventId = 10;
                local.Group.Update.DmdcProMatchResponse.ChMemberId =
                  import.DmdcProMatchResponse.PfMemberId;
                local.Group.Update.Infrastructure.ReasonCode = "PFCOVENDSYS";
                local.Group.Update.Grp2NdCode.ReasonCode = "PFCOVEND";
                local.Group.Update.Infrastructure.CsePersonNumber =
                  local.PfNumber.CsePersonNumber ?? "";
              }

              break;
            case "OTHCOVBEGIN":
              local.RecordFound.Flag = "";

              if (ReadInfrastructure38())
              {
                local.RecordFound.Flag = "Y";
              }

              if (AsChar(local.RecordFound.Flag) == 'Y')
              {
                if (!Lt(entities.SecondPass.CreatedTimestamp,
                  entities.Infrastructure.CreatedTimestamp))
                {
                  // THIS RECORD IS CLOSED SO NO NEED TO DO ANYTHING
                }
                else
                {
                  // RECORD IS NOT CLOSED IT COULD BE A FUTURE RECORD, BUT WE
                  // WANT TO CLOSE IT NOW INSTEAD IN THE FUTURE
                  ++local.Group.Index;
                  local.Group.CheckSize();

                  local.Group.Update.DmdcProMatchResponse.ChFirstName =
                    import.DmdcProMatchResponse.ChFirstName;
                  local.Group.Update.DmdcProMatchResponse.ChMiddleName =
                    import.DmdcProMatchResponse.ChMiddleName;
                  local.Group.Update.DmdcProMatchResponse.ChLastName =
                    import.DmdcProMatchResponse.ChLastName;
                  local.Group.Update.DmdcProMatchResponse.ChDeathIndicator =
                    import.DmdcProMatchResponse.ChDeathIndicator;
                  local.Group.Update.DmdcProMatchResponse.
                    ChMedicalCoverageIndicator =
                      import.DmdcProMatchResponse.ChMedicalCoverageIndicator;
                  local.Group.Update.Infrastructure.DenormNumeric12 =
                    import.DmdcProMatchResponse.ChMemberId;
                  local.Group.Update.Infrastructure.EventId = 10;
                  local.Group.Update.DmdcProMatchResponse.ChMemberId =
                    import.DmdcProMatchResponse.ChMemberId;
                  local.Group.Update.Infrastructure.ReasonCode = "OTHCOVEND";
                  local.Group.Update.Grp2NdCode.ReasonCode = "OTHCOVENDSYS";
                  local.Group.Update.Infrastructure.CsePersonNumber =
                    local.ChNumber.CsePersonNumber ?? "";
                }
              }
              else
              {
                // CLOSE THE OPEN RECORD
                ++local.Group.Index;
                local.Group.CheckSize();

                local.Group.Update.DmdcProMatchResponse.ChFirstName =
                  import.DmdcProMatchResponse.ChFirstName;
                local.Group.Update.DmdcProMatchResponse.ChMiddleName =
                  import.DmdcProMatchResponse.ChMiddleName;
                local.Group.Update.DmdcProMatchResponse.ChLastName =
                  import.DmdcProMatchResponse.ChLastName;
                local.Group.Update.DmdcProMatchResponse.ChDeathIndicator =
                  import.DmdcProMatchResponse.ChDeathIndicator;
                local.Group.Update.DmdcProMatchResponse.
                  ChMedicalCoverageIndicator =
                    import.DmdcProMatchResponse.ChMedicalCoverageIndicator;
                local.Group.Update.Infrastructure.DenormNumeric12 =
                  import.DmdcProMatchResponse.ChMemberId;
                local.Group.Update.Infrastructure.EventId = 10;
                local.Group.Update.DmdcProMatchResponse.ChMemberId =
                  import.DmdcProMatchResponse.ChMemberId;
                local.Group.Update.Infrastructure.ReasonCode = "OTHCOVEND";
                local.Group.Update.Grp2NdCode.ReasonCode = "OTHCOVENDSYS";
                local.Group.Update.Infrastructure.CsePersonNumber =
                  local.ChNumber.CsePersonNumber ?? "";
              }

              break;
            case "OTHCOVBEGINSYS":
              local.RecordFound.Flag = "";

              if (ReadInfrastructure39())
              {
                local.RecordFound.Flag = "Y";
              }

              if (AsChar(local.RecordFound.Flag) == 'Y')
              {
                if (!Lt(entities.SecondPass.CreatedTimestamp,
                  entities.Infrastructure.CreatedTimestamp))
                {
                  // THIS RECORD IS CLOSED SO NO NEED TO DO ANYTHING
                }
                else
                {
                  // RECORD IS NOT CLOSED IT COULD BE A FUTURE RECORD, BUT WE
                  // WANT TO CLOSE IT NOW INSTEAD IN THE FUTURE
                  ++local.Group.Index;
                  local.Group.CheckSize();

                  local.Group.Update.DmdcProMatchResponse.ChFirstName =
                    import.DmdcProMatchResponse.ChFirstName;
                  local.Group.Update.DmdcProMatchResponse.ChMiddleName =
                    import.DmdcProMatchResponse.ChMiddleName;
                  local.Group.Update.DmdcProMatchResponse.ChLastName =
                    import.DmdcProMatchResponse.ChLastName;
                  local.Group.Update.DmdcProMatchResponse.ChDeathIndicator =
                    import.DmdcProMatchResponse.ChDeathIndicator;
                  local.Group.Update.DmdcProMatchResponse.
                    ChMedicalCoverageIndicator =
                      import.DmdcProMatchResponse.ChMedicalCoverageIndicator;
                  local.Group.Update.Infrastructure.DenormNumeric12 =
                    import.DmdcProMatchResponse.ChMemberId;
                  local.Group.Update.Infrastructure.EventId = 10;
                  local.Group.Update.DmdcProMatchResponse.ChMemberId =
                    import.DmdcProMatchResponse.ChMemberId;
                  local.Group.Update.Infrastructure.ReasonCode = "OTHCOVENDSYS";
                  local.Group.Update.Grp2NdCode.ReasonCode = "OTHCOVEND";
                  local.Group.Update.Infrastructure.CsePersonNumber =
                    local.ChNumber.CsePersonNumber ?? "";
                }
              }
              else
              {
                // CLOSE THE OPEN RECORD
                ++local.Group.Index;
                local.Group.CheckSize();

                local.Group.Update.DmdcProMatchResponse.ChFirstName =
                  import.DmdcProMatchResponse.ChFirstName;
                local.Group.Update.DmdcProMatchResponse.ChMiddleName =
                  import.DmdcProMatchResponse.ChMiddleName;
                local.Group.Update.DmdcProMatchResponse.ChLastName =
                  import.DmdcProMatchResponse.ChLastName;
                local.Group.Update.DmdcProMatchResponse.ChDeathIndicator =
                  import.DmdcProMatchResponse.ChDeathIndicator;
                local.Group.Update.DmdcProMatchResponse.
                  ChMedicalCoverageIndicator =
                    import.DmdcProMatchResponse.ChMedicalCoverageIndicator;
                local.Group.Update.Infrastructure.DenormNumeric12 =
                  import.DmdcProMatchResponse.ChMemberId;
                local.Group.Update.Infrastructure.EventId = 10;
                local.Group.Update.DmdcProMatchResponse.ChMemberId =
                  import.DmdcProMatchResponse.ChMemberId;
                local.Group.Update.Infrastructure.ReasonCode = "OTHCOVENDSYS";
                local.Group.Update.Grp2NdCode.ReasonCode = "OTHCOVEND";
                local.Group.Update.Infrastructure.CsePersonNumber =
                  local.ChNumber.CsePersonNumber ?? "";
              }

              break;
            default:
              // ********************************************************************
              // REF:CQ237   Who: Raj S   When: 05/02/2008    Begin Change
              // ********************************************************************
              if (Equal(entities.Infrastructure.ReasonCode, 1, 6, "OTHCOV"))
              {
                if (ReadInfrastructure35())
                {
                  local.RecordFound.Flag = "Y";
                }

                if (AsChar(local.RecordFound.Flag) == 'Y')
                {
                  if (!Lt(entities.SecondPass.CreatedTimestamp,
                    entities.Infrastructure.CreatedTimestamp))
                  {
                    // THIS RECORD IS CLOSED SO NO NEED TO DO ANYTHING
                    goto Test6;
                  }
                }

                // RECORD IS NOT CLOSED
                ++local.Group.Index;
                local.Group.CheckSize();

                local.Group.Update.DmdcProMatchResponse.ChFirstName =
                  import.DmdcProMatchResponse.ChFirstName;
                local.Group.Update.DmdcProMatchResponse.ChMiddleName =
                  import.DmdcProMatchResponse.ChMiddleName;
                local.Group.Update.DmdcProMatchResponse.ChLastName =
                  import.DmdcProMatchResponse.ChLastName;
                local.Group.Update.DmdcProMatchResponse.ChDeathIndicator =
                  import.DmdcProMatchResponse.ChDeathIndicator;
                local.Group.Update.DmdcProMatchResponse.
                  ChMedicalCoverageIndicator =
                    import.DmdcProMatchResponse.ChMedicalCoverageIndicator;
                local.Group.Update.Infrastructure.DenormNumeric12 =
                  import.DmdcProMatchResponse.ChMemberId;
                local.Group.Update.Infrastructure.EventId = 10;
                local.Group.Update.DmdcProMatchResponse.ChMemberId =
                  import.DmdcProMatchResponse.ChMemberId;

                if (Equal(entities.SecondPass.ReasonCode, 12, 3, "SYS"))
                {
                  local.Group.Update.Infrastructure.ReasonCode =
                    Substring(local.SelOtherEndReason.Text20,
                    WorkArea.Text20_MaxLength, 1, 8) + "ENDSYS";
                  local.Group.Update.Grp2NdCode.ReasonCode =
                    Substring(local.SelOtherEndReason.Text20,
                    WorkArea.Text20_MaxLength, 1, 8) + "END";
                }
                else
                {
                  local.Group.Update.Infrastructure.ReasonCode =
                    Substring(local.SelOtherEndReason.Text20,
                    WorkArea.Text20_MaxLength, 1, 8) + "END";
                  local.Group.Update.Grp2NdCode.ReasonCode =
                    Substring(local.SelOtherEndReason.Text20,
                    WorkArea.Text20_MaxLength, 1, 8) + "ENDSYS";
                }

                local.Group.Update.Infrastructure.CsePersonNumber =
                  local.ChNumber.CsePersonNumber ?? "";
              }

              // ********************************************************************
              // REF:CQ237   Who: Raj S   When: 05/02/2008    End Change
              // ********************************************************************
              break;
          }
        }
        else
        {
          // NO BEGINNING REASON CODE FOUND BUT WE WILL CREATE A ENDING REASON 
          // CODE SO THAT WE CAN DOUBLE CHECK OURSELEVES LATER ON
          ++local.Group.Index;
          local.Group.CheckSize();

          local.Group.Update.DmdcProMatchResponse.ChFirstName =
            import.DmdcProMatchResponse.ChFirstName;
          local.Group.Update.DmdcProMatchResponse.ChMiddleName =
            import.DmdcProMatchResponse.ChMiddleName;
          local.Group.Update.DmdcProMatchResponse.ChLastName =
            import.DmdcProMatchResponse.ChLastName;
          local.Group.Update.DmdcProMatchResponse.ChDeathIndicator =
            import.DmdcProMatchResponse.ChDeathIndicator;
          local.Group.Update.DmdcProMatchResponse.ChMedicalCoverageIndicator =
            import.DmdcProMatchResponse.ChMedicalCoverageIndicator;
          local.Group.Update.Infrastructure.EventId = 10;
          local.Group.Update.Infrastructure.DenormDate = Now().Date;

          switch(AsChar(import.DmdcProMatchResponse.ChMedicalCoverageSponsorCode))
            
          {
            case '1':
              if (AsChar(local.ApInactive.Flag) == 'Y' || AsChar
                (local.ChildInactive.Flag) == 'Y')
              {
                local.Group.Update.Infrastructure.ReasonCode = "NCPCOVENDSYS";
                local.Group.Update.Grp2NdCode.ReasonCode = "NCPCOVEND";
              }
              else
              {
                local.Group.Update.Infrastructure.ReasonCode = "NCPCOVEND";
                local.Group.Update.Grp2NdCode.ReasonCode = "NCPCOVENDSYS";
              }

              local.Group.Update.Infrastructure.DenormNumeric12 =
                import.DmdcProMatchResponse.ChMemberId;
              local.Group.Update.DmdcProMatchResponse.ChMemberId =
                import.DmdcProMatchResponse.NcpMemberId;
              local.Group.Update.Infrastructure.CsePersonNumber =
                local.ApNumber.CsePersonNumber ?? "";

              break;
            case '2':
              local.Group.Update.Infrastructure.ReasonCode = "CPCOVEND";
              local.Group.Update.Infrastructure.DenormNumeric12 =
                import.DmdcProMatchResponse.ChMemberId;
              local.Group.Update.DmdcProMatchResponse.ChMemberId =
                import.DmdcProMatchResponse.CpMemberId;
              local.Group.Update.Infrastructure.CsePersonNumber =
                local.ArNumber.CsePersonNumber ?? "";

              break;
            case '3':
              if (AsChar(local.ApInactive.Flag) == 'Y' || AsChar
                (local.ChildInactive.Flag) == 'Y')
              {
                local.Group.Update.Infrastructure.ReasonCode = "PFCOVENDSYS";
                local.Group.Update.Grp2NdCode.ReasonCode = "PFCOVEND";
              }
              else
              {
                local.Group.Update.Infrastructure.ReasonCode = "PFCOVEND";
                local.Group.Update.Grp2NdCode.ReasonCode = "PFCOVENDSYS";
              }

              local.Group.Update.Infrastructure.DenormNumeric12 =
                import.DmdcProMatchResponse.ChMemberId;
              local.Group.Update.DmdcProMatchResponse.ChMemberId =
                import.DmdcProMatchResponse.PfMemberId;
              local.Group.Update.Infrastructure.CsePersonNumber =
                local.PfNumber.CsePersonNumber ?? "";

              break;
            case '4':
              // ********************************************************************
              // REF:CQ237   Who: Raj S   When: 05/02/2008    Begin Change
              // ********************************************************************
              if (AsChar(import.DmdcProMatchResponse.ChSponsorRelCode) > '0'
                && AsChar(import.DmdcProMatchResponse.ChSponsorRelCode) <= '7')
              {
                local.OtherCovChildRelList.Index =
                  (int)(StringToNumber(
                    import.DmdcProMatchResponse.ChSponsorRelCode) - 1);
                local.OtherCovChildRelList.CheckSize();

                local.SelOthcovChldRelShrt.Text2 =
                  local.OtherCovChildRelList.Item.OtherCovChldRelShort.Text2;
                local.SelOthcovChildRelation.Text14 =
                  local.OtherCovChildRelList.Item.OtherCovChildRelation.Text14;

                if (AsChar(local.ChildInactive.Flag) == 'Y')
                {
                  local.Group.Update.Infrastructure.ReasonCode =
                    TrimEnd(local.SelOthcovChildRelation.Text14) + "ENDSYS";
                  local.Group.Update.Grp2NdCode.ReasonCode =
                    TrimEnd(local.SelOthcovChildRelation.Text14) + "END";
                }
                else
                {
                  local.Group.Update.Infrastructure.ReasonCode =
                    TrimEnd(local.SelOthcovChildRelation.Text14) + "END";
                  local.Group.Update.Grp2NdCode.ReasonCode =
                    TrimEnd(local.SelOthcovChildRelation.Text14) + "ENDSYS";
                }
              }
              else if (AsChar(local.ApInactive.Flag) == 'Y' || AsChar
                (local.ChildInactive.Flag) == 'Y')
              {
                local.Group.Update.Infrastructure.ReasonCode = "OTHCOVENDSYS";
                local.Group.Update.Grp2NdCode.ReasonCode = "OTHCOVEND";
              }
              else
              {
                local.Group.Update.Infrastructure.ReasonCode = "OTHCOVEND";
                local.Group.Update.Grp2NdCode.ReasonCode = "OTHCOVENDSYS";
              }

              // ********************************************************************
              // REF:CQ237   Who: Raj S   When: 05/02/2008    End Change
              // ********************************************************************
              local.Group.Update.Infrastructure.DenormNumeric12 =
                import.DmdcProMatchResponse.ChMemberId;
              local.Group.Update.DmdcProMatchResponse.ChMemberId =
                import.DmdcProMatchResponse.ChMemberId;
              local.Group.Update.Infrastructure.CsePersonNumber =
                local.ChNumber.CsePersonNumber ?? "";

              break;
            default:
              break;
          }
        }

Test6:

        if (AsChar(import.DmdcProMatchResponse.NcpInTheMilitaryIndicator) == 'Y'
          )
        {
          if (AsChar(import.DmdcProMatchResponse.ChMedicalCoverageIndicator) ==
            'Y' && AsChar
            (import.DmdcProMatchResponse.ChMedicalCoverageSponsorCode) == '1')
          {
            // THIS HAD BE ALREADY DONE EARLIER, NO NEED TO DO IT AGAIN.
          }
          else
          {
            // IN MILITARY AND NOT PROVIDING MEDICAL BENFITS TO THE CHILD
            ++local.Group.Index;
            local.Group.CheckSize();

            local.Group.Update.DmdcProMatchResponse.ChFirstName =
              import.DmdcProMatchResponse.NcpFirstName;
            local.Group.Update.DmdcProMatchResponse.ChMiddleName =
              import.DmdcProMatchResponse.NcpMiddleName;
            local.Group.Update.DmdcProMatchResponse.ChLastName =
              import.DmdcProMatchResponse.NcpLastName;
            local.Group.Update.DmdcProMatchResponse.ChDeathIndicator =
              import.DmdcProMatchResponse.NcpDeathIndicator;
            local.Group.Update.DmdcProMatchResponse.ChMedicalCoverageIndicator =
              import.DmdcProMatchResponse.NcpInTheMilitaryIndicator;
            local.Group.Update.Infrastructure.DenormNumeric12 =
              import.DmdcProMatchResponse.ChMemberId;
            local.Group.Update.Infrastructure.EventId = 10;
            local.Group.Update.Infrastructure.DenormDate = Now().Date;
            local.Group.Update.DmdcProMatchResponse.ChMemberId =
              import.DmdcProMatchResponse.NcpMemberId;
            local.Group.Update.Infrastructure.CsePersonNumber =
              local.ApNumber.CsePersonNumber ?? "";

            if (AsChar(local.ApInactive.Flag) == 'Y' || AsChar
              (local.ChildInactive.Flag) == 'Y')
            {
              local.Group.Update.Infrastructure.ReasonCode = "NCPINMILTSYS";
              local.Group.Update.Grp2NdCode.ReasonCode = "NCPINMILT";
            }
            else
            {
              local.Group.Update.Infrastructure.ReasonCode = "NCPINMILT";
              local.Group.Update.Grp2NdCode.ReasonCode = "NCPINMILTSYS";
            }
          }
        }
        else if (import.DmdcProMatchResponse.NcpMemberId > 0)
        {
          ++local.Group.Index;
          local.Group.CheckSize();

          local.Group.Update.DmdcProMatchResponse.ChFirstName =
            import.DmdcProMatchResponse.NcpFirstName;
          local.Group.Update.DmdcProMatchResponse.ChMiddleName =
            import.DmdcProMatchResponse.NcpMiddleName;
          local.Group.Update.DmdcProMatchResponse.ChLastName =
            import.DmdcProMatchResponse.NcpLastName;
          local.Group.Update.DmdcProMatchResponse.ChDeathIndicator =
            import.DmdcProMatchResponse.NcpDeathIndicator;
          local.Group.Update.DmdcProMatchResponse.ChMedicalCoverageIndicator =
            import.DmdcProMatchResponse.NcpInTheMilitaryIndicator;
          local.Group.Update.Infrastructure.DenormNumeric12 =
            import.DmdcProMatchResponse.ChMemberId;
          local.Group.Update.Infrastructure.EventId = 10;
          local.Group.Update.Infrastructure.DenormDate = Now().Date;
          local.Group.Update.DmdcProMatchResponse.ChMemberId =
            import.DmdcProMatchResponse.NcpMemberId;
          local.Group.Update.Infrastructure.CsePersonNumber =
            local.ApNumber.CsePersonNumber ?? "";

          if (AsChar(local.ApInactive.Flag) == 'Y' || AsChar
            (local.ChildInactive.Flag) == 'Y')
          {
            local.Group.Update.Infrastructure.ReasonCode = "NCPOUTMILTSYS";
            local.Group.Update.Grp2NdCode.ReasonCode = "NCPOUTMILT";
          }
          else
          {
            local.Group.Update.Infrastructure.ReasonCode = "NCPOUTMILT";
            local.Group.Update.Grp2NdCode.ReasonCode = "NCPOUTMILTSYS";
          }
        }

        if (AsChar(import.DmdcProMatchResponse.CpInTheMilitaryIndicator) == 'Y')
        {
          if (AsChar(import.DmdcProMatchResponse.ChMedicalCoverageIndicator) ==
            'Y' && AsChar
            (import.DmdcProMatchResponse.ChMedicalCoverageSponsorCode) == '2')
          {
            // THIS HAD BE ALREADY DONE EARLIER, NO NEED TO DO IT AGAIN.
          }
          else
          {
            // IN MILITARY AND NOT PROVIDING MEDICAL BENFITS TO THE CHILD
            ++local.Group.Index;
            local.Group.CheckSize();

            local.Group.Update.DmdcProMatchResponse.ChFirstName =
              import.DmdcProMatchResponse.CpFirstName;
            local.Group.Update.DmdcProMatchResponse.ChMiddleName =
              import.DmdcProMatchResponse.CpMiddleName;
            local.Group.Update.DmdcProMatchResponse.ChLastName =
              import.DmdcProMatchResponse.CpLastName;
            local.Group.Update.DmdcProMatchResponse.ChDeathIndicator =
              import.DmdcProMatchResponse.CpDeathIndicator;
            local.Group.Update.DmdcProMatchResponse.ChMedicalCoverageIndicator =
              import.DmdcProMatchResponse.CpInTheMilitaryIndicator;
            local.Group.Update.Infrastructure.DenormNumeric12 =
              import.DmdcProMatchResponse.ChMemberId;
            local.Group.Update.Infrastructure.EventId = 10;
            local.Group.Update.Infrastructure.DenormDate = Now().Date;
            local.Group.Update.DmdcProMatchResponse.ChMemberId =
              import.DmdcProMatchResponse.CpMemberId;
            local.Group.Update.Infrastructure.ReasonCode = "CPINMILT";
            local.Group.Update.Infrastructure.CsePersonNumber =
              local.ArNumber.CsePersonNumber ?? "";
          }
        }
        else if (import.DmdcProMatchResponse.CpMemberId > 0)
        {
          ++local.Group.Index;
          local.Group.CheckSize();

          local.Group.Update.DmdcProMatchResponse.ChFirstName =
            import.DmdcProMatchResponse.CpFirstName;
          local.Group.Update.DmdcProMatchResponse.ChMiddleName =
            import.DmdcProMatchResponse.CpMiddleName;
          local.Group.Update.DmdcProMatchResponse.ChLastName =
            import.DmdcProMatchResponse.CpLastName;
          local.Group.Update.DmdcProMatchResponse.ChDeathIndicator =
            import.DmdcProMatchResponse.CpDeathIndicator;
          local.Group.Update.DmdcProMatchResponse.ChMedicalCoverageIndicator =
            import.DmdcProMatchResponse.CpInTheMilitaryIndicator;
          local.Group.Update.Infrastructure.DenormNumeric12 =
            import.DmdcProMatchResponse.ChMemberId;
          local.Group.Update.Infrastructure.EventId = 10;
          local.Group.Update.Infrastructure.DenormDate = Now().Date;
          local.Group.Update.Infrastructure.ReasonCode = "CPOUTMILT";
          local.Group.Update.DmdcProMatchResponse.ChMemberId =
            import.DmdcProMatchResponse.CpMemberId;
          local.Group.Update.Infrastructure.CsePersonNumber =
            local.ChNumber.CsePersonNumber ?? "";
        }

        if (AsChar(import.DmdcProMatchResponse.PfInTheMilitaryIndicator) == 'Y')
        {
          if (AsChar(import.DmdcProMatchResponse.ChMedicalCoverageIndicator) ==
            'Y' && AsChar
            (import.DmdcProMatchResponse.ChMedicalCoverageSponsorCode) == '3')
          {
            // THIS HAD BE ALREADY DONE EARLIER, NO NEED TO DO IT AGAIN.
          }
          else
          {
            // IN MILITARY AND NOT PROVIDING MEDICAL BENFITS TO THE CHILD
            ++local.Group.Index;
            local.Group.CheckSize();

            local.Group.Update.DmdcProMatchResponse.ChFirstName =
              import.DmdcProMatchResponse.PfFirstName;
            local.Group.Update.DmdcProMatchResponse.ChMiddleName =
              import.DmdcProMatchResponse.PfMiddleName;
            local.Group.Update.DmdcProMatchResponse.ChLastName =
              import.DmdcProMatchResponse.PfLastName;
            local.Group.Update.DmdcProMatchResponse.ChDeathIndicator =
              import.DmdcProMatchResponse.PfDeathIndicator;
            local.Group.Update.DmdcProMatchResponse.ChMedicalCoverageIndicator =
              import.DmdcProMatchResponse.PfInTheMilitaryIndicator;
            local.Group.Update.Infrastructure.DenormNumeric12 =
              import.DmdcProMatchResponse.ChMemberId;
            local.Group.Update.Infrastructure.EventId = 10;
            local.Group.Update.Infrastructure.DenormDate = Now().Date;
            local.Group.Update.DmdcProMatchResponse.ChMemberId =
              import.DmdcProMatchResponse.PfMemberId;

            if (AsChar(local.PfInactive.Flag) == 'Y' || AsChar
              (local.ChildInactive.Flag) == 'Y')
            {
              local.Group.Update.Infrastructure.ReasonCode = "PFINMILTSYS";
              local.Group.Update.Grp2NdCode.ReasonCode = "PFINMILT";
            }
            else
            {
              local.Group.Update.Infrastructure.ReasonCode = "PFINMILT";
              local.Group.Update.Grp2NdCode.ReasonCode = "PFINMILTSYS";
            }

            local.Group.Update.Infrastructure.CsePersonNumber =
              local.PfNumber.CsePersonNumber ?? "";
          }
        }
        else if (import.DmdcProMatchResponse.PfMemberId > 0)
        {
          ++local.Group.Index;
          local.Group.CheckSize();

          local.Group.Update.DmdcProMatchResponse.ChFirstName =
            import.DmdcProMatchResponse.PfFirstName;
          local.Group.Update.DmdcProMatchResponse.ChMiddleName =
            import.DmdcProMatchResponse.PfMiddleName;
          local.Group.Update.DmdcProMatchResponse.ChLastName =
            import.DmdcProMatchResponse.PfLastName;
          local.Group.Update.DmdcProMatchResponse.ChDeathIndicator =
            import.DmdcProMatchResponse.PfDeathIndicator;
          local.Group.Update.DmdcProMatchResponse.ChMedicalCoverageIndicator =
            import.DmdcProMatchResponse.PfInTheMilitaryIndicator;
          local.Group.Update.Infrastructure.DenormNumeric12 =
            import.DmdcProMatchResponse.ChMemberId;
          local.Group.Update.Infrastructure.EventId = 10;
          local.Group.Update.Infrastructure.DenormDate = Now().Date;
          local.Group.Update.DmdcProMatchResponse.ChMemberId =
            import.DmdcProMatchResponse.PfMemberId;
          local.Group.Update.Infrastructure.CsePersonNumber =
            local.PfNumber.CsePersonNumber ?? "";

          if (AsChar(local.PfInactive.Flag) == 'Y' || AsChar
            (local.ChildInactive.Flag) == 'Y')
          {
            local.Group.Update.Infrastructure.ReasonCode = "PFOUTMILTSYS";
            local.Group.Update.Grp2NdCode.ReasonCode = "PFOUTMILT";
          }
          else
          {
            local.Group.Update.Infrastructure.ReasonCode = "PFOUTMILT";
            local.Group.Update.Grp2NdCode.ReasonCode = "PFOUTMILTSYS";
          }
        }

        if (import.DmdcProMatchResponse.NcpMemberId > 0)
        {
          local.WorkArea.Text15 =
            NumberToString(import.DmdcProMatchResponse.NcpMemberId, 15);
          local.SecondRead.Number = Substring(local.WorkArea.Text15, 6, 10);

          if (!ReadCsePerson1())
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              TrimEnd(local.Infrastructure.CsePersonNumber) + " DMDC LOAD: CSE PERSON NOT FOUND.";
              
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
            }

            return;
          }

          ++local.Group.Index;
          local.Group.CheckSize();

          local.Group.Update.DmdcProMatchResponse.ChFirstName =
            import.DmdcProMatchResponse.NcpFirstName;
          local.Group.Update.DmdcProMatchResponse.ChMiddleName =
            import.DmdcProMatchResponse.NcpMiddleName;
          local.Group.Update.DmdcProMatchResponse.ChLastName =
            import.DmdcProMatchResponse.NcpLastName;
          local.Group.Update.DmdcProMatchResponse.ChDeathIndicator =
            import.DmdcProMatchResponse.NcpDeathIndicator;
          local.Group.Update.DmdcProMatchResponse.ChMedicalCoverageIndicator =
            import.DmdcProMatchResponse.NcpInTheMilitaryIndicator;
          local.Group.Update.Infrastructure.DenormNumeric12 =
            import.DmdcProMatchResponse.ChMemberId;
          local.Group.Update.Infrastructure.EventId = 10;
          local.Group.Update.Infrastructure.DenormDate = Now().Date;
          local.Group.Update.DmdcProMatchResponse.ChMemberId =
            import.DmdcProMatchResponse.NcpMemberId;
          local.Group.Update.Infrastructure.CsePersonNumber =
            local.ApNumber.CsePersonNumber ?? "";

          if (AsChar(import.DmdcProMatchResponse.NcpDeathIndicator) == 'Y')
          {
            if (Lt(local.Blank.Date, entities.N2dRead.DateOfDeath))
            {
              local.Group.Update.Infrastructure.ReasonCode = "DEATHBEGINSYS";
              local.Group.Update.Grp2NdCode.ReasonCode = "DEATHBEGIN";
            }
            else
            {
              local.Group.Update.Infrastructure.ReasonCode = "DEATHBEGIN";
              local.Group.Update.Grp2NdCode.ReasonCode = "DEATHBEGINSYS";
            }
          }
          else
          {
            if (ReadInfrastructure20())
            {
              ++local.PersonUpdated.Count;
            }

            if (entities.SecondPass.Populated && Equal
              (entities.SecondPass.ReasonCode, "DEATHBEGINSYS"))
            {
              local.Group.Update.Infrastructure.ReasonCode = "DEATHENDSYS";
              local.Group.Update.Grp2NdCode.ReasonCode = "DEATHEND";
            }
            else
            {
              local.Group.Update.Infrastructure.ReasonCode = "DEATHEND";
              local.Group.Update.Grp2NdCode.ReasonCode = "DEATHENDSYS";
            }
          }
        }

        if (import.DmdcProMatchResponse.CpMemberId > 0)
        {
          local.WorkArea.Text15 =
            NumberToString(import.DmdcProMatchResponse.CpMemberId, 15);
          local.SecondRead.Number = Substring(local.WorkArea.Text15, 6, 10);

          if (!ReadCsePerson1())
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              TrimEnd(local.Infrastructure.CsePersonNumber) + " DMDC LOAD: CSE PERSON NOT FOUND.";
              
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
            }

            return;
          }

          ++local.Group.Index;
          local.Group.CheckSize();

          local.Group.Update.DmdcProMatchResponse.ChFirstName =
            import.DmdcProMatchResponse.CpFirstName;
          local.Group.Update.DmdcProMatchResponse.ChMiddleName =
            import.DmdcProMatchResponse.CpMiddleName;
          local.Group.Update.DmdcProMatchResponse.ChLastName =
            import.DmdcProMatchResponse.CpLastName;
          local.Group.Update.DmdcProMatchResponse.ChDeathIndicator =
            import.DmdcProMatchResponse.CpDeathIndicator;
          local.Group.Update.DmdcProMatchResponse.ChMedicalCoverageIndicator =
            import.DmdcProMatchResponse.CpInTheMilitaryIndicator;
          local.Group.Update.Infrastructure.DenormNumeric12 =
            import.DmdcProMatchResponse.ChMemberId;
          local.Group.Update.Infrastructure.EventId = 10;
          local.Group.Update.Infrastructure.DenormDate = Now().Date;
          local.Group.Update.DmdcProMatchResponse.ChMemberId =
            import.DmdcProMatchResponse.CpMemberId;
          local.Group.Update.Infrastructure.CsePersonNumber =
            local.ArNumber.CsePersonNumber ?? "";

          if (AsChar(import.DmdcProMatchResponse.CpDeathIndicator) == 'Y')
          {
            if (Lt(local.Blank.Date, entities.N2dRead.DateOfDeath))
            {
              local.Group.Update.Infrastructure.ReasonCode = "DEATHBEGINSYS";
              local.Group.Update.Grp2NdCode.ReasonCode = "DEATHBEGIN";
            }
            else
            {
              local.Group.Update.Infrastructure.ReasonCode = "DEATHBEGIN";
              local.Group.Update.Grp2NdCode.ReasonCode = "DEATHBEGINSYS";
            }
          }
          else
          {
            if (ReadInfrastructure20())
            {
              ++local.PersonUpdated.Count;
            }

            if (entities.SecondPass.Populated && Equal
              (entities.SecondPass.ReasonCode, "DEATHBEGINSYS"))
            {
              local.Group.Update.Infrastructure.ReasonCode = "DEATHENDSYS";
              local.Group.Update.Grp2NdCode.ReasonCode = "DEATHEND";
            }
            else
            {
              local.Group.Update.Infrastructure.ReasonCode = "DEATHEND";
              local.Group.Update.Grp2NdCode.ReasonCode = "DEATHENDSYS";
            }
          }
        }

        if (import.DmdcProMatchResponse.PfMemberId > 0)
        {
          local.WorkArea.Text15 =
            NumberToString(import.DmdcProMatchResponse.PfMemberId, 15);
          local.SecondRead.Number = Substring(local.WorkArea.Text15, 6, 10);

          if (!ReadCsePerson1())
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              TrimEnd(local.Infrastructure.CsePersonNumber) + " DMDC LOAD: CSE PERSON NOT FOUND.";
              
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
            }

            return;
          }

          ++local.Group.Index;
          local.Group.CheckSize();

          local.Group.Update.DmdcProMatchResponse.ChFirstName =
            import.DmdcProMatchResponse.PfFirstName;
          local.Group.Update.DmdcProMatchResponse.ChMiddleName =
            import.DmdcProMatchResponse.PfMiddleName;
          local.Group.Update.DmdcProMatchResponse.ChLastName =
            import.DmdcProMatchResponse.PfLastName;
          local.Group.Update.DmdcProMatchResponse.ChDeathIndicator =
            import.DmdcProMatchResponse.PfDeathIndicator;
          local.Group.Update.DmdcProMatchResponse.ChMedicalCoverageIndicator =
            import.DmdcProMatchResponse.PfInTheMilitaryIndicator;
          local.Group.Update.Infrastructure.DenormNumeric12 =
            import.DmdcProMatchResponse.ChMemberId;
          local.Group.Update.Infrastructure.EventId = 10;
          local.Group.Update.Infrastructure.DenormDate = Now().Date;
          local.Group.Update.DmdcProMatchResponse.ChMemberId =
            import.DmdcProMatchResponse.PfMemberId;
          local.Group.Update.Infrastructure.CsePersonNumber =
            local.PfNumber.CsePersonNumber ?? "";

          if (AsChar(import.DmdcProMatchResponse.PfDeathIndicator) == 'Y')
          {
            if (Lt(local.Blank.Date, entities.N2dRead.DateOfDeath))
            {
              local.Group.Update.Infrastructure.ReasonCode = "DEATHBEGINSYS";
              local.Group.Update.Grp2NdCode.ReasonCode = "DEATHBEGIN";
            }
            else
            {
              local.Group.Update.Infrastructure.ReasonCode = "DEATHBEGIN";
              local.Group.Update.Grp2NdCode.ReasonCode = "DEATHBEGINSYS";
            }
          }
          else
          {
            if (ReadInfrastructure20())
            {
              ++local.PersonUpdated.Count;
            }

            if (entities.SecondPass.Populated && Equal
              (entities.SecondPass.ReasonCode, "DEATHBEGINSYS"))
            {
              local.Group.Update.Infrastructure.ReasonCode = "DEATHENDSYS";
              local.Group.Update.Grp2NdCode.ReasonCode = "DEATHEND";
            }
            else
            {
              local.Group.Update.Infrastructure.ReasonCode = "DEATHEND";
              local.Group.Update.Grp2NdCode.ReasonCode = "DEATHENDSYS";
            }
          }
        }
      }

      local.Group.Index = 0;

      for(var limit = local.Group.Count; local.Group.Index < limit; ++
        local.Group.Index)
      {
        if (!local.Group.CheckSize())
        {
          break;
        }

        local.Infrastructure.Assign(local.Clear);
        local.PersonsUpdated.Count = 0;

        if (Equal(local.Group.Item.Infrastructure.ReasonCode, "DEATHBEGIN") || Equal
          (local.Group.Item.Infrastructure.ReasonCode, "DEATHEND") || Equal
          (local.Group.Item.Infrastructure.ReasonCode, "DEATHBEGINSYS") || Equal
          (local.Group.Item.Infrastructure.ReasonCode, "DEATHEENDSYS"))
        {
          if (ReadInfrastructure2())
          {
            ++local.PersonsUpdated.Count;
          }
        }
        else if (ReadInfrastructure4())
        {
          ++local.PersonsUpdated.Count;
        }

        local.WorkArea.Text15 =
          NumberToString(local.Group.Item.DmdcProMatchResponse.ChMemberId, 15);
        local.SecondRead.Number = Substring(local.WorkArea.Text15, 6, 10);

        switch(TrimEnd(local.Group.Item.Infrastructure.ReasonCode))
        {
          case "DEATHBEGIN":
            if (local.PersonsUpdated.Count < 1)
            {
              // add record
            }
            else
            {
              local.PersonUpdated.Count = 0;

              if (Equal(entities.Infrastructure.ReasonCode,
                local.Group.Item.Infrastructure.ReasonCode))
              {
                if (ReadInfrastructure10())
                {
                  ++local.PersonUpdated.Count;
                }
              }

              if (local.PersonUpdated.Count >= 1)
              {
                if (Lt(entities.SecondPass.CreatedTimestamp,
                  entities.Infrastructure.CreatedTimestamp))
                {
                  // do nothing, already have record
                  continue;
                }
                else
                {
                  // add record
                }
              }
              else
              {
                // EITHER THERE WAS NOT A ENDING TO  A DEATHBEGIN RECORD OR 
                // THERE IS A DEATHBEGINSYS RECORD, NOW HAVE TO CHECK FOR IT.
                local.PersonUpdated.Count = 0;

                if (ReadInfrastructure1())
                {
                  ++local.PersonUpdated.Count;
                }

                if (local.PersonUpdated.Count >= 1)
                {
                  local.PersonUpdated.Count = 0;

                  // FOUND A DEATHBEGINSYS RECORD,  NOW HAVE TO CHECK TO SEE IF 
                  // IT IS CLOSED.
                  if (ReadInfrastructure11())
                  {
                    ++local.PersonUpdated.Count;
                  }

                  if (local.PersonUpdated.Count >= 1)
                  {
                    // FOUND A CLOSE FOR A DEATHBEGINSYS RECORD BUT IS IT FOR 
                    // THE LATEST?
                    if (Lt(entities.Infrastructure.CreatedTimestamp,
                      entities.SecondPass.CreatedTimestamp))
                    {
                      // THIS DEATHBEGINSYS IS CLOSED SO IT IS OK TO ADD A 
                      // DEATHBEGIN RECORD.
                    }
                    else
                    {
                      // FOUND A OPEN DEATHBEGINSYS RECORD SO WE DO NOT WANT TO 
                      // OPEN A RECORD.
                      continue;
                    }
                  }
                  else
                  {
                    // DID NOT FIND A CLOSED RECORD FOR THE DEATHBEGINSYS RECORD
                    // SO IT IS STILL ACTIVE, THEREFORE WE DO NOT WANT TO ADD A
                    // RECORD.
                    continue;
                  }
                }
                else
                {
                  // THERE IS A DEATHBEGIN BUT THERE IS NO END TO IT,  WE WILL 
                  // NOT ADD ANOTHER RECORD.
                  continue;
                }
              }
            }

            local.Infrastructure.ReasonCode =
              local.Group.Item.Infrastructure.ReasonCode;
            local.Infrastructure.CsePersonNumber =
              local.Group.Item.Infrastructure.CsePersonNumber ?? "";
            local.Infrastructure.Detail = "NO DATE OF DEATH AVAILABLE";

            break;
          case "DEATHBEGINSYS":
            if (local.PersonsUpdated.Count < 1)
            {
              // add record
            }
            else
            {
              local.PersonUpdated.Count = 0;

              if (Equal(entities.Infrastructure.ReasonCode,
                local.Group.Item.Infrastructure.ReasonCode))
              {
                if (ReadInfrastructure11())
                {
                  ++local.PersonUpdated.Count;
                }
              }

              if (local.PersonUpdated.Count >= 1)
              {
                if (Lt(entities.SecondPass.CreatedTimestamp,
                  entities.Infrastructure.CreatedTimestamp))
                {
                  // do nothing, already have record
                  continue;
                }
                else
                {
                  // add record
                }
              }
              else
              {
                // EITHER THERE WAS NOT A ENDING TO  A DEATHBEGINSYS RECORD OR 
                // THERE IS A DEATHBEGIN RECORD, NOW HAVE TO CHECK FOR IT.
                local.PersonUpdated.Count = 0;

                if (ReadInfrastructure1())
                {
                  ++local.PersonUpdated.Count;
                }

                if (local.PersonUpdated.Count >= 1)
                {
                  local.PersonUpdated.Count = 0;

                  // FOUND A DEATHBEGIN RECORD,  NOW HAVE TO CHECK TO SEE IF IT 
                  // IS CLOSED.
                  if (ReadInfrastructure10())
                  {
                    ++local.PersonUpdated.Count;
                  }

                  if (local.PersonUpdated.Count >= 1)
                  {
                    // FOUND A CLOSE FOR A DEATHBEGINSYS RECORD BUT IS IT FOR 
                    // THE LATEST?
                    if (Lt(entities.Infrastructure.CreatedTimestamp,
                      entities.SecondPass.CreatedTimestamp))
                    {
                      // THIS DEATHBEGIN IS CLOSED SO IT IS OK TO ADD A 
                      // DEATHBEGINSYS RECORD.
                    }
                    else
                    {
                      // FOUND A OPEN DEATHBEGIN RECORD SO WE DO NOT WANT TO 
                      // OPEN A RECORD.
                      continue;
                    }
                  }
                  else
                  {
                    // DID NOT FIND A CLOSED RECORD FOR THE DEATHBEGIN RECORD SO
                    // IT IS STILL ACTIVE, THEREFORE WE DO NOT WANT TO ADD A
                    // RECORD.
                    continue;
                  }
                }
                else
                {
                  // THERE IS A DEATHBEGINSYS BUT THERE IS NO END TO IT,  WE 
                  // WILL NOT ADD ANOTHER RECORD.
                  continue;
                }
              }
            }

            local.Infrastructure.ReasonCode =
              local.Group.Item.Infrastructure.ReasonCode;
            local.Infrastructure.CsePersonNumber =
              local.Group.Item.Infrastructure.CsePersonNumber ?? "";
            local.Infrastructure.Detail = "NO DATE OF DEATH AVAILABLE";

            break;
          case "DEATHEND":
            local.PersonUpdated.Count = 0;

            if (local.PersonsUpdated.Count < 1)
            {
              // no previous ending record found
              if (ReadInfrastructure8())
              {
                ++local.PersonUpdated.Count;
              }

              if (local.PersonUpdated.Count < 1)
              {
                local.PersonUpdated.Count = 0;

                if (ReadInfrastructure9())
                {
                  ++local.PersonUpdated.Count;
                }

                if (local.PersonUpdated.Count < 1)
                {
                  // do nothing, there is no beginning of death to reverse, the 
                  // normal state of records is that the peopple are alive
                  continue;
                }
                else
                {
                  // add DEATHENDSYS record
                  local.Infrastructure.ReasonCode =
                    local.Group.Item.Grp2NdCode.ReasonCode;
                }
              }
              else
              {
                // add DEATHEND record
                local.Infrastructure.ReasonCode =
                  local.Group.Item.Infrastructure.ReasonCode;
              }
            }
            else
            {
              // found a ending record, but which one?
              // is there even a begin record?
              local.PersonUpdated.Count = 0;

              if (ReadInfrastructure20())
              {
                ++local.PersonUpdated.Count;
              }

              if (local.PersonUpdated.Count >= 1)
              {
                if (Lt(entities.SecondPass.CreatedTimestamp,
                  entities.Infrastructure.CreatedTimestamp))
                {
                  // THERE IS A DEATHBEGIN/DEATHBEGINSYS RECORD BUT IT HAS 
                  // ALREADY BEEN CLOSED OUT
                  continue;
                }
                else
                {
                  // add record
                  if (Equal(entities.SecondPass.ReasonCode, "DEATHBEGIN"))
                  {
                    local.Infrastructure.ReasonCode =
                      local.Group.Item.Infrastructure.ReasonCode;
                  }
                  else
                  {
                    local.Infrastructure.ReasonCode =
                      local.Group.Item.Grp2NdCode.ReasonCode;
                  }
                }
              }
              else
              {
                // THERE WAS NOT A DEATHBEGIN OR A DEATHBEGINSYS RECORD SO THERE
                // IS NO REASON TO ADD A ENDING RECORD.
                continue;
              }
            }

            local.Infrastructure.CsePersonNumber =
              local.Group.Item.Infrastructure.CsePersonNumber ?? "";
            local.Infrastructure.Detail =
              Spaces(Infrastructure.Detail_MaxLength);

            break;
          case "DEATHENDSYS":
            if (local.PersonsUpdated.Count < 1)
            {
              local.PersonUpdated.Count = 0;

              // no previous ending record found
              if (ReadInfrastructure9())
              {
                ++local.PersonUpdated.Count;
              }

              if (local.PersonUpdated.Count < 1)
              {
                local.PersonUpdated.Count = 0;

                if (ReadInfrastructure8())
                {
                  ++local.PersonUpdated.Count;
                }

                if (local.PersonUpdated.Count < 1)
                {
                  // do nothing, there is no beginning of death to reverse, the 
                  // normal state of records is that the peopple are alive
                  continue;
                }
                else
                {
                  // add DEATHENDSYS record
                  local.Infrastructure.ReasonCode =
                    local.Group.Item.Grp2NdCode.ReasonCode;
                }
              }
              else
              {
                // add DEATHEND record
                local.Infrastructure.ReasonCode =
                  local.Group.Item.Infrastructure.ReasonCode;
              }
            }
            else
            {
              // found a ending record, but which one?
              // is there even a begin record?
              local.PersonUpdated.Count = 0;

              if (ReadInfrastructure20())
              {
                ++local.PersonUpdated.Count;
              }

              if (local.PersonUpdated.Count >= 1)
              {
                if (Lt(entities.SecondPass.CreatedTimestamp,
                  entities.Infrastructure.CreatedTimestamp))
                {
                  // THERE IS A DEATHBEGIN/DEATHBEGINSYS RECORD BUT IT HAS 
                  // ALREADY BEEN CLOSED OUT
                  continue;
                }
                else
                {
                  // add record
                  if (Equal(entities.SecondPass.ReasonCode, "DEATHBEGINSYS"))
                  {
                    local.Infrastructure.ReasonCode =
                      local.Group.Item.Infrastructure.ReasonCode;
                  }
                  else
                  {
                    local.Infrastructure.ReasonCode =
                      local.Group.Item.Grp2NdCode.ReasonCode;
                  }
                }
              }
              else
              {
                // THERE WAS NOT A DEATHBEGIN OR A DEATHBEGINSYS RECORD SO THERE
                // IS NO REASON TO ADD A ENDING RECORD.
                continue;
              }
            }

            local.Infrastructure.CsePersonNumber =
              local.Group.Item.Infrastructure.CsePersonNumber ?? "";
            local.Infrastructure.Detail =
              Spaces(Infrastructure.Detail_MaxLength);

            break;
          case "NCPCOVBEGIN":
            if (local.PersonsUpdated.Count < 1)
            {
              // add record
            }
            else
            {
              local.PersonUpdated.Count = 0;

              if (ReadInfrastructure59())
              {
                ++local.PersonUpdated.Count;
              }

              if (local.PersonUpdated.Count >= 1)
              {
                if (Lt(entities.SecondPass.CreatedTimestamp,
                  entities.Infrastructure.CreatedTimestamp))
                {
                  // do nothing, already have record
                  continue;
                }
                else
                {
                  // add record
                }
              }
              else
              {
                // EITHER THERE WAS NOT A ENDING TO  A NCPCOVBEGIN RECORD OR 
                // THERE IS A NCPCOVBEGINSYS RECORD, NOW HAVE TO CHECK FOR IT.
                local.PersonUpdated.Count = 0;

                if (ReadInfrastructure1())
                {
                  ++local.PersonUpdated.Count;
                }

                if (local.PersonUpdated.Count >= 1)
                {
                  local.PersonUpdated.Count = 0;

                  // FOUND A NCPCOVBEGINSYS RECORD,  NOW HAVE TO CHECK TO SEE IF
                  // IT IS CLOSED.
                  if (ReadInfrastructure23())
                  {
                    ++local.PersonUpdated.Count;
                  }

                  if (local.PersonUpdated.Count >= 1)
                  {
                    // FOUND A CLOSE FOR A NCPCOVBEGINSYS RECORD BUT IS IT FOR 
                    // THE LATEST?
                    if (Lt(entities.Infrastructure.CreatedTimestamp,
                      entities.SecondPass.CreatedTimestamp))
                    {
                      // THIS NCPCOVBEGINSYS IS CLOSED SO IT IS OK TO ADD A 
                      // NCPCOVBEGIN RECORD.
                    }
                    else
                    {
                      // FOUND A OPEN NCPCOVBEGINSYS RECORD SO WE DO NOT WANT TO
                      // OPEN A RECORD.
                      continue;
                    }
                  }
                  else
                  {
                    // DID NOT FIND A CLOSED RECORD FOR THE NCPCOVBEGINSYS 
                    // RECORD SO IT IS STILL ACTIVE, THEREFORE WE DO NOT WANT TO
                    // ADD A RECORD.
                    continue;
                  }
                }
                else
                {
                  // THERE IS A NCPCOVBEGIN BUT THERE IS NO END TO IT,  WE WILL 
                  // NOT ADD ANOTHER RECORD.
                  continue;
                }
              }
            }

            local.PersonQualified.Count = 0;
            local.Infrastructure.ReasonCode =
              local.Group.Item.Infrastructure.ReasonCode;
            local.Infrastructure.CsePersonNumber =
              local.Group.Item.Infrastructure.CsePersonNumber ?? "";
            local.ConvertDateDateWorkArea.Date =
              import.DmdcProMatchResponse.ChMedicalCoverageBeginDate;
            UseCabConvertDate2String();
            local.Dmdc.Text10 = local.ChNumber.CsePersonNumber ?? Spaces(10);
            local.Dmdc.Text50 =
              TrimEnd(import.DmdcProMatchResponse.ChLastName) + ", " + TrimEnd
              (import.DmdcProMatchResponse.ChFirstName) + " " + Substring
              (import.DmdcProMatchResponse.ChMiddleName,
              DmdcProMatchResponse.ChMiddleName_MaxLength, 1, 1);

            if (Equal(local.ConvertDateTextWorkArea.Text8, "00000000"))
            {
              local.ConvertDateTextWorkArea.Text8 = "";
            }

            local.Dmdc.Text32 = local.ConvertDateTextWorkArea.Text8 + " ELIGIBILITY DATE";
              
            local.Infrastructure.Detail = TrimEnd(local.Dmdc.Text50) + " " + TrimEnd
              (local.Dmdc.Text10) + " " + TrimEnd(local.Dmdc.Text32);

            break;
          case "NCPCOVBEGINSYS":
            if (local.PersonsUpdated.Count < 1)
            {
              // add record
            }
            else
            {
              local.PersonUpdated.Count = 0;

              if (ReadInfrastructure60())
              {
                ++local.PersonUpdated.Count;
              }

              if (local.PersonUpdated.Count >= 1)
              {
                if (Lt(entities.SecondPass.CreatedTimestamp,
                  entities.Infrastructure.CreatedTimestamp))
                {
                  // do nothing, already have record
                  continue;
                }
                else
                {
                  // add record
                }
              }
              else
              {
                // EITHER THERE WAS NOT A ENDING TO  A NCPCOVBEGINSYS RECORD OR 
                // THERE IS A NCPCOVBEGIN RECORD, NOW HAVE TO CHECK FOR IT.
                local.PersonUpdated.Count = 0;

                if (ReadInfrastructure1())
                {
                  ++local.PersonUpdated.Count;
                }

                if (local.PersonUpdated.Count >= 1)
                {
                  local.PersonUpdated.Count = 0;

                  // FOUND A NCPCOVBEGIN RECORD,  NOW HAVE TO CHECK TO SEE IF IT
                  // IS CLOSED.
                  if (ReadInfrastructure22())
                  {
                    ++local.PersonUpdated.Count;
                  }

                  if (local.PersonUpdated.Count >= 1)
                  {
                    // FOUND A CLOSE FOR A NCPCOVBEGIN RECORD BUT IS IT FOR THE 
                    // LATEST?
                    if (Lt(entities.Infrastructure.CreatedTimestamp,
                      entities.SecondPass.CreatedTimestamp))
                    {
                      // THIS NCPCOVBEGIN IS CLOSED SO IT IS OK TO ADD A 
                      // NCPCOVBEGINSYS RECORD.
                    }
                    else
                    {
                      // FOUND A OPEN NCPCOVBEGIN RECORD SO WE DO NOT WANT TO 
                      // OPEN A RECORD.
                      continue;
                    }
                  }
                  else
                  {
                    // DID NOT FIND A CLOSED RECORD FOR THE NCPCOVBEGIN RECORD 
                    // SO IT IS STILL ACTIVE, THEREFORE WE DO NOT WANT TO ADD A
                    // RECORD.
                    continue;
                  }
                }
                else
                {
                  // THERE IS A NCPCOVBEGINSYS BUT THERE IS NO END TO IT,  WE 
                  // WILL NOT ADD ANOTHER RECORD.
                  continue;
                }
              }
            }

            local.PersonQualified.Count = 0;
            local.Infrastructure.ReasonCode =
              local.Group.Item.Infrastructure.ReasonCode;
            local.Infrastructure.CsePersonNumber =
              local.Group.Item.Infrastructure.CsePersonNumber ?? "";
            local.ConvertDateDateWorkArea.Date =
              import.DmdcProMatchResponse.ChMedicalCoverageBeginDate;
            UseCabConvertDate2String();
            local.Dmdc.Text10 = local.ChNumber.CsePersonNumber ?? Spaces(10);
            local.Dmdc.Text50 =
              TrimEnd(import.DmdcProMatchResponse.ChLastName) + ", " + TrimEnd
              (import.DmdcProMatchResponse.ChFirstName) + " " + Substring
              (import.DmdcProMatchResponse.ChMiddleName,
              DmdcProMatchResponse.ChMiddleName_MaxLength, 1, 1);

            if (Equal(local.ConvertDateTextWorkArea.Text8, "00000000"))
            {
              local.ConvertDateTextWorkArea.Text8 = "";
            }

            local.Dmdc.Text32 = local.ConvertDateTextWorkArea.Text8 + " ELIGIBILITY DATE";
              
            local.Infrastructure.Detail = TrimEnd(local.Dmdc.Text50) + " " + TrimEnd
              (local.Dmdc.Text10) + " " + TrimEnd(local.Dmdc.Text32);

            break;
          case "CPCOVBEGIN":
            if (local.PersonsUpdated.Count < 1)
            {
              // add record
            }
            else
            {
              local.PersonUpdated.Count = 0;

              if (ReadInfrastructure55())
              {
                ++local.PersonUpdated.Count;
              }

              if (local.PersonUpdated.Count >= 1)
              {
                if (Lt(entities.SecondPass.CreatedTimestamp,
                  entities.Infrastructure.CreatedTimestamp))
                {
                  // do nothing, already have record
                  continue;
                }
                else
                {
                  // add record
                }
              }
              else
              {
                // do nothing
                continue;
              }
            }

            local.PersonQualified.Count = 0;
            local.Infrastructure.ReasonCode =
              local.Group.Item.Infrastructure.ReasonCode;
            local.Infrastructure.CsePersonNumber =
              local.Group.Item.Infrastructure.CsePersonNumber ?? "";
            local.Dmdc.Text10 = local.ChNumber.CsePersonNumber ?? Spaces(10);
            local.Dmdc.Text50 =
              TrimEnd(import.DmdcProMatchResponse.ChLastName) + ", " + TrimEnd
              (import.DmdcProMatchResponse.ChFirstName) + " " + Substring
              (import.DmdcProMatchResponse.ChMiddleName,
              DmdcProMatchResponse.ChMiddleName_MaxLength, 1, 1);
            local.ConvertDateDateWorkArea.Date =
              import.DmdcProMatchResponse.ChMedicalCoverageBeginDate;
            UseCabConvertDate2String();

            if (Equal(local.ConvertDateTextWorkArea.Text8, "00000000"))
            {
              local.ConvertDateTextWorkArea.Text8 = "";
            }

            local.Dmdc.Text32 = local.ConvertDateTextWorkArea.Text8 + " ELIGIBILITY DATE";
              
            local.Infrastructure.Detail = TrimEnd(local.Dmdc.Text50) + " " + TrimEnd
              (local.Dmdc.Text10) + " " + TrimEnd(local.Dmdc.Text32);

            break;
          case "PFCOVBEGIN":
            if (local.PersonsUpdated.Count < 1)
            {
              // add record
            }
            else
            {
              local.PersonUpdated.Count = 0;

              if (ReadInfrastructure64())
              {
                ++local.PersonUpdated.Count;
              }

              if (local.PersonUpdated.Count >= 1)
              {
                if (Lt(entities.SecondPass.CreatedTimestamp,
                  entities.Infrastructure.CreatedTimestamp))
                {
                  // do nothing, already have record
                  continue;
                }
                else
                {
                  // add record
                }
              }
              else
              {
                // EITHER THERE WAS NOT A ENDING TO  A PFCOVBEGIN RECORD OR 
                // THERE IS A PFCOVBEGINSYS RECORD, NOW HAVE TO CHECK FOR IT.
                local.PersonUpdated.Count = 0;

                if (ReadInfrastructure1())
                {
                  ++local.PersonUpdated.Count;
                }

                if (local.PersonUpdated.Count >= 1)
                {
                  local.PersonUpdated.Count = 0;

                  // FOUND A PFCOVBEGINSYS RECORD,  NOW HAVE TO CHECK TO SEE IF 
                  // IT IS CLOSED.
                  if (ReadInfrastructure68())
                  {
                    ++local.PersonUpdated.Count;
                  }

                  if (local.PersonUpdated.Count >= 1)
                  {
                    // FOUND A CLOSE FOR A PFCOVBEGINSYS RECORD BUT IS IT FOR 
                    // THE LATEST?
                    if (Lt(entities.Infrastructure.CreatedTimestamp,
                      entities.SecondPass.CreatedTimestamp))
                    {
                      // THIS PFCOVBEGINSYS IS CLOSED SO IT IS OK TO ADD A 
                      // PFCOVBEGIN RECORD.
                    }
                    else
                    {
                      // FOUND A OPEN PFCOVBEGINSYS RECORD SO WE DO NOT WANT TO 
                      // OPEN A RECORD.
                      continue;
                    }
                  }
                  else
                  {
                    // DID NOT FIND A CLOSED RECORD FOR THE PFCOVBEGINSYS RECORD
                    // SO IT IS STILL ACTIVE, THEREFORE WE DO NOT WANT TO ADD A
                    // RECORD.
                    continue;
                  }
                }
                else
                {
                  // THERE IS A PFCOVBEGIN BUT THERE IS NO END TO IT,  WE WILL 
                  // NOT ADD ANOTHER RECORD.
                  continue;
                }
              }
            }

            local.PersonQualified.Count = 0;
            local.Infrastructure.ReasonCode =
              local.Group.Item.Infrastructure.ReasonCode;
            local.Infrastructure.CsePersonNumber =
              local.Group.Item.Infrastructure.CsePersonNumber ?? "";
            local.Dmdc.Text10 = local.ChNumber.CsePersonNumber ?? Spaces(10);
            local.Dmdc.Text50 =
              TrimEnd(import.DmdcProMatchResponse.ChLastName) + ", " + TrimEnd
              (import.DmdcProMatchResponse.ChFirstName) + " " + Substring
              (import.DmdcProMatchResponse.ChMiddleName,
              DmdcProMatchResponse.ChMiddleName_MaxLength, 1, 1);
            local.ConvertDateDateWorkArea.Date =
              import.DmdcProMatchResponse.ChMedicalCoverageBeginDate;
            UseCabConvertDate2String();

            if (Equal(local.ConvertDateTextWorkArea.Text8, "00000000"))
            {
              local.ConvertDateTextWorkArea.Text8 = "";
            }

            local.Dmdc.Text32 = local.ConvertDateTextWorkArea.Text8 + " ELIGIBILITY DATE";
              
            local.Infrastructure.Detail = TrimEnd(local.Dmdc.Text50) + " " + TrimEnd
              (local.Dmdc.Text10) + " " + TrimEnd(local.Dmdc.Text32);

            break;
          case "PFCOVBEGINSYS":
            if (local.PersonsUpdated.Count < 1)
            {
              // add record
            }
            else
            {
              local.PersonUpdated.Count = 0;

              if (ReadInfrastructure65())
              {
                ++local.PersonUpdated.Count;
              }

              if (local.PersonUpdated.Count >= 1)
              {
                if (Lt(entities.SecondPass.CreatedTimestamp,
                  entities.Infrastructure.CreatedTimestamp))
                {
                  // do nothing, already have record
                  continue;
                }
                else
                {
                  // add record
                }
              }
              else
              {
                // EITHER THERE WAS NOT A ENDING TO  A PFCOVBEGINSYS RECORD OR 
                // THERE IS A PFCOVBEGIN RECORD, NOW HAVE TO CHECK FOR IT.
                local.PersonUpdated.Count = 0;

                if (ReadInfrastructure1())
                {
                  ++local.PersonUpdated.Count;
                }

                if (local.PersonUpdated.Count >= 1)
                {
                  local.PersonUpdated.Count = 0;

                  // FOUND A PFCOVBEGIN RECORD,  NOW HAVE TO CHECK TO SEE IF IT 
                  // IS CLOSED.
                  if (ReadInfrastructure30())
                  {
                    ++local.PersonUpdated.Count;
                  }

                  if (local.PersonUpdated.Count >= 1)
                  {
                    // FOUND A CLOSE FOR A PFCOVBEGIN RECORD BUT IS IT FOR THE 
                    // LATEST?
                    if (Lt(entities.Infrastructure.CreatedTimestamp,
                      entities.SecondPass.CreatedTimestamp))
                    {
                      // THIS PFCOVBEGIN IS CLOSED SO IT IS OK TO ADD A 
                      // PFCOVBEGINSYS RECORD.
                    }
                    else
                    {
                      // FOUND A OPEN PFCOVBEGINSYS RECORD SO WE DO NOT WANT TO 
                      // OPEN A RECORD.
                      continue;
                    }
                  }
                  else
                  {
                    // DID NOT FIND A CLOSED RECORD FOR THE PFCOVBEGIN RECORD SO
                    // IT IS STILL ACTIVE, THEREFORE WE DO NOT WANT TO ADD A
                    // RECORD.
                    continue;
                  }
                }
                else
                {
                  // THERE IS A PFCOVBEGINSYS BUT THERE IS NO END TO IT,  WE 
                  // WILL NOT ADD ANOTHER RECORD.
                  continue;
                }
              }
            }

            local.PersonQualified.Count = 0;
            local.Infrastructure.ReasonCode =
              local.Group.Item.Infrastructure.ReasonCode;
            local.Infrastructure.CsePersonNumber =
              local.Group.Item.Infrastructure.CsePersonNumber ?? "";
            local.Dmdc.Text10 = local.ChNumber.CsePersonNumber ?? Spaces(10);
            local.Dmdc.Text50 =
              TrimEnd(import.DmdcProMatchResponse.ChLastName) + ", " + TrimEnd
              (import.DmdcProMatchResponse.ChFirstName) + " " + Substring
              (import.DmdcProMatchResponse.ChMiddleName,
              DmdcProMatchResponse.ChMiddleName_MaxLength, 1, 1);
            local.ConvertDateDateWorkArea.Date =
              import.DmdcProMatchResponse.ChMedicalCoverageBeginDate;
            UseCabConvertDate2String();

            if (Equal(local.ConvertDateTextWorkArea.Text8, "00000000"))
            {
              local.ConvertDateTextWorkArea.Text8 = "";
            }

            local.Dmdc.Text32 = local.ConvertDateTextWorkArea.Text8 + " ELIGIBILITY DATE";
              
            local.Infrastructure.Detail = TrimEnd(local.Dmdc.Text50) + " " + TrimEnd
              (local.Dmdc.Text10) + " " + TrimEnd(local.Dmdc.Text32);

            break;
          case "OTHCOVBEGIN":
            if (local.PersonsUpdated.Count < 1)
            {
              // add record
            }
            else
            {
              local.PersonUpdated.Count = 0;

              if (ReadInfrastructure53())
              {
                ++local.PersonUpdated.Count;
              }

              if (local.PersonUpdated.Count >= 1)
              {
                if (Lt(entities.SecondPass.CreatedTimestamp,
                  entities.Infrastructure.CreatedTimestamp))
                {
                  // do nothing, already have record
                  continue;
                }
                else
                {
                  // add record
                }
              }
              else
              {
                // EITHER THERE WAS NOT A ENDING TO  A OTHCOVBEGINSYS RECORD OR 
                // THERE IS A OTHCOVBEGIN RECORD, NOW HAVE TO CHECK FOR IT.
                local.PersonUpdated.Count = 0;

                if (ReadInfrastructure5())
                {
                  ++local.PersonUpdated.Count;
                }

                if (local.PersonUpdated.Count >= 1)
                {
                  local.PersonUpdated.Count = 0;

                  // FOUND A OTHCOVBEGIN RECORD,  NOW HAVE TO CHECK TO SEE IF IT
                  // IS CLOSED.
                  if (ReadInfrastructure63())
                  {
                    ++local.PersonUpdated.Count;
                  }

                  if (local.PersonUpdated.Count >= 1)
                  {
                    // FOUND A CLOSE FOR A OTHCOVBEGIN RECORD BUT IS IT FOR THE 
                    // LATEST?
                    if (Lt(entities.Infrastructure.CreatedTimestamp,
                      entities.SecondPass.CreatedTimestamp))
                    {
                      // THIS OTHCOVBEGIN IS CLOSED SO IT IS OK TO ADD A 
                      // OTHCOVBEGINSYS RECORD.
                    }
                    else
                    {
                      // FOUND A OPEN OTHCOVBEGIN RECORD SO WE DO NOT WANT TO 
                      // OPEN A RECORD.
                      continue;
                    }
                  }
                  else
                  {
                    // DID NOT FIND A CLOSED RECORD FOR THE OTHCOVBEGIN RECORD 
                    // SO IT IS STILL ACTIVE, THEREFORE WE DO NOT WANT TO ADD A
                    // RECORD.
                    continue;
                  }
                }
                else
                {
                  // THERE IS A OTHCOVBEGIN BUT THERE IS NO END TO IT,  WE WILL 
                  // NOT ADD ANOTHER RECORD.
                  continue;
                }
              }
            }

            local.Infrastructure.ReasonCode =
              local.Group.Item.Infrastructure.ReasonCode;
            local.Infrastructure.CsePersonNumber =
              local.Group.Item.Infrastructure.CsePersonNumber ?? "";
            local.ConvertDateDateWorkArea.Date =
              import.DmdcProMatchResponse.ChMedicalCoverageBeginDate;
            UseCabConvertDate2String();

            if (Equal(local.ConvertDateTextWorkArea.Text8, "00000000"))
            {
              local.ConvertDateTextWorkArea.Text8 = "";
            }

            local.Dmdc.Text32 = local.ConvertDateTextWorkArea.Text8 + " ELIGIBILITY DATE";
              
            local.Dmdc.Text40 = "NO MILITARY EMPLOYEE/SPONSOR PROVIDED";
            local.Infrastructure.Detail = TrimEnd(local.Dmdc.Text40) + " " + TrimEnd
              (local.Dmdc.Text32);

            break;
          case "OTHCOVBEGINSYS":
            if (local.PersonsUpdated.Count < 1)
            {
              // add record
            }
            else
            {
              local.PersonUpdated.Count = 0;

              if (ReadInfrastructure53())
              {
                ++local.PersonUpdated.Count;
              }

              if (local.PersonUpdated.Count >= 1)
              {
                if (Lt(entities.SecondPass.CreatedTimestamp,
                  entities.Infrastructure.CreatedTimestamp))
                {
                  // do nothing, already have record
                  continue;
                }
                else
                {
                  // add record
                }
              }
              else
              {
                // EITHER THERE WAS NOT A ENDING TO  A OTHCOVBEGIN RECORD OR 
                // THERE IS A OTHCOVBEGINSYS RECORD, NOW HAVE TO CHECK FOR IT.
                local.PersonUpdated.Count = 0;

                if (ReadInfrastructure1())
                {
                  ++local.PersonUpdated.Count;
                }

                if (local.PersonUpdated.Count >= 1)
                {
                  local.PersonUpdated.Count = 0;

                  // FOUND A OTHCOVBEGINSYS RECORD,  NOW HAVE TO CHECK TO SEE IF
                  // IT IS CLOSED.
                  if (ReadInfrastructure28())
                  {
                    ++local.PersonUpdated.Count;
                  }

                  if (local.PersonUpdated.Count >= 1)
                  {
                    // FOUND A CLOSE FOR A OTHCOVBEGINSYS RECORD BUT IS IT FOR 
                    // THE LATEST?
                    if (Lt(entities.Infrastructure.CreatedTimestamp,
                      entities.SecondPass.CreatedTimestamp))
                    {
                      // THIS OTHCOVBEGINSYS IS CLOSED SO IT IS OK TO ADD A 
                      // OTHCOVBEGIN RECORD.
                    }
                    else
                    {
                      // FOUND A OPEN OTHCOVBEGINSYS RECORD SO WE DO NOT WANT TO
                      // OPEN A RECORD.
                      continue;
                    }
                  }
                  else
                  {
                    // DID NOT FIND A CLOSED RECORD FOR THE OTHCOVBEGINSYS 
                    // RECORD SO IT IS STILL ACTIVE, THEREFORE WE DO NOT WANT TO
                    // ADD A RECORD.
                    continue;
                  }
                }
                else
                {
                  // THERE IS A OTHCOVBEGINSYS BUT THERE IS NO END TO IT,  WE 
                  // WILL NOT ADD ANOTHER RECORD.
                  continue;
                }
              }
            }

            local.Infrastructure.ReasonCode =
              local.Group.Item.Infrastructure.ReasonCode;
            local.Infrastructure.CsePersonNumber =
              local.Group.Item.Infrastructure.CsePersonNumber ?? "";
            local.ConvertDateDateWorkArea.Date =
              import.DmdcProMatchResponse.ChMedicalCoverageBeginDate;
            UseCabConvertDate2String();

            if (Equal(local.ConvertDateTextWorkArea.Text8, "00000000"))
            {
              local.ConvertDateTextWorkArea.Text8 = "";
            }

            local.Dmdc.Text32 = local.ConvertDateTextWorkArea.Text8 + " ELIGIBILITY DATE";
              
            local.Dmdc.Text40 = "NO MILITARY EMPLOYEE/SPONSOR PROVIDED";
            local.Infrastructure.Detail = TrimEnd(local.Dmdc.Text40) + " " + TrimEnd
              (local.Dmdc.Text32);

            break;
          case "NCPCOVEND":
            local.PersonUpdated.Count = 0;

            if (local.PersonsUpdated.Count < 1)
            {
              // no previous ending record found
              if (ReadInfrastructure12())
              {
                ++local.PersonUpdated.Count;
              }

              if (local.PersonUpdated.Count < 1)
              {
                local.PersonUpdated.Count = 0;

                if (ReadInfrastructure13())
                {
                  ++local.PersonUpdated.Count;
                }

                if (local.PersonUpdated.Count < 1)
                {
                  // do nothing, there is no beginning of ncp coverage to 
                  // reverse, the normal state of records is that there is no
                  // coverage
                  continue;
                }
                else
                {
                  // add NCPCOVENDSYS record
                  local.Infrastructure.ReasonCode =
                    local.Group.Item.Grp2NdCode.ReasonCode;
                }
              }
              else
              {
                // add NCPCOVEND record
                local.Infrastructure.ReasonCode =
                  local.Group.Item.Infrastructure.ReasonCode;
              }
            }
            else
            {
              // found a ending record, but which one?
              // is there even a begin record?
              local.PersonUpdated.Count = 0;

              if (ReadInfrastructure21())
              {
                ++local.PersonUpdated.Count;
              }

              if (local.PersonUpdated.Count >= 1)
              {
                if (Lt(entities.SecondPass.CreatedTimestamp,
                  entities.Infrastructure.CreatedTimestamp))
                {
                  // THERE IS A NCPCOVBEGIN/NCPCOVBEGINSYS RECORD BUT IT HAS 
                  // ALREADY BEEN CLOSED OUT
                  continue;
                }
                else
                {
                  // add record
                  if (Equal(entities.SecondPass.ReasonCode, "NCPCOVBEGIN"))
                  {
                    local.Infrastructure.ReasonCode =
                      local.Group.Item.Infrastructure.ReasonCode;
                  }
                  else
                  {
                    local.Infrastructure.ReasonCode =
                      local.Group.Item.Grp2NdCode.ReasonCode;
                  }
                }
              }
              else
              {
                // THERE WAS NOT A NCPCOVBEGIN OR A NCPCOVBEGINSYS RECORD SO 
                // THERE IS NO REASON TO ADD A ENDING RECORD.
                continue;
              }
            }

            local.Infrastructure.CsePersonNumber =
              local.Group.Item.Infrastructure.CsePersonNumber ?? "";
            local.ConvertDateDateWorkArea.Date =
              import.DmdcProMatchResponse.ChMedicalCoverageEndDate;
            UseCabConvertDate2String();

            if (Equal(local.ConvertDateTextWorkArea.Text8, "00000000"))
            {
              local.ConvertDateTextWorkArea.Text8 = "";
            }

            local.Dmdc.Text10 = local.ChNumber.CsePersonNumber ?? Spaces(10);
            local.Dmdc.Text32 = local.ConvertDateTextWorkArea.Text8 + " TERMINATION DATE";
              
            local.Dmdc.Text50 =
              TrimEnd(import.DmdcProMatchResponse.ChLastName) + ", " + TrimEnd
              (import.DmdcProMatchResponse.ChFirstName) + " " + Substring
              (import.DmdcProMatchResponse.ChMiddleName,
              DmdcProMatchResponse.ChMiddleName_MaxLength, 1, 1);
            local.Infrastructure.Detail = TrimEnd(local.Dmdc.Text50) + " " + TrimEnd
              (local.Dmdc.Text10) + " " + TrimEnd(local.Dmdc.Text32);

            break;
          case "NCPCOVENDSYS":
            local.PersonUpdated.Count = 0;

            if (local.PersonsUpdated.Count < 1)
            {
              // no previous ending record found
              if (ReadInfrastructure13())
              {
                ++local.PersonUpdated.Count;
              }

              if (local.PersonUpdated.Count < 1)
              {
                local.PersonUpdated.Count = 0;

                if (ReadInfrastructure12())
                {
                  ++local.PersonUpdated.Count;
                }

                if (local.PersonUpdated.Count < 1)
                {
                  // do nothing, there is no beginning of ncp coverage to 
                  // reverse, the normal state of records is that there is no
                  // coverage
                  continue;
                }
                else
                {
                  // add NCPCOVEND record
                  local.Infrastructure.ReasonCode =
                    local.Group.Item.Grp2NdCode.ReasonCode;
                }
              }
              else
              {
                // add NCPCOVENDSYS record
                local.Infrastructure.ReasonCode =
                  local.Group.Item.Infrastructure.ReasonCode;
              }
            }
            else
            {
              // found a ending record, but which one?
              // is there even a begin record?
              local.PersonUpdated.Count = 0;

              if (ReadInfrastructure21())
              {
                ++local.PersonUpdated.Count;
              }

              if (local.PersonUpdated.Count >= 1)
              {
                if (Lt(entities.SecondPass.CreatedTimestamp,
                  entities.Infrastructure.CreatedTimestamp))
                {
                  // THERE IS A NCPCOVBEGINSYS/NCPCOVBEGIN RECORD BUT IT HAS 
                  // ALREADY BEEN CLOSED OUT
                  continue;
                }
                else
                {
                  // add record
                  if (Equal(entities.SecondPass.ReasonCode, "NCPCOVBEGINSYS"))
                  {
                    local.Infrastructure.ReasonCode =
                      local.Group.Item.Infrastructure.ReasonCode;
                  }
                  else
                  {
                    local.Infrastructure.ReasonCode =
                      local.Group.Item.Grp2NdCode.ReasonCode;
                  }
                }
              }
              else
              {
                // THERE WAS NOT A NCPCOVBEGINSYS OR A NCPCOVBEGIN RECORD SO 
                // THERE IS NO REASON TO ADD A ENDING RECORD.
                continue;
              }
            }

            local.Infrastructure.CsePersonNumber =
              local.Group.Item.Infrastructure.CsePersonNumber ?? "";
            local.ConvertDateDateWorkArea.Date =
              import.DmdcProMatchResponse.ChMedicalCoverageEndDate;
            UseCabConvertDate2String();

            if (Equal(local.ConvertDateTextWorkArea.Text8, "00000000"))
            {
              local.ConvertDateTextWorkArea.Text8 = "";
            }

            local.Dmdc.Text10 = local.ChNumber.CsePersonNumber ?? Spaces(10);
            local.Dmdc.Text32 = local.ConvertDateTextWorkArea.Text8 + " TERMINATION DATE";
              
            local.Dmdc.Text50 =
              TrimEnd(import.DmdcProMatchResponse.ChLastName) + ", " + TrimEnd
              (import.DmdcProMatchResponse.ChFirstName) + " " + Substring
              (import.DmdcProMatchResponse.ChMiddleName,
              DmdcProMatchResponse.ChMiddleName_MaxLength, 1, 1);
            local.Infrastructure.Detail = TrimEnd(local.Dmdc.Text50) + " " + TrimEnd
              (local.Dmdc.Text10) + " " + TrimEnd(local.Dmdc.Text32);

            break;
          case "CPCOVEND":
            local.PersonUpdated.Count = 0;

            if (local.PersonsUpdated.Count < 1)
            {
              if (ReadInfrastructure54())
              {
                ++local.PersonUpdated.Count;
              }

              if (local.PersonUpdated.Count < 1)
              {
                // do nothing, there never was a beginning so it can not be 
                // eneded
                continue;
              }
              else
              {
                // add record
              }
            }
            else
            {
              if (ReadInfrastructure54())
              {
                ++local.PersonUpdated.Count;
              }

              if (local.PersonUpdated.Count >= 1)
              {
                if (Lt(entities.SecondPass.CreatedTimestamp,
                  entities.Infrastructure.CreatedTimestamp))
                {
                  // do nothing, already have record
                  continue;
                }
                else
                {
                  // add record
                }
              }
              else
              {
                // do nothing
                continue;
              }
            }

            local.PersonQualified.Count = 0;
            local.Infrastructure.ReasonCode =
              local.Group.Item.Infrastructure.ReasonCode;
            local.Infrastructure.CsePersonNumber =
              local.Group.Item.Infrastructure.CsePersonNumber ?? "";
            local.ConvertDateDateWorkArea.Date =
              import.DmdcProMatchResponse.ChMedicalCoverageEndDate;
            UseCabConvertDate2String();

            if (Equal(local.ConvertDateTextWorkArea.Text8, "00000000"))
            {
              local.ConvertDateTextWorkArea.Text8 = "";
            }

            local.Dmdc.Text10 = local.ChNumber.CsePersonNumber ?? Spaces(10);
            local.Dmdc.Text32 = local.ConvertDateTextWorkArea.Text8 + " TERMINATION DATE";
              
            local.Dmdc.Text50 =
              TrimEnd(import.DmdcProMatchResponse.ChLastName) + ", " + TrimEnd
              (import.DmdcProMatchResponse.ChFirstName) + " " + Substring
              (import.DmdcProMatchResponse.ChMiddleName,
              DmdcProMatchResponse.ChMiddleName_MaxLength, 1, 1);
            local.Infrastructure.Detail = TrimEnd(local.Dmdc.Text50) + " " + TrimEnd
              (local.Dmdc.Text10) + " " + TrimEnd(local.Dmdc.Text32);

            break;
          case "PFCOVEND":
            local.PersonUpdated.Count = 0;

            if (local.PersonsUpdated.Count < 1)
            {
              // no previous ending record found
              if (ReadInfrastructure16())
              {
                ++local.PersonUpdated.Count;
              }

              if (local.PersonUpdated.Count < 1)
              {
                local.PersonUpdated.Count = 0;

                if (ReadInfrastructure17())
                {
                  ++local.PersonUpdated.Count;
                }

                if (local.PersonUpdated.Count < 1)
                {
                  // do nothing, there is no beginning of pf coverage to 
                  // reverse, the normal state of records is that there is no
                  // coverage
                  continue;
                }
                else
                {
                  // add PFCOVENDSYS record
                  local.Infrastructure.ReasonCode =
                    local.Group.Item.Grp2NdCode.ReasonCode;
                }
              }
              else
              {
                // add PFCOVEND record
                local.Infrastructure.ReasonCode =
                  local.Group.Item.Infrastructure.ReasonCode;
              }
            }
            else
            {
              // found a ending record, but which one?
              // is there even a begin record?
              local.PersonUpdated.Count = 0;

              if (ReadInfrastructure29())
              {
                ++local.PersonUpdated.Count;
              }

              if (local.PersonUpdated.Count >= 1)
              {
                if (Lt(entities.SecondPass.CreatedTimestamp,
                  entities.Infrastructure.CreatedTimestamp))
                {
                  // THERE IS A PFCOVBEGIN/PFCOVBEGINSYS RECORD BUT IT HAS 
                  // ALREADY BEEN CLOSED OUT
                  continue;
                }
                else
                {
                  // add record
                  if (Equal(entities.SecondPass.ReasonCode, "PFCOVBEGIN"))
                  {
                    local.Infrastructure.ReasonCode =
                      local.Group.Item.Infrastructure.ReasonCode;
                  }
                  else
                  {
                    local.Infrastructure.ReasonCode =
                      local.Group.Item.Grp2NdCode.ReasonCode;
                  }
                }
              }
              else
              {
                // THERE WAS NOT A PFCOVBEGIN OR A PFCOVBEGINSYS RECORD SO THERE
                // IS NO REASON TO ADD A ENDING RECORD.
                continue;
              }
            }

            local.PersonQualified.Count = 0;
            local.Infrastructure.CsePersonNumber =
              local.Group.Item.Infrastructure.CsePersonNumber ?? "";
            local.ConvertDateDateWorkArea.Date =
              import.DmdcProMatchResponse.ChMedicalCoverageEndDate;
            UseCabConvertDate2String();

            if (Equal(local.ConvertDateTextWorkArea.Text8, "00000000"))
            {
              local.ConvertDateTextWorkArea.Text8 = "";
            }

            local.Dmdc.Text10 = local.ChNumber.CsePersonNumber ?? Spaces(10);
            local.Dmdc.Text32 = local.ConvertDateTextWorkArea.Text8 + " TERMINATION DATE";
              
            local.Dmdc.Text50 =
              TrimEnd(import.DmdcProMatchResponse.ChLastName) + ", " + TrimEnd
              (import.DmdcProMatchResponse.ChFirstName) + " " + Substring
              (import.DmdcProMatchResponse.ChMiddleName,
              DmdcProMatchResponse.ChMiddleName_MaxLength, 1, 1);
            local.Infrastructure.Detail = TrimEnd(local.Dmdc.Text50) + " " + TrimEnd
              (local.Dmdc.Text10) + " " + TrimEnd(local.Dmdc.Text32);

            break;
          case "PFCOVENDSYS":
            local.PersonUpdated.Count = 0;

            if (local.PersonsUpdated.Count < 1)
            {
              // no previous ending record found
              if (ReadInfrastructure17())
              {
                ++local.PersonUpdated.Count;
              }

              if (local.PersonUpdated.Count < 1)
              {
                local.PersonUpdated.Count = 0;

                if (ReadInfrastructure16())
                {
                  ++local.PersonUpdated.Count;
                }

                if (local.PersonUpdated.Count < 1)
                {
                  // do nothing, there is no beginning of ncp coverage to 
                  // reverse, the normal state of records is that there is no
                  // coverage
                  continue;
                }
                else
                {
                  // add PFCOVEND record
                  local.Infrastructure.ReasonCode =
                    local.Group.Item.Grp2NdCode.ReasonCode;
                }
              }
              else
              {
                // add PFCOVENDSYS record
                local.Infrastructure.ReasonCode =
                  local.Group.Item.Infrastructure.ReasonCode;
              }
            }
            else
            {
              // found a ending record, but which one?
              // is there even a begin record?
              local.PersonUpdated.Count = 0;

              if (ReadInfrastructure29())
              {
                ++local.PersonUpdated.Count;
              }

              if (local.PersonUpdated.Count >= 1)
              {
                if (Lt(entities.SecondPass.CreatedTimestamp,
                  entities.Infrastructure.CreatedTimestamp))
                {
                  // THERE IS A PFCOVBEGINSYS/PFCOVBEGIN RECORD BUT IT HAS 
                  // ALREADY BEEN CLOSED OUT
                  continue;
                }
                else
                {
                  // add record
                  if (Equal(entities.SecondPass.ReasonCode, "PFCOVBEGINSYS"))
                  {
                    local.Infrastructure.ReasonCode =
                      local.Group.Item.Infrastructure.ReasonCode;
                  }
                  else
                  {
                    local.Infrastructure.ReasonCode =
                      local.Group.Item.Grp2NdCode.ReasonCode;
                  }
                }
              }
              else
              {
                // THERE WAS NOT A PFCOVBEGINSYS OR A PFCOVBEGIN RECORD SO THERE
                // IS NO REASON TO ADD A ENDING RECORD.
                continue;
              }
            }

            local.Infrastructure.CsePersonNumber =
              local.Group.Item.Infrastructure.CsePersonNumber ?? "";
            local.Dmdc.Text10 =
              local.Group.Item.Infrastructure.CsePersonNumber ?? Spaces(10);
            local.ConvertDateDateWorkArea.Date =
              import.DmdcProMatchResponse.ChMedicalCoverageEndDate;
            UseCabConvertDate2String();

            if (Equal(local.ConvertDateTextWorkArea.Text8, "00000000"))
            {
              local.ConvertDateTextWorkArea.Text8 = "";
            }

            local.Dmdc.Text10 = local.ChNumber.CsePersonNumber ?? Spaces(10);
            local.Dmdc.Text32 = local.ConvertDateTextWorkArea.Text8 + " TERMINATION DATE";
              
            local.Dmdc.Text50 =
              TrimEnd(import.DmdcProMatchResponse.ChLastName) + ", " + TrimEnd
              (import.DmdcProMatchResponse.ChFirstName) + " " + Substring
              (import.DmdcProMatchResponse.ChMiddleName,
              DmdcProMatchResponse.ChMiddleName_MaxLength, 1, 1);
            local.Infrastructure.Detail = TrimEnd(local.Dmdc.Text50) + " " + TrimEnd
              (local.Dmdc.Text10) + " " + TrimEnd(local.Dmdc.Text32);

            break;
          case "OTHCOVEND":
            local.PersonUpdated.Count = 0;

            if (local.PersonsUpdated.Count < 1)
            {
              // no previous ending record found
              if (ReadInfrastructure48())
              {
                ++local.PersonUpdated.Count;
              }

              if (local.PersonUpdated.Count < 1)
              {
                local.PersonUpdated.Count = 0;

                if (ReadInfrastructure49())
                {
                  ++local.PersonUpdated.Count;
                }

                if (local.PersonUpdated.Count < 1)
                {
                  // do nothing, there is no beginning of other coverage to 
                  // reverse, the normal state of records is that there is no
                  // coverage
                  continue;
                }
                else
                {
                  // add OTHCOVENDSYS record
                  local.Infrastructure.ReasonCode =
                    local.Group.Item.Grp2NdCode.ReasonCode;
                }
              }
              else
              {
                // add OTHCOVEND record
                local.Infrastructure.ReasonCode =
                  local.Group.Item.Infrastructure.ReasonCode;
              }
            }
            else
            {
              // found a ending record, but which one?
              // is there even a begin record?
              local.PersonUpdated.Count = 0;

              if (ReadInfrastructure51())
              {
                ++local.PersonUpdated.Count;
              }

              if (local.PersonUpdated.Count >= 1)
              {
                if (Lt(entities.SecondPass.CreatedTimestamp,
                  entities.Infrastructure.CreatedTimestamp))
                {
                  // THERE IS A OTHCOVBEGIN/OTHCOVBEGINSYS RECORD BUT IT HAS 
                  // ALREADY BEEN CLOSED OUT
                  continue;
                }
                else
                {
                  // add record
                  if (Equal(entities.SecondPass.ReasonCode, "OTHCOVBEGIN"))
                  {
                    local.Infrastructure.ReasonCode =
                      local.Group.Item.Infrastructure.ReasonCode;
                  }
                  else
                  {
                    local.Infrastructure.ReasonCode =
                      local.Group.Item.Grp2NdCode.ReasonCode;
                  }
                }
              }
              else
              {
                // THERE WAS NOT A OTHCOVBEGIN OR A OTHCOVBEGINSYS RECORD SO 
                // THERE IS NO REASON TO ADD A ENDING RECORD.
                continue;
              }
            }

            local.Infrastructure.CsePersonNumber =
              local.Group.Item.Infrastructure.CsePersonNumber ?? "";
            local.ConvertDateDateWorkArea.Date =
              import.DmdcProMatchResponse.ChMedicalCoverageEndDate;
            UseCabConvertDate2String();

            if (Equal(local.ConvertDateTextWorkArea.Text8, "00000000"))
            {
              local.ConvertDateTextWorkArea.Text8 = "";
            }

            local.Dmdc.Text32 = local.ConvertDateTextWorkArea.Text8 + " TERMINATION DATE";
              
            local.Dmdc.Text40 = "NO MILITARY EMPLOYEE/SPONSOR PROVIDED";
            local.Infrastructure.Detail = TrimEnd(local.Dmdc.Text40) + " " + TrimEnd
              (local.Dmdc.Text32);

            break;
          case "OTHCOVENDSYS":
            local.PersonUpdated.Count = 0;

            if (local.PersonsUpdated.Count < 1)
            {
              // no previous ending record found
              if (ReadInfrastructure49())
              {
                ++local.PersonUpdated.Count;
              }

              if (local.PersonUpdated.Count < 1)
              {
                local.PersonUpdated.Count = 0;

                if (ReadInfrastructure48())
                {
                  ++local.PersonUpdated.Count;
                }

                if (local.PersonUpdated.Count < 1)
                {
                  // do nothing, there is no beginning of pf coverage to 
                  // reverse, the normal state of records is that there is no
                  // coverage
                  continue;
                }
                else
                {
                  // add OTHCOVENDSYS record
                  local.Infrastructure.ReasonCode =
                    local.Group.Item.Grp2NdCode.ReasonCode;
                }
              }
              else
              {
                // add OTHCOVEND record
                local.Infrastructure.ReasonCode =
                  local.Group.Item.Infrastructure.ReasonCode;
              }
            }
            else
            {
              // found a ending record, but which one?
              // is there even a begin record?
              local.PersonUpdated.Count = 0;

              if (ReadInfrastructure52())
              {
                ++local.PersonUpdated.Count;
              }

              if (local.PersonUpdated.Count >= 1)
              {
                if (Lt(entities.SecondPass.CreatedTimestamp,
                  entities.Infrastructure.CreatedTimestamp))
                {
                  // THERE IS A OTHCOVBEGINSYS/OTHCOVBEGIN RECORD BUT IT HAS 
                  // ALREADY BEEN CLOSED OUT
                  continue;
                }
                else
                {
                  // add record
                  if (Equal(entities.SecondPass.ReasonCode, "OTHCOVBEGINSYS"))
                  {
                    local.Infrastructure.ReasonCode =
                      local.Group.Item.Infrastructure.ReasonCode;
                  }
                  else
                  {
                    local.Infrastructure.ReasonCode =
                      local.Group.Item.Grp2NdCode.ReasonCode;
                  }
                }
              }
              else
              {
                // THERE WAS NOT A OTHCOVBEGINSYS OR A OTHCOVBEGIN RECORD SO 
                // THERE IS NO REASON TO ADD A ENDING RECORD.
                continue;
              }
            }

            local.Infrastructure.CsePersonNumber =
              local.Group.Item.Infrastructure.CsePersonNumber ?? "";
            local.ConvertDateDateWorkArea.Date =
              import.DmdcProMatchResponse.ChMedicalCoverageEndDate;
            UseCabConvertDate2String();

            if (Equal(local.ConvertDateTextWorkArea.Text8, "00000000"))
            {
              local.ConvertDateTextWorkArea.Text8 = "";
            }

            local.Dmdc.Text32 = local.ConvertDateTextWorkArea.Text8 + " TERMINATION DATE";
              
            local.Dmdc.Text40 = "NO MILITARY EMPLOYEE/SPONSOR PROVIDED";
            local.Infrastructure.Detail = TrimEnd(local.Dmdc.Text40) + " " + TrimEnd
              (local.Dmdc.Text32);

            break;
          case "NCPINMILT":
            if (local.PersonsUpdated.Count < 1)
            {
              // add record
            }
            else
            {
              local.PersonUpdated.Count = 0;

              if (ReadInfrastructure61())
              {
                ++local.PersonUpdated.Count;
              }

              if (local.PersonUpdated.Count >= 1)
              {
                if (Lt(entities.SecondPass.CreatedTimestamp,
                  entities.Infrastructure.CreatedTimestamp))
                {
                  // do nothing, already have record
                  continue;
                }
                else
                {
                  // add record
                }
              }
              else
              {
                // EITHER THERE WAS NOT A ENDING TO  A NCPINMILT RECORD OR THERE
                // IS A NCPINMILTSYS RECORD, NOW HAVE TO CHECK FOR IT.
                local.PersonUpdated.Count = 0;

                if (ReadInfrastructure1())
                {
                  ++local.PersonUpdated.Count;
                }

                if (local.PersonUpdated.Count >= 1)
                {
                  local.PersonUpdated.Count = 0;

                  // FOUND A NCPINMILTSYS RECORD,  NOW HAVE TO CHECK TO SEE IF 
                  // IT IS CLOSED.
                  if (ReadInfrastructure27())
                  {
                    ++local.PersonUpdated.Count;
                  }

                  if (local.PersonUpdated.Count >= 1)
                  {
                    // FOUND A CLOSE FOR A NCPINMILTNSYS RECORD BUT IS IT FOR 
                    // THE LATEST?
                    if (Lt(entities.Infrastructure.CreatedTimestamp,
                      entities.SecondPass.CreatedTimestamp))
                    {
                      // THIS NCPCINMILTSYS IS CLOSED SO IT IS OK TO ADD A 
                      // NCPINMILT RECORD.
                    }
                    else
                    {
                      // FOUND A OPEN NCPINMILTNSYS RECORD SO WE DO NOT WANT TO 
                      // OPEN A RECORD.
                      continue;
                    }
                  }
                  else
                  {
                    // DID NOT FIND A CLOSED RECORD FOR THE NCPINMILTSYS RECORD 
                    // SO IT IS STILL ACTIVE, THEREFORE WE DO NOT WANT TO ADD A
                    // RECORD.
                    continue;
                  }
                }
                else
                {
                  // THERE IS A NCPINMILT BUT THERE IS NO END TO IT,  WE WILL 
                  // NOT ADD ANOTHER RECORD.
                  continue;
                }
              }
            }

            local.PersonQualified.Count = 0;
            local.Infrastructure.CsePersonNumber =
              local.Group.Item.Infrastructure.CsePersonNumber ?? "";
            local.Infrastructure.ReasonCode =
              local.Group.Item.Infrastructure.ReasonCode;
            local.Dmdc.Text10 = local.ChNumber.CsePersonNumber ?? Spaces(10);
            local.Dmdc.Text50 =
              TrimEnd(import.DmdcProMatchResponse.ChLastName) + ", " + TrimEnd
              (import.DmdcProMatchResponse.ChFirstName) + " " + Substring
              (import.DmdcProMatchResponse.ChMiddleName,
              DmdcProMatchResponse.ChMiddleName_MaxLength, 1, 1);
            local.Dmdc.Text32 = "NO MILITARY DEPENDENT";
            local.Infrastructure.Detail = TrimEnd(local.Dmdc.Text50) + " " + TrimEnd
              (local.Dmdc.Text10) + " " + TrimEnd(local.Dmdc.Text32);

            break;
          case "NCPINMILTSYS":
            if (local.PersonsUpdated.Count < 1)
            {
              // add record
            }
            else
            {
              local.PersonUpdated.Count = 0;

              if (ReadInfrastructure62())
              {
                ++local.PersonUpdated.Count;
              }

              if (local.PersonUpdated.Count >= 1)
              {
                if (Lt(entities.SecondPass.CreatedTimestamp,
                  entities.Infrastructure.CreatedTimestamp))
                {
                  // do nothing, already have record
                  continue;
                }
                else
                {
                  // add record
                }
              }
              else
              {
                // EITHER THERE WAS NOT A ENDING TO  A NCPINMILTSYS RECORD OR 
                // THERE IS A NCPINMILT RECORD, NOW HAVE TO CHECK FOR IT.
                local.PersonUpdated.Count = 0;

                if (ReadInfrastructure1())
                {
                  ++local.PersonUpdated.Count;
                }

                if (local.PersonUpdated.Count >= 1)
                {
                  local.PersonUpdated.Count = 0;

                  // FOUND A NCPINMILT RECORD,  NOW HAVE TO CHECK TO SEE IF IT 
                  // IS CLOSED.
                  if (ReadInfrastructure26())
                  {
                    ++local.PersonUpdated.Count;
                  }

                  if (local.PersonUpdated.Count >= 1)
                  {
                    // FOUND A CLOSE FOR A NCPINMILTN RECORD BUT IS IT FOR THE 
                    // LATEST?
                    if (Lt(entities.Infrastructure.CreatedTimestamp,
                      entities.SecondPass.CreatedTimestamp))
                    {
                      // THIS NCPCINMILT IS CLOSED SO IT IS OK TO ADD A 
                      // NCPINMILTSYS RECORD.
                    }
                    else
                    {
                      // FOUND A OPEN NCPINMILTN RECORD SO WE DO NOT WANT TO 
                      // OPEN A RECORD.
                      continue;
                    }
                  }
                  else
                  {
                    // DID NOT FIND A CLOSED RECORD FOR THE NCPINMILT RECORD SO 
                    // IT IS STILL ACTIVE, THEREFORE WE DO NOT WANT TO ADD A
                    // RECORD.
                    continue;
                  }
                }
                else
                {
                  // THERE IS A NCPINMILSYS BUT THERE IS NO END TO IT,  WE WILL 
                  // NOT ADD ANOTHER RECORD.
                  return;
                }
              }
            }

            local.PersonQualified.Count = 0;
            local.Infrastructure.CsePersonNumber =
              local.Group.Item.Infrastructure.CsePersonNumber ?? "";
            local.Infrastructure.ReasonCode =
              local.Group.Item.Infrastructure.ReasonCode;
            local.Dmdc.Text10 = local.ChNumber.CsePersonNumber ?? Spaces(10);
            local.Dmdc.Text50 =
              TrimEnd(import.DmdcProMatchResponse.ChLastName) + ", " + TrimEnd
              (import.DmdcProMatchResponse.ChFirstName) + " " + Substring
              (import.DmdcProMatchResponse.ChMiddleName,
              DmdcProMatchResponse.ChMiddleName_MaxLength, 1, 1);
            local.Dmdc.Text32 = "NO MILITARY DEPENDENT";
            local.Infrastructure.Detail = TrimEnd(local.Dmdc.Text50) + " " + TrimEnd
              (local.Dmdc.Text10) + " " + TrimEnd(local.Dmdc.Text32);

            break;
          case "CPINMILT":
            if (local.PersonsUpdated.Count < 1)
            {
              // add record
            }
            else
            {
              local.PersonUpdated.Count = 0;

              if (ReadInfrastructure57())
              {
                ++local.PersonUpdated.Count;
              }

              if (local.PersonUpdated.Count >= 1)
              {
                if (Lt(entities.SecondPass.CreatedTimestamp,
                  entities.Infrastructure.CreatedTimestamp))
                {
                  // do nothing, already have record
                  continue;
                }
                else
                {
                  // add record
                }
              }
              else
              {
                // do nothing
                continue;
              }
            }

            local.PersonQualified.Count = 0;
            local.Infrastructure.ReasonCode =
              local.Group.Item.Infrastructure.ReasonCode;
            local.Infrastructure.CsePersonNumber =
              local.Group.Item.Infrastructure.CsePersonNumber ?? "";
            local.Dmdc.Text10 = local.ChNumber.CsePersonNumber ?? Spaces(10);
            local.Dmdc.Text50 =
              TrimEnd(import.DmdcProMatchResponse.ChLastName) + ", " + TrimEnd
              (import.DmdcProMatchResponse.ChFirstName) + " " + Substring
              (import.DmdcProMatchResponse.ChMiddleName,
              DmdcProMatchResponse.ChMiddleName_MaxLength, 1, 1);
            local.Dmdc.Text32 = "NO MILITARY DEPENDENT";
            local.Infrastructure.Detail = TrimEnd(local.Dmdc.Text50) + " " + TrimEnd
              (local.Dmdc.Text10) + " " + TrimEnd(local.Dmdc.Text32);

            break;
          case "PFINMILT":
            if (local.PersonsUpdated.Count < 1)
            {
              // add record
            }
            else
            {
              local.PersonUpdated.Count = 0;

              if (ReadInfrastructure66())
              {
                ++local.PersonUpdated.Count;
              }

              if (local.PersonUpdated.Count >= 1)
              {
                if (Lt(entities.SecondPass.CreatedTimestamp,
                  entities.Infrastructure.CreatedTimestamp))
                {
                  // do nothing, already have record
                  continue;
                }
                else
                {
                  // add record
                }
              }
              else
              {
                // EITHER THERE WAS NOT A ENDING TO  A PFINMILT RECORD OR THERE 
                // IS A PFINMILTSYS RECORD, NOW HAVE TO CHECK FOR IT.
                local.PersonUpdated.Count = 0;

                if (ReadInfrastructure1())
                {
                  ++local.PersonUpdated.Count;
                }

                if (local.PersonUpdated.Count >= 1)
                {
                  local.PersonUpdated.Count = 0;

                  // FOUND A PFINMILTSYS RECORD,  NOW HAVE TO CHECK TO SEE IF IT
                  // IS CLOSED.
                  if (ReadInfrastructure34())
                  {
                    ++local.PersonUpdated.Count;
                  }

                  if (local.PersonUpdated.Count >= 1)
                  {
                    // FOUND A CLOSE FOR A PFINMILTNSYS RECORD BUT IS IT FOR THE
                    // LATEST?
                    if (Lt(entities.Infrastructure.CreatedTimestamp,
                      entities.SecondPass.CreatedTimestamp))
                    {
                      // THIS PFCINMILTSYS IS CLOSED SO IT IS OK TO ADD A 
                      // PFINMILT RECORD.
                    }
                    else
                    {
                      // FOUND A OPEN PFINMILTNSYS RECORD SO WE DO NOT WANT TO 
                      // OPEN A RECORD.
                      continue;
                    }
                  }
                  else
                  {
                    // DID NOT FIND A CLOSED RECORD FOR THE PFINMILTSYS RECORD 
                    // SO IT IS STILL ACTIVE, THEREFORE WE DO NOT WANT TO ADD A
                    // RECORD.
                    continue;
                  }
                }
                else
                {
                  // THERE IS A PFINMIL BUT THERE IS NO END TO IT,  WE WILL NOT 
                  // ADD ANOTHER RECORD.
                  continue;
                }
              }
            }

            local.PersonQualified.Count = 0;
            local.Infrastructure.CsePersonNumber =
              local.Group.Item.Infrastructure.CsePersonNumber ?? "";
            local.Infrastructure.ReasonCode =
              local.Group.Item.Infrastructure.ReasonCode;
            local.Dmdc.Text10 = local.ChNumber.CsePersonNumber ?? Spaces(10);
            local.Dmdc.Text50 =
              TrimEnd(import.DmdcProMatchResponse.ChLastName) + ", " + TrimEnd
              (import.DmdcProMatchResponse.ChFirstName) + " " + Substring
              (import.DmdcProMatchResponse.ChMiddleName,
              DmdcProMatchResponse.ChMiddleName_MaxLength, 1, 1);
            local.Dmdc.Text32 = "NO MILITARY DEPENDENT";
            local.Infrastructure.Detail = TrimEnd(local.Dmdc.Text50) + " " + TrimEnd
              (local.Dmdc.Text10) + " " + TrimEnd(local.Dmdc.Text32);

            break;
          case "PFINMILTSYS":
            if (local.PersonsUpdated.Count < 1)
            {
              // add record
            }
            else
            {
              local.PersonUpdated.Count = 0;

              if (ReadInfrastructure67())
              {
                ++local.PersonUpdated.Count;
              }

              if (local.PersonUpdated.Count >= 1)
              {
                if (Lt(entities.SecondPass.CreatedTimestamp,
                  entities.Infrastructure.CreatedTimestamp))
                {
                  // do nothing, already have record
                  continue;
                }
                else
                {
                  // add record
                }
              }
              else
              {
                // EITHER THERE WAS NOT A ENDING TO  A PFINMILTSYS RECORD OR 
                // THERE IS A PFINMILT RECORD, NOW HAVE TO CHECK FOR IT.
                local.PersonUpdated.Count = 0;

                if (ReadInfrastructure1())
                {
                  ++local.PersonUpdated.Count;
                }

                if (local.PersonUpdated.Count >= 1)
                {
                  local.PersonUpdated.Count = 0;

                  // FOUND A PFINMILT RECORD,  NOW HAVE TO CHECK TO SEE IF IT IS
                  // CLOSED.
                  if (ReadInfrastructure33())
                  {
                    ++local.PersonUpdated.Count;
                  }

                  if (local.PersonUpdated.Count >= 1)
                  {
                    // FOUND A CLOSE FOR A PFINMILTN RECORD BUT IS IT FOR THE 
                    // LATEST?
                    if (Lt(entities.Infrastructure.CreatedTimestamp,
                      entities.SecondPass.CreatedTimestamp))
                    {
                      // THIS PFCINMILT IS CLOSED SO IT IS OK TO ADD A 
                      // PFINMILTSYS RECORD.
                    }
                    else
                    {
                      // FOUND A OPEN PFINMILTN RECORD SO WE DO NOT WANT TO OPEN
                      // A RECORD.
                      continue;
                    }
                  }
                  else
                  {
                    // DID NOT FIND A CLOSED RECORD FOR THE PFINMILT RECORD SO 
                    // IT IS STILL ACTIVE, THEREFORE WE DO NOT WANT TO ADD A
                    // RECORD.
                    continue;
                  }
                }
                else
                {
                  // THERE IS A PFINMILSYS BUT THERE IS NO END TO IT,  WE WILL 
                  // NOT ADD ANOTHER RECORD.
                  continue;
                }
              }
            }

            local.PersonQualified.Count = 0;
            local.Infrastructure.ReasonCode =
              local.Group.Item.Infrastructure.ReasonCode;
            local.Infrastructure.CsePersonNumber =
              local.Group.Item.Infrastructure.CsePersonNumber ?? "";
            local.Dmdc.Text10 = local.ChNumber.CsePersonNumber ?? Spaces(10);
            local.Dmdc.Text50 =
              TrimEnd(import.DmdcProMatchResponse.ChLastName) + ", " + TrimEnd
              (import.DmdcProMatchResponse.ChFirstName) + " " + Substring
              (import.DmdcProMatchResponse.ChMiddleName,
              DmdcProMatchResponse.ChMiddleName_MaxLength, 1, 1);
            local.Dmdc.Text32 = "NO MILITARY DEPENDENT";
            local.Infrastructure.Detail = TrimEnd(local.Dmdc.Text50) + " " + TrimEnd
              (local.Dmdc.Text10) + " " + TrimEnd(local.Dmdc.Text32);

            break;
          case "NCPOUTMILT":
            local.PersonUpdated.Count = 0;

            if (local.PersonsUpdated.Count < 1)
            {
              // no previous ending record found
              if (ReadInfrastructure14())
              {
                ++local.PersonUpdated.Count;
              }

              if (local.PersonUpdated.Count < 1)
              {
                local.PersonUpdated.Count = 0;

                if (ReadInfrastructure15())
                {
                  ++local.PersonUpdated.Count;
                }

                if (local.PersonUpdated.Count < 1)
                {
                  // do nothing, there is no beginning of ncp in military to 
                  // reverse, the normal state of records is that there is no
                  // coverage
                  continue;
                }
                else
                {
                  // add NCPOUTMILTSYS record
                  local.Infrastructure.ReasonCode =
                    local.Group.Item.Grp2NdCode.ReasonCode;
                }
              }
              else
              {
                // add NCPOUTMILT record
                local.Infrastructure.ReasonCode =
                  local.Group.Item.Infrastructure.ReasonCode;
              }
            }
            else
            {
              // found a ending record, but which one?
              // is there even a begin record?
              local.PersonUpdated.Count = 0;

              if (ReadInfrastructure24())
              {
                ++local.PersonUpdated.Count;
              }

              if (local.PersonUpdated.Count >= 1)
              {
                if (Lt(entities.SecondPass.CreatedTimestamp,
                  entities.Infrastructure.CreatedTimestamp))
                {
                  // THERE IS A NCPINMILT/NCPINMILTSYS RECORD BUT IT HAS ALREADY
                  // BEEN CLOSED OUT
                  continue;
                }
                else
                {
                  // add record
                  if (Equal(entities.SecondPass.ReasonCode, "NCPINMILT"))
                  {
                    local.Infrastructure.ReasonCode =
                      local.Group.Item.Infrastructure.ReasonCode;
                  }
                  else
                  {
                    local.Infrastructure.ReasonCode =
                      local.Group.Item.Grp2NdCode.ReasonCode;
                  }
                }
              }
              else
              {
                // THERE WAS NOT A NCPINMILT OR A NCPINMILTSYS RECORD SO THERE 
                // IS NO REASON TO ADD A ENDING RECORD.
                continue;
              }
            }

            local.PersonQualified.Count = 0;
            local.Infrastructure.ReasonCode =
              local.Group.Item.Infrastructure.ReasonCode;
            local.Infrastructure.CsePersonNumber =
              local.Group.Item.Infrastructure.CsePersonNumber ?? "";
            local.Infrastructure.Detail =
              "MEDICAL BENFITS NO LONGER AVAILABLE FOR MILITARY EMPLOYEE";

            break;
          case "NCPOUTMILTSYS":
            local.PersonUpdated.Count = 0;

            if (local.PersonsUpdated.Count < 1)
            {
              // no previous ending record found
              if (ReadInfrastructure15())
              {
                ++local.PersonUpdated.Count;
              }

              if (local.PersonUpdated.Count < 1)
              {
                local.PersonUpdated.Count = 0;

                if (ReadInfrastructure14())
                {
                  ++local.PersonUpdated.Count;
                }

                if (local.PersonUpdated.Count < 1)
                {
                  // do nothing, there is no beginning of ncp in military to 
                  // reverse, the normal state of records is that there is no
                  // coverage
                  continue;
                }
                else
                {
                  // add NCPOUTMILT record
                  local.Infrastructure.ReasonCode =
                    local.Group.Item.Grp2NdCode.ReasonCode;
                }
              }
              else
              {
                // add NCPOUTMILTSYS record
                local.Infrastructure.ReasonCode =
                  local.Group.Item.Infrastructure.ReasonCode;
              }
            }
            else
            {
              // found a ending record, but which one?
              // is there even a begin record?
              local.PersonUpdated.Count = 0;

              if (ReadInfrastructure25())
              {
                ++local.PersonUpdated.Count;
              }

              if (local.PersonUpdated.Count >= 1)
              {
                if (Lt(entities.SecondPass.CreatedTimestamp,
                  entities.Infrastructure.CreatedTimestamp))
                {
                  // THERE IS A NCPINMILTSYS/NCPINMILT RECORD BUT IT HAS ALREADY
                  // BEEN CLOSED OUT
                  continue;
                }
                else
                {
                  // add record
                  if (Equal(entities.SecondPass.ReasonCode, "NCPINMILTSYS"))
                  {
                    local.Infrastructure.ReasonCode =
                      local.Group.Item.Infrastructure.ReasonCode;
                  }
                  else
                  {
                    local.Infrastructure.ReasonCode =
                      local.Group.Item.Grp2NdCode.ReasonCode;
                  }
                }
              }
              else
              {
                // THERE WAS NOT A NCPINMILTSYS OR A NCPINMILT RECORD SO THERE 
                // IS NO REASON TO ADD A ENDING RECORD.
                continue;
              }
            }

            local.PersonQualified.Count = 0;
            local.Infrastructure.CsePersonNumber =
              local.Group.Item.Infrastructure.CsePersonNumber ?? "";
            local.Infrastructure.Detail =
              "MEDICAL BENFITS NO LONGER AVAIBLE FOR MILITARY EMPLOYEE";

            break;
          case "CPOUTMILT":
            local.PersonUpdated.Count = 0;

            if (local.PersonsUpdated.Count < 1)
            {
              if (ReadInfrastructure56())
              {
                ++local.PersonUpdated.Count;
              }

              if (local.PersonUpdated.Count < 1)
              {
                // do nothing, there never was a beginning so it can not be 
                // eneded
                continue;
              }
              else
              {
                // add record
              }
            }
            else
            {
              if (ReadInfrastructure7())
              {
                ++local.PersonUpdated.Count;
              }

              if (local.PersonUpdated.Count >= 1)
              {
                if (Lt(entities.SecondPass.CreatedTimestamp,
                  entities.Infrastructure.CreatedTimestamp))
                {
                  // do nothing, already have record
                  continue;
                }
                else
                {
                  // add record
                }
              }
              else
              {
                // do nothing, never in the military
                continue;
              }
            }

            local.PersonQualified.Count = 0;
            local.Infrastructure.ReasonCode =
              local.Group.Item.Infrastructure.ReasonCode;
            local.Infrastructure.CsePersonNumber =
              local.Group.Item.Infrastructure.CsePersonNumber ?? "";
            local.Infrastructure.Detail =
              "MEDICAL BENFITS NO LONGER AVAILABLE FOR MILITARY EMPLOYEE";

            break;
          case "PFOUTMILT":
            local.PersonUpdated.Count = 0;

            if (local.PersonsUpdated.Count < 1)
            {
              // no previous ending record found
              if (ReadInfrastructure18())
              {
                ++local.PersonUpdated.Count;
              }

              if (local.PersonUpdated.Count < 1)
              {
                local.PersonUpdated.Count = 0;

                if (ReadInfrastructure19())
                {
                  ++local.PersonUpdated.Count;
                }

                if (local.PersonUpdated.Count < 1)
                {
                  // do nothing, there is no beginning of pf in military to 
                  // reverse, the normal state of records is that there is no
                  // coverage
                  continue;
                }
                else
                {
                  // add PFOUTMILTSYS record
                  local.Infrastructure.ReasonCode =
                    local.Group.Item.Grp2NdCode.ReasonCode;
                }
              }
              else
              {
                // add PFOUTMILT record
                local.Infrastructure.ReasonCode =
                  local.Group.Item.Infrastructure.ReasonCode;
              }
            }
            else
            {
              // found a ending record, but which one?
              // is there even a begin record?
              local.PersonUpdated.Count = 0;

              if (ReadInfrastructure31())
              {
                ++local.PersonUpdated.Count;
              }

              if (local.PersonUpdated.Count >= 1)
              {
                if (Lt(entities.SecondPass.CreatedTimestamp,
                  entities.Infrastructure.CreatedTimestamp))
                {
                  // THERE IS A PFINMILT/PFINMILTSYS RECORD BUT IT HAS ALREADY 
                  // BEEN CLOSED OUT
                  continue;
                }
                else
                {
                  // add record
                  if (Equal(entities.SecondPass.ReasonCode, "PFINMILT"))
                  {
                    local.Infrastructure.ReasonCode =
                      local.Group.Item.Infrastructure.ReasonCode;
                  }
                  else
                  {
                    local.Infrastructure.ReasonCode =
                      local.Group.Item.Grp2NdCode.ReasonCode;
                  }
                }
              }
              else
              {
                // THERE WAS NOT A PFINMILT OR A PFINMILTSYS RECORD SO THERE IS 
                // NO REASON TO ADD A ENDING RECORD.
                continue;
              }
            }

            local.PersonQualified.Count = 0;
            local.Infrastructure.CsePersonNumber =
              local.Group.Item.Infrastructure.CsePersonNumber ?? "";
            local.Infrastructure.Detail =
              "MEDICAL BENFITS NO LONGER AVAILABLE FOR MILITARY EMPLOYEE";

            break;
          case "PFOUTMILTSYS":
            local.PersonUpdated.Count = 0;

            if (local.PersonsUpdated.Count < 1)
            {
              // no previous ending record found
              if (ReadInfrastructure19())
              {
                ++local.PersonUpdated.Count;
              }

              if (local.PersonUpdated.Count < 1)
              {
                local.PersonUpdated.Count = 0;

                if (ReadInfrastructure18())
                {
                  ++local.PersonUpdated.Count;
                }

                if (local.PersonUpdated.Count < 1)
                {
                  // do nothing, there is no beginning of pf in military to 
                  // reverse, the normal state of records is that there is no
                  // coverage
                  continue;
                }
                else
                {
                  // add PFOUTMILT record
                  local.Infrastructure.ReasonCode =
                    local.Group.Item.Grp2NdCode.ReasonCode;
                }
              }
              else
              {
                // add PFOUTMILTSYS record
                local.Infrastructure.ReasonCode =
                  local.Group.Item.Infrastructure.ReasonCode;
              }
            }
            else
            {
              // found a ending record, but which one?
              // is there even a begin record?
              local.PersonUpdated.Count = 0;

              if (ReadInfrastructure32())
              {
                ++local.PersonUpdated.Count;
              }

              if (local.PersonUpdated.Count >= 1)
              {
                if (Lt(entities.SecondPass.CreatedTimestamp,
                  entities.Infrastructure.CreatedTimestamp))
                {
                  // THERE IS A PFINMILTSYS/PFINMILT RECORD BUT IT HAS ALREADY 
                  // BEEN CLOSED OUT
                  continue;
                }
                else
                {
                  // add record
                  if (Equal(entities.SecondPass.ReasonCode, "PFINMILTSYS"))
                  {
                    local.Infrastructure.ReasonCode =
                      local.Group.Item.Infrastructure.ReasonCode;
                  }
                  else
                  {
                    local.Infrastructure.ReasonCode =
                      local.Group.Item.Grp2NdCode.ReasonCode;
                  }
                }
              }
              else
              {
                // THERE WAS NOT A PFINMILTSYS OR A PFINMILT RECORD SO THERE IS 
                // NO REASON TO ADD A ENDING RECORD.
                continue;
              }
            }

            local.PersonQualified.Count = 0;
            local.Infrastructure.CsePersonNumber =
              local.Group.Item.Infrastructure.CsePersonNumber ?? "";
            local.Infrastructure.Detail =
              "MEDICAL BENFITS NO LONGER AVAILABLE FOR MILITARY EMPLOYEE";

            break;
          default:
            // ********************************************************************
            // REF:CQ237   Who: Raj S   When: 05/02/2008    Begin Change
            // ********************************************************************
            if (Equal(local.Group.Item.Infrastructure.ReasonCode, 1, 6, "OTHCOV"))
              
            {
              if (Equal(local.Group.Item.Infrastructure.ReasonCode, 9, 3, "BEG"))
                
              {
                local.ReadReasonCode1.ReasonCode =
                  Substring(local.Group.Item.Infrastructure.ReasonCode,
                  Infrastructure.ReasonCode_MaxLength, 1, 8) + "END";
                local.ReasReasonCode2.ReasonCode =
                  TrimEnd(local.ReadReasonCode1.ReasonCode) + "SYS";

                if (local.PersonsUpdated.Count < 1)
                {
                  // add record
                }
                else
                {
                  local.PersonUpdated.Count = 0;

                  if (ReadInfrastructure50())
                  {
                    ++local.PersonUpdated.Count;
                  }

                  if (local.PersonUpdated.Count >= 1)
                  {
                    // ***********************************************************************
                    // There is a opposite reason code record found for the 
                    // selected reason
                    // record.  (i.e. if the selected is END then there is BEGIN
                    // reason record
                    // found).  Check to see the opposite reason record is 
                    // created later.
                    // ***********************************************************************
                    if (Lt(entities.SecondPass.CreatedTimestamp,
                      entities.Infrastructure.CreatedTimestamp))
                    {
                      // do nothing, already have record
                      continue;
                    }
                    else
                    {
                      // add record
                    }
                  }
                  else
                  {
                    // ***********************************************************************
                    // There is NO opposite reason code record found for the 
                    // selected reason
                    // record.  (i.e. if the selected is BEGIN then there is END
                    // reason record
                    // found).
                    // ***********************************************************************
                    local.PersonUpdated.Count = 0;

                    if (ReadInfrastructure5())
                    {
                      ++local.PersonUpdated.Count;
                    }

                    if (local.PersonUpdated.Count >= 1)
                    {
                      // ************************************************************************
                      // Found a begin record but it is not closed, begin record
                      // is in open state
                      // do not create new begin reason record.
                      // ************************************************************************
                      local.PersonUpdated.Count = 0;

                      // FOUND A OTHCOVBEGIN RECORD,  NOW HAVE TO CHECK TO SEE 
                      // IF IT IS CLOSED.
                      if (ReadInfrastructure58())
                      {
                        ++local.PersonUpdated.Count;
                      }

                      if (local.PersonUpdated.Count >= 1)
                      {
                        // FOUND A CLOSE FOR A OTHCOVXXBEG RECORD BUT IS IT FOR 
                        // THE LATEST?
                        if (Lt(entities.Infrastructure.CreatedTimestamp,
                          entities.SecondPass.CreatedTimestamp))
                        {
                          // THIS OTHCOVXXBEG IS CLOSED SO IT IS OK TO ADD A 
                          // OTHCOVXXBEGSYS RECORD.
                          if (Equal(entities.SecondPass.ReasonCode,
                            local.ReadReasonCode1.ReasonCode))
                          {
                            local.Infrastructure.ReasonCode =
                              local.Group.Item.Infrastructure.ReasonCode;
                          }
                          else
                          {
                            local.Infrastructure.ReasonCode =
                              local.Group.Item.Grp2NdCode.ReasonCode;
                          }
                        }
                        else
                        {
                          // FOUND A OPEN OTHCOVBEGIN RECORD SO WE DO NOT WANT 
                          // TO OPEN A RECORD.
                          continue;
                        }
                      }
                      else
                      {
                        // DID NOT FIND A CLOSED RECORD FOR THE OTHCOVBEGIN 
                        // RECORD SO IT IS STILL ACTIVE, THEREFORE WE DO NOT
                        // WANT TO ADD A RECORD.
                        continue;
                      }
                    }
                    else
                    {
                      // THERE IS A OTHCOVBEGIN BUT THERE IS NO END TO IT,  WE 
                      // WILL NOT ADD ANOTHER RECORD.
                      continue;
                    }
                  }
                }
              }
              else
              {
                local.ReadReasonCode1.ReasonCode =
                  Substring(local.Group.Item.Infrastructure.ReasonCode,
                  Infrastructure.ReasonCode_MaxLength, 1, 8) + "BEG";
                local.ReasReasonCode2.ReasonCode =
                  TrimEnd(local.ReadReasonCode1.ReasonCode) + "SYS";
                local.PersonUpdated.Count = 0;

                if (local.PersonsUpdated.Count < 1)
                {
                  // no previous ending record found
                  if (ReadInfrastructure46())
                  {
                    ++local.PersonUpdated.Count;
                  }

                  if (local.PersonUpdated.Count < 1)
                  {
                    local.PersonUpdated.Count = 0;

                    if (ReadInfrastructure47())
                    {
                      ++local.PersonUpdated.Count;
                    }

                    if (local.PersonUpdated.Count < 1)
                    {
                      // do nothing, there is no beginning of other coverage to 
                      // reverse, the normal state of records is that there is
                      // no coverage
                      continue;
                    }
                    else
                    {
                      // add OTHCOVENDSYS record
                      local.Infrastructure.ReasonCode =
                        local.Group.Item.Grp2NdCode.ReasonCode;
                    }
                  }
                  else
                  {
                    // add OTHCOVEND record
                    local.Infrastructure.ReasonCode =
                      local.Group.Item.Infrastructure.ReasonCode;
                  }
                }
                else
                {
                  // found a ending record, but which one?
                  // is there even a begin record?
                  local.PersonUpdated.Count = 0;

                  if (ReadInfrastructure50())
                  {
                    ++local.PersonUpdated.Count;
                  }

                  if (local.PersonUpdated.Count >= 1)
                  {
                    if (Lt(entities.SecondPass.CreatedTimestamp,
                      entities.Infrastructure.CreatedTimestamp))
                    {
                      // THERE IS A OTHCOVXXBEG/OTHCOVXXBEGSYS RECORD BUT IT HAS
                      // ALREADY BEEN CLOSED OUT
                      continue;
                    }
                    else
                    {
                      // add record
                      if (Equal(entities.SecondPass.ReasonCode,
                        local.ReadReasonCode1.ReasonCode))
                      {
                        local.Infrastructure.ReasonCode =
                          local.Group.Item.Infrastructure.ReasonCode;
                      }
                      else
                      {
                        local.Infrastructure.ReasonCode =
                          local.Group.Item.Grp2NdCode.ReasonCode;
                      }
                    }
                  }
                  else
                  {
                    // THERE WAS NOT A OTHCOVXXBEG OR A OTHCOVXXBEGSYS RECORD SO
                    // THERE IS NO REASON TO ADD A ENDING RECORD.
                    continue;
                  }
                }
              }

              local.Infrastructure.ReasonCode =
                local.Group.Item.Infrastructure.ReasonCode;
              local.Infrastructure.CsePersonNumber =
                local.Group.Item.Infrastructure.CsePersonNumber ?? "";
              local.ConvertDateDateWorkArea.Date =
                import.DmdcProMatchResponse.ChMedicalCoverageBeginDate;
              UseCabConvertDate2String();

              if (Equal(local.ConvertDateTextWorkArea.Text8, "00000000"))
              {
                local.ConvertDateTextWorkArea.Text8 = "";
              }

              // *** CQ237 Changed by Arun on 06/04/2008 ***
              // *** Eligibility and Termination were shortened so that it could
              // be seen on the HIST Scrren ***
              if (Equal(local.Group.Item.Infrastructure.ReasonCode, 9, 3, "END"))
                
              {
                local.Dmdc.Text32 = local.ConvertDateTextWorkArea.Text8 + " TERM DATE";
                  
              }
              else
              {
                local.Dmdc.Text32 = local.ConvertDateTextWorkArea.Text8 + " ELIG DATE";
                  
              }

              if (IsEmpty(import.DmdcProMatchResponse.ChSponsorFirstName) && IsEmpty
                (import.DmdcProMatchResponse.ChSponsorLastNameSuffix) && IsEmpty
                (import.DmdcProMatchResponse.ChSponsorLastName) && IsEmpty
                (import.DmdcProMatchResponse.ChSponsorMiddleName) && IsEmpty
                (import.DmdcProMatchResponse.ChSponsorSsn))
              {
                local.Dmdc.Text50 = "NO MILITARY EMPLOYEE/SPONSOR PROVIDED";
              }
              else
              {
                // *** CQ237 Arun added If condition so that it would not 
                // display comma if the sponsor name is spaces ***
                if (!IsEmpty(import.DmdcProMatchResponse.ChSponsorFirstName) ||
                  !
                  IsEmpty(import.DmdcProMatchResponse.ChSponsorLastNameSuffix) ||
                  !IsEmpty(import.DmdcProMatchResponse.ChSponsorLastName) || !
                  IsEmpty(import.DmdcProMatchResponse.ChSponsorMiddleName))
                {
                  local.Dmdc.Text50 =
                    TrimEnd(import.DmdcProMatchResponse.ChSponsorLastName) + ","
                    + TrimEnd
                    (import.DmdcProMatchResponse.ChSponsorFirstName) + TrimEnd
                    (import.DmdcProMatchResponse.ChSponsorMiddleName) + TrimEnd
                    (import.DmdcProMatchResponse.ChSponsorLastNameSuffix);

                  if (!IsEmpty(import.DmdcProMatchResponse.ChSponsorSsn))
                  {
                    local.Dmdc.Text50 =
                      TrimEnd(Substring(local.Dmdc.Text50, 1, 36)) + " SSN#" + import
                      .DmdcProMatchResponse.ChSponsorSsn;
                  }
                }
                else if (!IsEmpty(import.DmdcProMatchResponse.ChSponsorSsn))
                {
                  // ***CQ237 Added this statement to avoid a space if the 
                  // sponsor name is spaces ***
                  local.Dmdc.Text50 = "SSN#" + import
                    .DmdcProMatchResponse.ChSponsorSsn;
                }

                // *** CQ237 Arun commented the sponsor ssn condition and moved 
                // above ***
              }

              local.Infrastructure.Detail = TrimEnd(local.Dmdc.Text50) + " " + TrimEnd
                (local.Dmdc.Text32);

              break;
            }

            continue;
        }

        local.Infrastructure.CaseNumber = local.ReadCase.Number;
        local.Infrastructure.SituationNumber = 0;
        local.Infrastructure.EventId = 10;
        local.Infrastructure.UserId = "DMDC";
        local.Infrastructure.BusinessObjectCd = "FPL";
        local.Infrastructure.ReferenceDate =
          import.ProgramProcessingInfo.ProcessDate;
        local.Infrastructure.CreatedBy = import.ProgramProcessingInfo.Name;
        local.Infrastructure.DenormNumeric12 =
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault();
        local.Infrastructure.DenormDate = Now().Date;
        local.Infrastructure.ProcessStatus = "Q";
        local.Infrastructure.EventType = "LOC";

        // ***********************************************************************************************
        // NOW ADD THE INFRASTRUCTURE RECORD
        // ***********************************************************************************************
        UseSpCabCreateInfrastructure();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        ++export.HistoryRecordsCreated.Count;
      }

      local.Group.CheckIndex();
    }
    else
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        TrimEnd(local.Infrastructure.CsePersonNumber) + " DMDC LOAD: CSE PERSON NOT FOUND.";
        
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }
    }
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.Function = source.Function;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.EventType = source.EventType;
    target.EventDetailName = source.EventDetailName;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CsenetInOutCode = source.CsenetInOutCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private void UseCabConvertDate2String()
  {
    var useImport = new CabConvertDate2String.Import();
    var useExport = new CabConvertDate2String.Export();

    useImport.DateWorkArea.Date = local.ConvertDateDateWorkArea.Date;

    Call(CabConvertDate2String.Execute, useImport, useExport);

    local.ConvertDateTextWorkArea.Text8 = useExport.TextWorkArea.Text8;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    MoveInfrastructure(useExport.Infrastructure, local.Infrastructure);
  }

  private bool ReadCaseCaseRole1()
  {
    entities.CaseRole.Populated = false;
    entities.Case1.Populated = false;

    return Read("ReadCaseCaseRole1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetString(command, "numb", local.ReadCase.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.CaseRole.CspNumber = db.GetString(reader, 2);
        entities.CaseRole.Type1 = db.GetString(reader, 3);
        entities.CaseRole.Identifier = db.GetInt32(reader, 4);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 6);
        entities.CaseRole.Populated = true;
        entities.Case1.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadCaseCaseRole2()
  {
    entities.CaseRole.Populated = false;
    entities.Case1.Populated = false;

    return Read("ReadCaseCaseRole2",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", local.Infrastructure.CsePersonNumber ?? "");
        db.SetString(command, "numb", local.ReadCase.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.CaseRole.CspNumber = db.GetString(reader, 2);
        entities.CaseRole.Type1 = db.GetString(reader, 3);
        entities.CaseRole.Identifier = db.GetInt32(reader, 4);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 6);
        entities.CaseRole.Populated = true;
        entities.Case1.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadCsePerson1()
  {
    entities.N2dRead.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", local.SecondRead.Number);
      },
      (db, reader) =>
      {
        entities.N2dRead.Number = db.GetString(reader, 0);
        entities.N2dRead.Type1 = db.GetString(reader, 1);
        entities.N2dRead.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.N2dRead.UnemploymentInd = db.GetNullableString(reader, 3);
        entities.N2dRead.FederalInd = db.GetNullableString(reader, 4);
        entities.N2dRead.Populated = true;
        CheckValid<CsePerson>("Type1", entities.N2dRead.Type1);
      });
  }

  private bool ReadCsePerson2()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", local.ChNumber.CsePersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.CsePerson.UnemploymentInd = db.GetNullableString(reader, 3);
        entities.CsePerson.FederalInd = db.GetNullableString(reader, 4);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadInfrastructure1()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure1",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
        db.SetString(
          command, "reasonCode", local.Group.Item.Grp2NdCode.ReasonCode);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 1);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 2);
        entities.Infrastructure.EventId = db.GetInt32(reader, 3);
        entities.Infrastructure.EventType = db.GetString(reader, 4);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 5);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 6);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 7);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 8);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 9);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 10);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 12);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 13);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 14);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 15);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 16);
        entities.Infrastructure.UserId = db.GetString(reader, 17);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 18);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 20);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.Infrastructure.Function = db.GetNullableString(reader, 23);
        entities.Infrastructure.CaseUnitState =
          db.GetNullableString(reader, 24);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 25);
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadInfrastructure10()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure10",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure11()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure11",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure12()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure12",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure13()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure13",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure14()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure14",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure15()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure15",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure16()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure16",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure17()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure17",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure18()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure18",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure19()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure19",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure2()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure2",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
        db.SetString(
          command, "reasonCode1", local.Group.Item.Infrastructure.ReasonCode);
        db.SetString(
          command, "reasonCode2", local.Group.Item.Grp2NdCode.ReasonCode);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 1);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 2);
        entities.Infrastructure.EventId = db.GetInt32(reader, 3);
        entities.Infrastructure.EventType = db.GetString(reader, 4);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 5);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 6);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 7);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 8);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 9);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 10);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 12);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 13);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 14);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 15);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 16);
        entities.Infrastructure.UserId = db.GetString(reader, 17);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 18);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 20);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.Infrastructure.Function = db.GetNullableString(reader, 23);
        entities.Infrastructure.CaseUnitState =
          db.GetNullableString(reader, 24);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 25);
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadInfrastructure20()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure20",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure21()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure21",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure22()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure22",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure23()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure23",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure24()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure24",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure25()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure25",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure26()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure26",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure27()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure27",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure28()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure28",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure29()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure29",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure3()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure3",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
        db.SetNullableString(
          command, "csePersonNum", local.Infrastructure.CsePersonNumber ?? "");
        db.SetInt64(
          command, "chMemberId", import.DmdcProMatchResponse.ChMemberId);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 1);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 2);
        entities.Infrastructure.EventId = db.GetInt32(reader, 3);
        entities.Infrastructure.EventType = db.GetString(reader, 4);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 5);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 6);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 7);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 8);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 9);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 10);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 12);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 13);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 14);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 15);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 16);
        entities.Infrastructure.UserId = db.GetString(reader, 17);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 18);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 20);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.Infrastructure.Function = db.GetNullableString(reader, 23);
        entities.Infrastructure.CaseUnitState =
          db.GetNullableString(reader, 24);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 25);
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadInfrastructure30()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure30",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure31()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure31",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure32()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure32",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure33()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure33",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure34()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure34",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure35()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure35",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
        db.SetNullableString(
          command, "csePersonNum", local.Infrastructure.CsePersonNumber ?? "");
        db.SetInt64(
          command, "chMemberId", import.DmdcProMatchResponse.ChMemberId);
        db.SetString(command, "text20", local.SelOtherEndReason.Text20);
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure36()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure36",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
        db.SetNullableString(
          command, "csePersonNum", local.Infrastructure.CsePersonNumber ?? "");
        db.SetInt64(
          command, "chMemberId", import.DmdcProMatchResponse.ChMemberId);
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure37()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure37",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
        db.SetNullableString(
          command, "csePersonNum", local.Infrastructure.CsePersonNumber ?? "");
        db.SetInt64(
          command, "chMemberId", import.DmdcProMatchResponse.ChMemberId);
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure38()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure38",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
        db.SetNullableString(
          command, "csePersonNum", local.Infrastructure.CsePersonNumber ?? "");
        db.SetInt64(
          command, "chMemberId", import.DmdcProMatchResponse.ChMemberId);
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure39()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure39",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
        db.SetNullableString(
          command, "csePersonNum", local.Infrastructure.CsePersonNumber ?? "");
        db.SetInt64(
          command, "chMemberId", import.DmdcProMatchResponse.ChMemberId);
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure4()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure4",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
        db.SetString(
          command, "reasonCode1", local.Group.Item.Infrastructure.ReasonCode);
        db.SetString(
          command, "reasonCode2", local.Group.Item.Grp2NdCode.ReasonCode);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 1);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 2);
        entities.Infrastructure.EventId = db.GetInt32(reader, 3);
        entities.Infrastructure.EventType = db.GetString(reader, 4);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 5);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 6);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 7);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 8);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 9);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 10);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 12);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 13);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 14);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 15);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 16);
        entities.Infrastructure.UserId = db.GetString(reader, 17);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 18);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 20);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.Infrastructure.Function = db.GetNullableString(reader, 23);
        entities.Infrastructure.CaseUnitState =
          db.GetNullableString(reader, 24);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 25);
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadInfrastructure40()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure40",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
        db.SetNullableString(command, "csePersonNum", local.SecondRead.Number);
        db.SetInt64(
          command, "chMemberId", import.DmdcProMatchResponse.ChMemberId);
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure41()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure41",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
        db.SetInt64(
          command, "chMemberId", import.DmdcProMatchResponse.ChMemberId);
        db.SetNullableString(
          command, "csePersonNum", local.Infrastructure.CsePersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure42()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure42",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
        db.SetInt64(
          command, "chMemberId", import.DmdcProMatchResponse.ChMemberId);
        db.SetNullableString(command, "csePersonNum", local.SecondRead.Number);
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure43()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure43",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
        db.SetInt64(
          command, "chMemberId", import.DmdcProMatchResponse.ChMemberId);
        db.SetNullableString(command, "csePersonNum", local.SecondRead.Number);
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure44()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure44",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
        db.SetInt64(
          command, "chMemberId", import.DmdcProMatchResponse.ChMemberId);
        db.SetNullableString(command, "csePersonNum", local.SecondRead.Number);
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure45()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure45",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
        db.SetInt64(
          command, "chMemberId", import.DmdcProMatchResponse.ChMemberId);
        db.SetNullableString(command, "csePersonNum", local.SecondRead.Number);
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure46()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure46",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
        db.SetString(command, "reasonCode", local.ReadReasonCode1.ReasonCode);
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure47()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure47",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
        db.SetString(command, "reasonCode", local.ReasReasonCode2.ReasonCode);
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure48()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure48",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure49()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure49",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure5()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure5",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
        db.SetString(
          command, "reasonCode", local.Group.Item.Grp2NdCode.ReasonCode);
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 1);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 2);
        entities.Infrastructure.EventId = db.GetInt32(reader, 3);
        entities.Infrastructure.EventType = db.GetString(reader, 4);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 5);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 6);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 7);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 8);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 9);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 10);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 12);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 13);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 14);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 15);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 16);
        entities.Infrastructure.UserId = db.GetString(reader, 17);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 18);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 20);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.Infrastructure.Function = db.GetNullableString(reader, 23);
        entities.Infrastructure.CaseUnitState =
          db.GetNullableString(reader, 24);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 25);
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadInfrastructure50()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure50",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
        db.SetString(command, "reasonCode1", local.ReadReasonCode1.ReasonCode);
        db.SetString(command, "reasonCode2", local.ReasReasonCode2.ReasonCode);
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure51()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure51",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure52()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure52",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure53()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure53",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure54()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure54",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure55()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure55",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure56()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure56",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure57()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure57",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure58()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure58",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
        db.SetString(command, "reasonCode1", local.ReadReasonCode1.ReasonCode);
        db.SetString(command, "reasonCode2", local.ReasReasonCode2.ReasonCode);
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure59()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure59",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure6()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure6",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum", local.Infrastructure.CsePersonNumber ?? "");
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure60()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure60",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure61()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure61",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure62()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure62",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure63()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure63",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure64()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure64",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure65()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure65",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure66()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure66",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure67()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure67",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure68()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure68",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure7()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure7",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure8()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure8",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInfrastructure9()
  {
    entities.SecondPass.Populated = false;

    return Read("ReadInfrastructure9",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Group.Item.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          local.Group.Item.Infrastructure.CsePersonNumber ?? "");
        db.SetNullableString(command, "caseNumber", local.ReadCase.Number);
      },
      (db, reader) =>
      {
        entities.SecondPass.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.SecondPass.SituationNumber = db.GetInt32(reader, 1);
        entities.SecondPass.ProcessStatus = db.GetString(reader, 2);
        entities.SecondPass.EventId = db.GetInt32(reader, 3);
        entities.SecondPass.EventType = db.GetString(reader, 4);
        entities.SecondPass.EventDetailName = db.GetString(reader, 5);
        entities.SecondPass.ReasonCode = db.GetString(reader, 6);
        entities.SecondPass.BusinessObjectCd = db.GetString(reader, 7);
        entities.SecondPass.DenormNumeric12 = db.GetNullableInt64(reader, 8);
        entities.SecondPass.DenormText12 = db.GetNullableString(reader, 9);
        entities.SecondPass.DenormDate = db.GetNullableDate(reader, 10);
        entities.SecondPass.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.SecondPass.InitiatingStateCode = db.GetString(reader, 12);
        entities.SecondPass.CsenetInOutCode = db.GetString(reader, 13);
        entities.SecondPass.CaseNumber = db.GetNullableString(reader, 14);
        entities.SecondPass.CsePersonNumber = db.GetNullableString(reader, 15);
        entities.SecondPass.CaseUnitNumber = db.GetNullableInt32(reader, 16);
        entities.SecondPass.UserId = db.GetString(reader, 17);
        entities.SecondPass.CreatedBy = db.GetString(reader, 18);
        entities.SecondPass.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.SecondPass.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.SecondPass.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.SecondPass.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.SecondPass.Function = db.GetNullableString(reader, 23);
        entities.SecondPass.CaseUnitState = db.GetNullableString(reader, 24);
        entities.SecondPass.Detail = db.GetNullableString(reader, 25);
        entities.SecondPass.Populated = true;
      });
  }

  private bool ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 1);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 2);
        entities.InterstateRequest.Populated = true;
      });
  }

  private bool ReadInvalidSsn()
  {
    entities.InvalidSsn.Populated = false;

    return Read("ReadInvalidSsn",
      (db, command) =>
      {
        db.SetInt32(command, "ssn", local.Convert.SsnNum9);
        db.SetString(
          command, "cspNumber", local.Infrastructure.CsePersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.InvalidSsn.CspNumber = db.GetString(reader, 0);
        entities.InvalidSsn.Ssn = db.GetInt32(reader, 1);
        entities.InvalidSsn.Populated = true;
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
    /// A value of DmdcProMatchResponse.
    /// </summary>
    [JsonPropertyName("dmdcProMatchResponse")]
    public DmdcProMatchResponse DmdcProMatchResponse
    {
      get => dmdcProMatchResponse ??= new();
      set => dmdcProMatchResponse = value;
    }

    /// <summary>
    /// A value of DmdcProcessed.
    /// </summary>
    [JsonPropertyName("dmdcProcessed")]
    public Common DmdcProcessed
    {
      get => dmdcProcessed ??= new();
      set => dmdcProcessed = value;
    }

    /// <summary>
    /// A value of HistoryRecordsCreated.
    /// </summary>
    [JsonPropertyName("historyRecordsCreated")]
    public Common HistoryRecordsCreated
    {
      get => historyRecordsCreated ??= new();
      set => historyRecordsCreated = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    private DmdcProMatchResponse dmdcProMatchResponse;
    private Common dmdcProcessed;
    private Common historyRecordsCreated;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of HistoryRecordsCreated.
    /// </summary>
    [JsonPropertyName("historyRecordsCreated")]
    public Common HistoryRecordsCreated
    {
      get => historyRecordsCreated ??= new();
      set => historyRecordsCreated = value;
    }

    /// <summary>
    /// A value of DmdcProcessed.
    /// </summary>
    [JsonPropertyName("dmdcProcessed")]
    public Common DmdcProcessed
    {
      get => dmdcProcessed ??= new();
      set => dmdcProcessed = value;
    }

    private Common historyRecordsCreated;
    private Common dmdcProcessed;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A OtherCovChildRelListGroup group.</summary>
    [Serializable]
    public class OtherCovChildRelListGroup
    {
      /// <summary>
      /// A value of OtherCovChldRelShort.
      /// </summary>
      [JsonPropertyName("otherCovChldRelShort")]
      public WorkArea OtherCovChldRelShort
      {
        get => otherCovChldRelShort ??= new();
        set => otherCovChldRelShort = value;
      }

      /// <summary>
      /// A value of OtherCovChildRelation.
      /// </summary>
      [JsonPropertyName("otherCovChildRelation")]
      public WorkArea OtherCovChildRelation
      {
        get => otherCovChildRelation ??= new();
        set => otherCovChildRelation = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private WorkArea otherCovChldRelShort;
      private WorkArea otherCovChildRelation;
    }

    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of Grp2NdCode.
      /// </summary>
      [JsonPropertyName("grp2NdCode")]
      public Infrastructure Grp2NdCode
      {
        get => grp2NdCode ??= new();
        set => grp2NdCode = value;
      }

      /// <summary>
      /// A value of DmdcProMatchResponse.
      /// </summary>
      [JsonPropertyName("dmdcProMatchResponse")]
      public DmdcProMatchResponse DmdcProMatchResponse
      {
        get => dmdcProMatchResponse ??= new();
        set => dmdcProMatchResponse = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private Infrastructure grp2NdCode;
      private DmdcProMatchResponse dmdcProMatchResponse;
      private Infrastructure infrastructure;
    }

    /// <summary>
    /// A value of ConvertMessage.
    /// </summary>
    [JsonPropertyName("convertMessage")]
    public SsnWorkArea ConvertMessage
    {
      get => convertMessage ??= new();
      set => convertMessage = value;
    }

    /// <summary>
    /// A value of Message2.
    /// </summary>
    [JsonPropertyName("message2")]
    public WorkArea Message2
    {
      get => message2 ??= new();
      set => message2 = value;
    }

    /// <summary>
    /// A value of Message1.
    /// </summary>
    [JsonPropertyName("message1")]
    public WorkArea Message1
    {
      get => message1 ??= new();
      set => message1 = value;
    }

    /// <summary>
    /// A value of Convert.
    /// </summary>
    [JsonPropertyName("convert")]
    public SsnWorkArea Convert
    {
      get => convert ??= new();
      set => convert = value;
    }

    /// <summary>
    /// A value of ReadReasonCode1.
    /// </summary>
    [JsonPropertyName("readReasonCode1")]
    public Infrastructure ReadReasonCode1
    {
      get => readReasonCode1 ??= new();
      set => readReasonCode1 = value;
    }

    /// <summary>
    /// A value of ReasReasonCode2.
    /// </summary>
    [JsonPropertyName("reasReasonCode2")]
    public Infrastructure ReasReasonCode2
    {
      get => reasReasonCode2 ??= new();
      set => reasReasonCode2 = value;
    }

    /// <summary>
    /// A value of SelOthcovChldRelShrt.
    /// </summary>
    [JsonPropertyName("selOthcovChldRelShrt")]
    public WorkArea SelOthcovChldRelShrt
    {
      get => selOthcovChldRelShrt ??= new();
      set => selOthcovChldRelShrt = value;
    }

    /// <summary>
    /// A value of SelOthcovChildRelation.
    /// </summary>
    [JsonPropertyName("selOthcovChildRelation")]
    public WorkArea SelOthcovChildRelation
    {
      get => selOthcovChildRelation ??= new();
      set => selOthcovChildRelation = value;
    }

    /// <summary>
    /// A value of SponsorChildRelationNum.
    /// </summary>
    [JsonPropertyName("sponsorChildRelationNum")]
    public Common SponsorChildRelationNum
    {
      get => sponsorChildRelationNum ??= new();
      set => sponsorChildRelationNum = value;
    }

    /// <summary>
    /// Gets a value of OtherCovChildRelList.
    /// </summary>
    [JsonIgnore]
    public Array<OtherCovChildRelListGroup> OtherCovChildRelList =>
      otherCovChildRelList ??= new(OtherCovChildRelListGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of OtherCovChildRelList for json serialization.
    /// </summary>
    [JsonPropertyName("otherCovChildRelList")]
    [Computed]
    public IList<OtherCovChildRelListGroup> OtherCovChildRelList_Json
    {
      get => otherCovChildRelList;
      set => OtherCovChildRelList.Assign(value);
    }

    /// <summary>
    /// A value of ChNumber.
    /// </summary>
    [JsonPropertyName("chNumber")]
    public Infrastructure ChNumber
    {
      get => chNumber ??= new();
      set => chNumber = value;
    }

    /// <summary>
    /// A value of PfNumber.
    /// </summary>
    [JsonPropertyName("pfNumber")]
    public Infrastructure PfNumber
    {
      get => pfNumber ??= new();
      set => pfNumber = value;
    }

    /// <summary>
    /// A value of ArNumber.
    /// </summary>
    [JsonPropertyName("arNumber")]
    public Infrastructure ArNumber
    {
      get => arNumber ??= new();
      set => arNumber = value;
    }

    /// <summary>
    /// A value of ApNumber.
    /// </summary>
    [JsonPropertyName("apNumber")]
    public Infrastructure ApNumber
    {
      get => apNumber ??= new();
      set => apNumber = value;
    }

    /// <summary>
    /// A value of PfInactive.
    /// </summary>
    [JsonPropertyName("pfInactive")]
    public Common PfInactive
    {
      get => pfInactive ??= new();
      set => pfInactive = value;
    }

    /// <summary>
    /// A value of ApInactive.
    /// </summary>
    [JsonPropertyName("apInactive")]
    public Common ApInactive
    {
      get => apInactive ??= new();
      set => apInactive = value;
    }

    /// <summary>
    /// A value of ChildInactive.
    /// </summary>
    [JsonPropertyName("childInactive")]
    public Common ChildInactive
    {
      get => childInactive ??= new();
      set => childInactive = value;
    }

    /// <summary>
    /// A value of SecondRead.
    /// </summary>
    [JsonPropertyName("secondRead")]
    public CsePerson SecondRead
    {
      get => secondRead ??= new();
      set => secondRead = value;
    }

    /// <summary>
    /// A value of RecordFound.
    /// </summary>
    [JsonPropertyName("recordFound")]
    public Common RecordFound
    {
      get => recordFound ??= new();
      set => recordFound = value;
    }

    /// <summary>
    /// A value of PersonQualified.
    /// </summary>
    [JsonPropertyName("personQualified")]
    public Common PersonQualified
    {
      get => personQualified ??= new();
      set => personQualified = value;
    }

    /// <summary>
    /// A value of ReadCsePerson.
    /// </summary>
    [JsonPropertyName("readCsePerson")]
    public CsePerson ReadCsePerson
    {
      get => readCsePerson ??= new();
      set => readCsePerson = value;
    }

    /// <summary>
    /// A value of Dmdc.
    /// </summary>
    [JsonPropertyName("dmdc")]
    public WorkArea Dmdc
    {
      get => dmdc ??= new();
      set => dmdc = value;
    }

    /// <summary>
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    /// <summary>
    /// A value of ReadCase.
    /// </summary>
    [JsonPropertyName("readCase")]
    public Case1 ReadCase
    {
      get => readCase ??= new();
      set => readCase = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    /// <summary>
    /// A value of RecordUpdated.
    /// </summary>
    [JsonPropertyName("recordUpdated")]
    public Common RecordUpdated
    {
      get => recordUpdated ??= new();
      set => recordUpdated = value;
    }

    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public DateWorkArea Blank
    {
      get => blank ??= new();
      set => blank = value;
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
    /// A value of PersonUpdated.
    /// </summary>
    [JsonPropertyName("personUpdated")]
    public Common PersonUpdated
    {
      get => personUpdated ??= new();
      set => personUpdated = value;
    }

    /// <summary>
    /// A value of PersonsUpdated.
    /// </summary>
    [JsonPropertyName("personsUpdated")]
    public Common PersonsUpdated
    {
      get => personsUpdated ??= new();
      set => personsUpdated = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    /// <summary>
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
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

    /// <summary>
    /// A value of ConvertDateDateWorkArea.
    /// </summary>
    [JsonPropertyName("convertDateDateWorkArea")]
    public DateWorkArea ConvertDateDateWorkArea
    {
      get => convertDateDateWorkArea ??= new();
      set => convertDateDateWorkArea = value;
    }

    /// <summary>
    /// A value of ConvertDateTextWorkArea.
    /// </summary>
    [JsonPropertyName("convertDateTextWorkArea")]
    public TextWorkArea ConvertDateTextWorkArea
    {
      get => convertDateTextWorkArea ??= new();
      set => convertDateTextWorkArea = value;
    }

    /// <summary>
    /// A value of Clear.
    /// </summary>
    [JsonPropertyName("clear")]
    public Infrastructure Clear
    {
      get => clear ??= new();
      set => clear = value;
    }

    /// <summary>
    /// A value of SelOtherStartReason.
    /// </summary>
    [JsonPropertyName("selOtherStartReason")]
    public WorkArea SelOtherStartReason
    {
      get => selOtherStartReason ??= new();
      set => selOtherStartReason = value;
    }

    /// <summary>
    /// A value of SelOtherEndReason.
    /// </summary>
    [JsonPropertyName("selOtherEndReason")]
    public WorkArea SelOtherEndReason
    {
      get => selOtherEndReason ??= new();
      set => selOtherEndReason = value;
    }

    private SsnWorkArea convertMessage;
    private WorkArea message2;
    private WorkArea message1;
    private SsnWorkArea convert;
    private Infrastructure readReasonCode1;
    private Infrastructure reasReasonCode2;
    private WorkArea selOthcovChldRelShrt;
    private WorkArea selOthcovChildRelation;
    private Common sponsorChildRelationNum;
    private Array<OtherCovChildRelListGroup> otherCovChildRelList;
    private Infrastructure chNumber;
    private Infrastructure pfNumber;
    private Infrastructure arNumber;
    private Infrastructure apNumber;
    private Common pfInactive;
    private Common apInactive;
    private Common childInactive;
    private CsePerson secondRead;
    private Common recordFound;
    private Common personQualified;
    private CsePerson readCsePerson;
    private WorkArea dmdc;
    private WorkArea workArea;
    private Case1 readCase;
    private Array<GroupGroup> group;
    private Common recordUpdated;
    private DateWorkArea blank;
    private DateWorkArea current;
    private Common personUpdated;
    private Common personsUpdated;
    private DateWorkArea max;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private External passArea;
    private Infrastructure infrastructure;
    private DateWorkArea convertDateDateWorkArea;
    private TextWorkArea convertDateTextWorkArea;
    private Infrastructure clear;
    private WorkArea selOtherStartReason;
    private WorkArea selOtherEndReason;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InvalidSsn.
    /// </summary>
    [JsonPropertyName("invalidSsn")]
    public InvalidSsn InvalidSsn
    {
      get => invalidSsn ??= new();
      set => invalidSsn = value;
    }

    /// <summary>
    /// A value of SecondPass.
    /// </summary>
    [JsonPropertyName("secondPass")]
    public Infrastructure SecondPass
    {
      get => secondPass ??= new();
      set => secondPass = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
    }

    /// <summary>
    /// A value of N2dRead.
    /// </summary>
    [JsonPropertyName("n2dRead")]
    public CsePerson N2dRead
    {
      get => n2dRead ??= new();
      set => n2dRead = value;
    }

    private InvalidSsn invalidSsn;
    private Infrastructure secondPass;
    private CaseRole caseRole;
    private Infrastructure infrastructure;
    private CsePerson csePerson;
    private Case1 case1;
    private InterstateRequest interstateRequest;
    private CaseUnit caseUnit;
    private CsePerson n2dRead;
  }
#endregion
}
