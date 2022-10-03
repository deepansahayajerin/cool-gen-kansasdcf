// Program: OE_SVES_ALERT_N_NARR_DTL_GEN, ID: 374518485, model: 746.
// Short name: SWE00109
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
/// A program: OE_SVES_ALERT_N_NARR_DTL_GEN.
/// </para>
/// <para>
/// This action block generates the worker alerts whenever CSE receives SVES 
/// Responses by applying the required business edits.  In addition to the
/// worker alerts, action block will generate the narrative detail with SVES
/// data to the worker, this information will help the worker to generate manual
/// IWOs and send it to the respective SSA district offices.   This Narrative
/// generation will be removed once CSE implements SVES batch processing with
/// auto IWO generation.
/// </para>
/// </summary>
[Serializable]
public partial class OeSvesAlertNNarrDtlGen: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_SVES_ALERT_N_NARR_DTL_GEN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeSvesAlertNNarrDtlGen(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeSvesAlertNNarrDtlGen.
  /// </summary>
  public OeSvesAlertNNarrDtlGen(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // **************************************************************************************
    // *                               
    // Maintenance Log
    // 
    // *
    // **************************************************************************************
    // *    DATE       NAME             PR/SR #     DESCRIPTION OF THE CHANGE
    // *
    // * ----------  -----------------  
    // ---------
    // ----------------------------------------*
    // * 12/01/2009  Raj S              CQ13575     **** Initial Coding  ****
    // *
    // *
    // 
    // As part of FCR tracking service Request *
    // *
    // 
    // this AB will generate alerts and        *
    // *
    // 
    // Narrative Detail record for responses   *
    // *
    // 
    // received from SVES.                     *
    // *
    // 
    // *
    // * 03/14/2011  Raj S              CQ24511     Modified to adde code to 
    // generate worker*
    // *
    // 
    // alerts for Title-II & Title-XVI         *
    // *
    // 
    // *
    // *
    // 
    // *
    // **************************************************************************************
    // **************************************************************************************
    // This action block will be used by SVES Response batch procedure in order 
    // to generate
    // worker alerts and Narrative Details for manual IWO Generation.
    // **************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.Max.Date = import.Max.Date;
    local.Infrastructure.Assign(import.Infrastructure);
    export.DuplicateAlertCount.Count = import.DuplicateAlertCount.Count;
    export.TotalHistoryCount.Count = import.TotalHistoryCount.Count;
    export.TotalAlertCount.Count = import.TotalAlertCount.Count;
    export.TotalT2SkippedCr.Count = import.TotalT2SkippedCr.Count;
    export.TotT2PenForAlrtNHist.Count = import.TotT2PenForAlrtNHist.Count;
    local.T2ecordProcessedFlag.Flag = "";

    // **************************************************************************************
    // Read all case & case role for the selected person and for each case 
    // selected establish
    // infrastructure record(Alert & History) record.  While selecting the case 
    // role record
    // only active records needs to be selected.
    // **************************************************************************************
    foreach(var item in ReadCaseRoleCase())
    {
      local.Infrastructure.CaseNumber = entities.Case1.Number;

      // **************************************************************************************
      // Assign the respective reason codes based on the role played by the 
      // person for the
      // selected case.
      // Person Role Type           		History     Alert   Narrative Detail
      // AP
      // 
      // X          X              X
      // AR
      // 
      // X
      // CH (Auxiliary/Survivor Benefits)	   X	      X              X
      // **************************************************************************************
      local.Infrastructure.ReasonCode = "";
      local.Infrastructure.ProcessStatus = "Q";

      if (Equal(import.SvesTitleIiPendClaim.LocSrcResponseAgencyCode, "E04"))
      {
        // **************************************************************************************
        // *
        // 
        // *
        // * If the SVES response is for Titl-II pending claim then the process 
        // will follow this*
        // * path to set Infrastructure record 
        // values.
        // 
        // *
        // *
        // 
        // *
        // **************************************************************************************
        if (Equal(entities.CaseRole.Type1, "AP"))
        {
          local.Infrastructure.ReasonCode = "FCRSVESPENDAP";
          local.Infrastructure.Detail = "SSN:" + TrimEnd
            (import.SvesTitleIiPendClaim.Ssn) + " Claim Type: " + import
            .SvesTitleIiPendClaim.ClaimTypeCode + ", PF16 to view SSA Info (expires in 1 Yr)";
            
        }
        else if (Equal(entities.CaseRole.Type1, "AR"))
        {
          local.Infrastructure.ReasonCode = "FCRSVESPENDAR";
          local.Infrastructure.Detail = "SSN:" + TrimEnd
            (import.SvesTitleIiPendClaim.Ssn) + " Claim Type: " + import
            .SvesTitleIiPendClaim.ClaimTypeCode + ",  provided for your information only";
            
          local.Infrastructure.ProcessStatus = "H";
        }
        else if (Equal(entities.CaseRole.Type1, "CH"))
        {
          if (Equal(import.SvesTitleIiPendClaim.ClaimTypeCode, "AU") || Equal
            (import.SvesTitleIiPendClaim.ClaimTypeCode, "SU"))
          {
            local.Infrastructure.ReasonCode = "FCRSVESPENDCH";
            local.Infrastructure.Detail = "SSN:" + TrimEnd
              (import.SvesTitleIiPendClaim.Ssn) + " Claim Type: " + import
              .SvesTitleIiPendClaim.ClaimTypeCode + ", PF16 to view SSA Info (expires in 1 Yr)";
              
          }
        }
      }
      else if (Equal(import.SvesTitleIiPendClaim.LocSrcResponseAgencyCode, "E05"))
        
      {
        // **************************************************************************************
        // *
        // 
        // *
        // * If the SVES response is for Titl-II (E05) then the process will 
        // follow this path   *
        // * to set Infrastructure record values.
        // 
        // *
        // *
        // 
        // *
        // **************************************************************************************
        local.Infrastructure.Detail = "" + "Refer to SAR report #SRM03085-R01 dated:  " +
          NumberToString(DateToInt(import.ProcessingDate.Date), 8, 8) + " (expires in 3 Months)";
          

        if (Equal(entities.CaseRole.Type1, "AP"))
        {
          local.Infrastructure.ReasonCode = "FCRSVEST2AP";
        }
        else if (Equal(entities.CaseRole.Type1, "AR"))
        {
          local.Infrastructure.ReasonCode = "FCRSVEST2AR";
        }
        else if (Equal(entities.CaseRole.Type1, "CH"))
        {
          local.Infrastructure.ReasonCode = "FCRSVEST2CH";
        }
      }
      else if (Equal(import.SvesTitleIiPendClaim.LocSrcResponseAgencyCode, "E06"))
        
      {
        // **************************************************************************************
        // *
        // 
        // *
        // * If the SVES response is for Titl-XVI(E06) then the process will 
        // follow this path   *
        // * to set Infrastructure record values.
        // 
        // *
        // *
        // 
        // *
        // **************************************************************************************
        local.Infrastructure.Detail = "" + "Refer to SAR report #SRM03085-R02 dated:  " +
          NumberToString(DateToInt(import.ProcessingDate.Date), 8, 8) + " (expires in 3 Months)";
          

        if (Equal(entities.CaseRole.Type1, "AP"))
        {
          local.Infrastructure.ReasonCode = "FCRSVEST16AP";
        }
        else if (Equal(entities.CaseRole.Type1, "AR"))
        {
          local.Infrastructure.ReasonCode = "FCRSVEST16AR";
        }
        else if (Equal(entities.CaseRole.Type1, "CH"))
        {
          local.Infrastructure.ReasonCode = "FCRSVEST16CH";
        }
      }
      else
      {
        // **************************************************************************************
        // *
        // 
        // *
        // * The SVES Response record currently read id for other than Title-II 
        // Pending Claim,  *
        // * Title-II or Title-XVI. Control will be back to calling object 
        // without processing.  *
        // *
        // 
        // *
        // **************************************************************************************
        ++export.TotalT2SkippedCr.Count;

        return;
      }

      if (IsEmpty(local.Infrastructure.ReasonCode))
      {
        continue;
      }

      // **************************************************************************************
      // The following check ensures that process is not creating a duplicate 
      // alert/narrative
      // details for the same person and same claim type.  FCR/SVES can send the
      // same info.
      // again when there is a change to the case, in those cases the following 
      // check will not
      // generate any alerts/narrative details.
      // **************************************************************************************
      if (ReadInfrastructure())
      {
        ++export.DuplicateAlertCount.Count;

        continue;
      }

      // **************************************************************************************
      // No infrastructure records available for the selected Case, Person, 
      // Reason code & detail,
      // the process can generate the alert & Narrative detail to the worker.
      // **************************************************************************************
      UseSpCabCreateInfrastructure();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      local.T2ecordProcessedFlag.Flag = "Y";

      // **************************************************************************************
      // If the infrastruture record is History record then do not generate 
      // narrative detail
      // and skip to next case role records
      // **************************************************************************************
      if (AsChar(local.Infrastructure.ProcessStatus) == 'H')
      {
        ++export.TotalHistoryCount.Count;

        continue;
      }

      // **************************************************************************************
      // *  Narrative Detail will be populated only for Title-II Pending Claim(
      // E04) Records   *
      // **************************************************************************************
      if (Equal(import.SvesTitleIiPendClaim.LocSrcResponseAgencyCode, "E04"))
      {
        // **************************************************************************************
        // Get the user id from Service Provider entity type for the selected 
        // via case assignment
        // and Officer Service Provider Entity Types.   This user id required to
        // update in
        // Narrative detail entity type to display on CSLN screen.
        // **************************************************************************************
        local.NarrativeDetail.CreatedBy = global.UserId;

        if (ReadCaseAssignmentServiceProvider())
        {
          local.NarrativeDetail.CreatedBy = entities.ServiceProvider.UserId;
        }

        // **************************************************************************************
        // For each Infrastructure generated by the process, Narrative Detail 
        // information needs
        // to be created with multiple line containing below mentioned 
        // information:
        // Line1:  CSE Person# & FCR SSN# received through SVES file
        // Line2:  Person Name: First, Middle & Last Name.
        // Line3:  Additional Name1: First, Middle & Last Name.
        // Line4:  Additional Name2: First, Middle & Last Name.
        // Line5:  Returned Name: First, Middle & Last Name.
        // Line6:  Claim Type, Corrected/Addtional/Multiple SSN, SSN Match Code
        // Line7:  District Office Address Line 1&2
        // Line8:  District Office Address Line 3&4
        // Line9:  District Office Address City, Zip & State.
        // Line10: **Title II Pending Claim from SVES thru SWEIB445 Batch 
        // Process**
        // **************************************************************************************
        local.NarrativeDetail.CaseNumber = entities.Case1.Number;
        local.NarrativeDetail.CreatedTimestamp =
          local.Infrastructure.CreatedTimestamp;
        local.NarrativeDetail.InfrastructureId =
          local.Infrastructure.SystemGeneratedIdentifier;
        local.NarrativeDetailLineCnt.Count = 0;
        local.NarrativeDetail.LineNumber = 0;

        do
        {
          ++local.NarrativeDetailLineCnt.Count;

          if (local.NarrativeDetailLineCnt.Count > 11)
          {
            break;
          }

          local.NarrativeDetail.NarrativeText = "";

          switch(local.NarrativeDetailLineCnt.Count)
          {
            case 1:
              ++local.NarrativeDetail.LineNumber;
              local.NarrativeDetail.NarrativeText = "CSE PERSON#:" + import
                .SvesTitleIiPendClaim.MemberIdentifier + "       FCR SSN#:" + import
                .SvesTitleIiPendClaim.Ssn;

              break;
            case 2:
              ++local.NarrativeDetail.LineNumber;
              local.Text.Text60 =
                TrimEnd(import.SvesTitleIiPendClaim.FirstName) + " " + TrimEnd
                (import.SvesTitleIiPendClaim.MiddleName) + " " + TrimEnd
                (import.SvesTitleIiPendClaim.LastName);
              local.NarrativeDetail.NarrativeText = "Person Name: " + TrimEnd
                (local.Text.Text60);

              break;
            case 3:
              if (IsEmpty(import.SvesTitleIiPendClaim.AdditionalFirstName1) && IsEmpty
                (import.SvesTitleIiPendClaim.AdditionalLastName1) && IsEmpty
                (import.SvesTitleIiPendClaim.AdditionalMiddleName1))
              {
                continue;
              }

              ++local.NarrativeDetail.LineNumber;
              local.Text.Text60 =
                TrimEnd(import.SvesTitleIiPendClaim.AdditionalFirstName1) + " " +
                TrimEnd(import.SvesTitleIiPendClaim.AdditionalMiddleName1) + " " +
                TrimEnd(import.SvesTitleIiPendClaim.AdditionalLastName1);
              local.NarrativeDetail.NarrativeText = "Addl Name 1: " + TrimEnd
                (local.Text.Text60);

              break;
            case 4:
              if (IsEmpty(import.SvesTitleIiPendClaim.AdditionalFirstName2) && IsEmpty
                (import.SvesTitleIiPendClaim.AdditionalLastName2) && IsEmpty
                (import.SvesTitleIiPendClaim.AdditionalMiddleName2))
              {
                continue;
              }

              ++local.NarrativeDetail.LineNumber;
              local.Text.Text60 =
                TrimEnd(import.SvesTitleIiPendClaim.AdditionalFirstName2) + " " +
                TrimEnd(import.SvesTitleIiPendClaim.AdditionalMiddleName2) + " " +
                TrimEnd(import.SvesTitleIiPendClaim.AdditionalLastName2);
              local.NarrativeDetail.NarrativeText = "Addl Name 2: " + TrimEnd
                (local.Text.Text60);

              break;
            case 5:
              if (IsEmpty(import.SvesTitleIiPendClaim.ReturnedFirstName) && IsEmpty
                (import.SvesTitleIiPendClaim.ReturnedLastName) && IsEmpty
                (import.SvesTitleIiPendClaim.ReturnedMiddleName))
              {
                continue;
              }

              ++local.NarrativeDetail.LineNumber;
              local.Text.Text60 =
                TrimEnd(import.SvesTitleIiPendClaim.ReturnedFirstName) + " " + TrimEnd
                (import.SvesTitleIiPendClaim.ReturnedMiddleName) + " " + TrimEnd
                (import.SvesTitleIiPendClaim.ReturnedLastName);
              local.NarrativeDetail.NarrativeText = "Returned Name: " + TrimEnd
                (local.Text.Text60);

              break;
            case 6:
              ++local.NarrativeDetail.LineNumber;
              local.NarrativeDetail.NarrativeText = "Claim Type: " + "";

              if (Equal(import.SvesTitleIiPendClaim.ClaimTypeCode, "AU"))
              {
                local.NarrativeDetail.NarrativeText = "Claim Type: " + "Auxiliary             ";
                  
              }

              if (Equal(import.SvesTitleIiPendClaim.ClaimTypeCode, "DI"))
              {
                local.NarrativeDetail.NarrativeText = "Claim Type: " + "Disability            ";
                  
              }

              if (Equal(import.SvesTitleIiPendClaim.ClaimTypeCode, "RI"))
              {
                local.NarrativeDetail.NarrativeText = "Claim Type: " + "Retirement            ";
                  
              }

              if (Equal(import.SvesTitleIiPendClaim.ClaimTypeCode, "SU"))
              {
                local.NarrativeDetail.NarrativeText = "Claim Type: " + "Survivor Benefits     ";
                  
              }

              local.NarrativeDetail.NarrativeText =
                TrimEnd(local.NarrativeDetail.NarrativeText) + "     " + "Corr/Addl/Multi SSN: ";
                
              local.NarrativeDetail.NarrativeText =
                TrimEnd(local.NarrativeDetail.NarrativeText) + import
                .SvesTitleIiPendClaim.CorrAdditlMultipleSsn;

              break;
            case 7:
              ++local.NarrativeDetail.LineNumber;

              if (IsEmpty(import.SvesTitleIiPendClaim.DoMailingAddressLine1) &&
                IsEmpty(import.SvesTitleIiPendClaim.DoMailingAddressLine2) && IsEmpty
                (import.SvesTitleIiPendClaim.DoMailingAddressLine3) && IsEmpty
                (import.SvesTitleIiPendClaim.DoMailingAddressLine4) && IsEmpty
                (import.SvesTitleIiPendClaim.DoMailingAddressState) && IsEmpty
                (import.SvesTitleIiPendClaim.DoMainlingAddressZip))
              {
                local.NarrativeDetail.NarrativeText =
                  "SSA Dist Office Addr: " + TrimEnd
                  ("***** NO SSA ADDRESS INFORMATION *****") + TrimEnd
                  (" " + "");
              }
              else
              {
                local.NarrativeDetail.NarrativeText =
                  "SSA Dist Office Addr: " + TrimEnd
                  (import.SvesTitleIiPendClaim.DoMailingAddressLine1) + TrimEnd
                  (" " + "");
              }

              break;
            case 8:
              if (IsEmpty(import.SvesTitleIiPendClaim.DoMailingAddressLine2))
              {
                continue;
              }

              ++local.NarrativeDetail.LineNumber;
              local.NarrativeDetail.NarrativeText = "                      " + TrimEnd
                (import.SvesTitleIiPendClaim.DoMailingAddressLine2) + TrimEnd
                (" " + "");

              break;
            case 9:
              if (IsEmpty(import.SvesTitleIiPendClaim.DoMailingAddressLine3) &&
                IsEmpty(import.SvesTitleIiPendClaim.DoMailingAddressLine4))
              {
                continue;
              }

              ++local.NarrativeDetail.LineNumber;
              local.NarrativeDetail.NarrativeText = "                      " + TrimEnd
                (import.SvesTitleIiPendClaim.DoMailingAddressLine3) + TrimEnd
                (" " + import.SvesTitleIiPendClaim.DoMailingAddressLine4);

              break;
            case 10:
              if (IsEmpty(import.SvesTitleIiPendClaim.DoMailingAddressState) &&
                IsEmpty(import.SvesTitleIiPendClaim.DoMainlingAddressZip))
              {
                continue;
              }

              ++local.NarrativeDetail.LineNumber;
              local.NarrativeDetail.NarrativeText = "                      " + TrimEnd
                (import.SvesTitleIiPendClaim.DoMailingAddressCity) + " " + TrimEnd
                (import.SvesTitleIiPendClaim.DoMailingAddressState) + " " + Substring
                (import.SvesTitleIiPendClaim.DoMainlingAddressZip,
                FcrSvesTitleIiPendingClaim.DoMainlingAddressZip_MaxLength, 1,
                5) + " " + Substring
                (import.SvesTitleIiPendClaim.DoMainlingAddressZip,
                FcrSvesTitleIiPendingClaim.DoMainlingAddressZip_MaxLength, 6,
                4);

              break;
            case 11:
              ++local.NarrativeDetail.LineNumber;
              local.NarrativeDetail.NarrativeText =
                import.EmployerSourceTxt.NarrativeText ?? "";

              break;
            default:
              goto AfterCycle;
          }

          // **************************************************************************************
          // The following Common Action Block(CAB) will be used to store the 
          // narrative detail
          // information.
          // **************************************************************************************
          UseSpCabCreateNarrativeDetail();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }
        while(local.NarrativeDetailLineCnt.Count <= 11);

AfterCycle:
        ;
      }

      ++export.TotalAlertCount.Count;
    }

    if (AsChar(local.T2ecordProcessedFlag.Flag) == 'Y')
    {
      ++export.TotT2PenForAlrtNHist.Count;
    }
    else
    {
      ++export.TotalT2SkippedCr.Count;
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

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    MoveInfrastructure(useExport.Infrastructure, local.Infrastructure);
  }

  private void UseSpCabCreateNarrativeDetail()
  {
    var useImport = new SpCabCreateNarrativeDetail.Import();
    var useExport = new SpCabCreateNarrativeDetail.Export();

    useImport.NarrativeDetail.Assign(local.NarrativeDetail);

    Call(SpCabCreateNarrativeDetail.Execute, useImport, useExport);
  }

  private bool ReadCaseAssignmentServiceProvider()
  {
    entities.CaseAssignment.Populated = false;
    entities.ServiceProvider.Populated = false;

    return Read("ReadCaseAssignmentServiceProvider",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetDate(command, "effectiveDate", date);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 0);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 1);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 3);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 4);
        entities.CaseAssignment.OspCode = db.GetString(reader, 5);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 6);
        entities.CaseAssignment.CasNo = db.GetString(reader, 7);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 8);
        entities.ServiceProvider.UserId = db.GetString(reader, 9);
        entities.CaseAssignment.Populated = true;
        entities.ServiceProvider.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseRoleCase()
  {
    entities.Case1.Populated = false;
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRoleCase",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetString(
          command, "cspNumber", import.SvesTitleIiPendClaim.MemberIdentifier);
        db.SetNullableDate(command, "startDate", date);
        db.SetNullableDate(
          command, "endDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.Case1.Status = db.GetNullableString(reader, 6);
        entities.Case1.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
      });
  }

  private bool ReadInfrastructure()
  {
    entities.Existing.Populated = false;

    return Read("ReadInfrastructure",
      (db, command) =>
      {
        db.SetNullableString(
          command, "caseNumber", local.Infrastructure.CaseNumber ?? "");
        db.SetNullableString(
          command, "csePersonNum", local.Infrastructure.CsePersonNumber ?? "");
        db.SetString(command, "reasonCode", local.Infrastructure.ReasonCode);
        db.SetNullableString(
          command, "detail", local.Infrastructure.Detail ?? "");
      },
      (db, reader) =>
      {
        entities.Existing.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Existing.ReasonCode = db.GetString(reader, 1);
        entities.Existing.CaseNumber = db.GetNullableString(reader, 2);
        entities.Existing.CsePersonNumber = db.GetNullableString(reader, 3);
        entities.Existing.Detail = db.GetNullableString(reader, 4);
        entities.Existing.Populated = true;
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
    /// A value of TotT2PenForAlrtNHist.
    /// </summary>
    [JsonPropertyName("totT2PenForAlrtNHist")]
    public Common TotT2PenForAlrtNHist
    {
      get => totT2PenForAlrtNHist ??= new();
      set => totT2PenForAlrtNHist = value;
    }

    /// <summary>
    /// A value of TotalT2SkippedCr.
    /// </summary>
    [JsonPropertyName("totalT2SkippedCr")]
    public Common TotalT2SkippedCr
    {
      get => totalT2SkippedCr ??= new();
      set => totalT2SkippedCr = value;
    }

    /// <summary>
    /// A value of DuplicateAlertCount.
    /// </summary>
    [JsonPropertyName("duplicateAlertCount")]
    public Common DuplicateAlertCount
    {
      get => duplicateAlertCount ??= new();
      set => duplicateAlertCount = value;
    }

    /// <summary>
    /// A value of TotalHistoryCount.
    /// </summary>
    [JsonPropertyName("totalHistoryCount")]
    public Common TotalHistoryCount
    {
      get => totalHistoryCount ??= new();
      set => totalHistoryCount = value;
    }

    /// <summary>
    /// A value of TotalAlertCount.
    /// </summary>
    [JsonPropertyName("totalAlertCount")]
    public Common TotalAlertCount
    {
      get => totalAlertCount ??= new();
      set => totalAlertCount = value;
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
    /// A value of SvesTitleIiPendClaim.
    /// </summary>
    [JsonPropertyName("svesTitleIiPendClaim")]
    public FcrSvesTitleIiPendingClaim SvesTitleIiPendClaim
    {
      get => svesTitleIiPendClaim ??= new();
      set => svesTitleIiPendClaim = value;
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
    /// A value of EmployerSourceTxt.
    /// </summary>
    [JsonPropertyName("employerSourceTxt")]
    public NarrativeDetail EmployerSourceTxt
    {
      get => employerSourceTxt ??= new();
      set => employerSourceTxt = value;
    }

    /// <summary>
    /// A value of ProcessingDate.
    /// </summary>
    [JsonPropertyName("processingDate")]
    public DateWorkArea ProcessingDate
    {
      get => processingDate ??= new();
      set => processingDate = value;
    }

    private Common totT2PenForAlrtNHist;
    private Common totalT2SkippedCr;
    private Common duplicateAlertCount;
    private Common totalHistoryCount;
    private Common totalAlertCount;
    private DateWorkArea max;
    private FcrSvesTitleIiPendingClaim svesTitleIiPendClaim;
    private Infrastructure infrastructure;
    private NarrativeDetail employerSourceTxt;
    private DateWorkArea processingDate;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of TotT2PenForAlrtNHist.
    /// </summary>
    [JsonPropertyName("totT2PenForAlrtNHist")]
    public Common TotT2PenForAlrtNHist
    {
      get => totT2PenForAlrtNHist ??= new();
      set => totT2PenForAlrtNHist = value;
    }

    /// <summary>
    /// A value of TotalT2SkippedCr.
    /// </summary>
    [JsonPropertyName("totalT2SkippedCr")]
    public Common TotalT2SkippedCr
    {
      get => totalT2SkippedCr ??= new();
      set => totalT2SkippedCr = value;
    }

    /// <summary>
    /// A value of DuplicateAlertCount.
    /// </summary>
    [JsonPropertyName("duplicateAlertCount")]
    public Common DuplicateAlertCount
    {
      get => duplicateAlertCount ??= new();
      set => duplicateAlertCount = value;
    }

    /// <summary>
    /// A value of TotalHistoryCount.
    /// </summary>
    [JsonPropertyName("totalHistoryCount")]
    public Common TotalHistoryCount
    {
      get => totalHistoryCount ??= new();
      set => totalHistoryCount = value;
    }

    /// <summary>
    /// A value of TotalAlertCount.
    /// </summary>
    [JsonPropertyName("totalAlertCount")]
    public Common TotalAlertCount
    {
      get => totalAlertCount ??= new();
      set => totalAlertCount = value;
    }

    private Common totT2PenForAlrtNHist;
    private Common totalT2SkippedCr;
    private Common duplicateAlertCount;
    private Common totalHistoryCount;
    private Common totalAlertCount;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of T2ecordProcessedFlag.
    /// </summary>
    [JsonPropertyName("t2ecordProcessedFlag")]
    public Common T2ecordProcessedFlag
    {
      get => t2ecordProcessedFlag ??= new();
      set => t2ecordProcessedFlag = value;
    }

    /// <summary>
    /// A value of PersonPlaysArRoleFlag.
    /// </summary>
    [JsonPropertyName("personPlaysArRoleFlag")]
    public Common PersonPlaysArRoleFlag
    {
      get => personPlaysArRoleFlag ??= new();
      set => personPlaysArRoleFlag = value;
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
    /// A value of Text.
    /// </summary>
    [JsonPropertyName("text")]
    public WorkArea Text
    {
      get => text ??= new();
      set => text = value;
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
    /// A value of NarrativeDetailLineCnt.
    /// </summary>
    [JsonPropertyName("narrativeDetailLineCnt")]
    public Common NarrativeDetailLineCnt
    {
      get => narrativeDetailLineCnt ??= new();
      set => narrativeDetailLineCnt = value;
    }

    /// <summary>
    /// A value of NarrativeDetail.
    /// </summary>
    [JsonPropertyName("narrativeDetail")]
    public NarrativeDetail NarrativeDetail
    {
      get => narrativeDetail ??= new();
      set => narrativeDetail = value;
    }

    private Common t2ecordProcessedFlag;
    private Common personPlaysArRoleFlag;
    private DateWorkArea max;
    private WorkArea text;
    private Infrastructure infrastructure;
    private Common narrativeDetailLineCnt;
    private NarrativeDetail narrativeDetail;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public Infrastructure Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    private Case1 case1;
    private CaseAssignment caseAssignment;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private Infrastructure existing;
  }
#endregion
}
