// Program: OE_B411_EXTRACT_FCR_REQUESTS, ID: 373528621, model: 746.
// Short name: SWEE411B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B411_EXTRACT_FCR_REQUESTS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB411ExtractFcrRequests: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B411_EXTRACT_FCR_REQUESTS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB411ExtractFcrRequests(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB411ExtractFcrRequests.
  /// </summary>
  public OeB411ExtractFcrRequests(IContext context, Import import, Export export)
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
    // *******************************************************************************
    // Date	       Developer	      Description
    // 02/16/2000     Sree Veettil	   Initial Creation.
    // 08/22/2000     Ed Lyman            Performance Improvements.
    // 03/30/2001     Ed Lyman            PR11584 Add Alias.
    // 03/30/2001     Ed Lyman            PR116221 Delay sending newborns.
    // 05/07/2001     Ed Lyman            PR116735 Tighten edits of Place
    //                                    
    // of Birth, mother's maiden name
    //                                    
    // and father's name.
    // 12/15/2001     Ed Lyman            PR133257  Send foreign place of birth.
    // 12/15/2001     Ed Lyman            PR131235  Don't request 1099 locate 
    // source.
    // 11/09/2005     DDupree            WR00258947 FCR minor release , can now 
    // do
    // locate request on children on a IV-D case. Also added A03 to the locate 
    // request.
    // *******************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    UseOeB411Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // *****************************************************************
    // Main Read which reads all the cases from the KESSEP daabase.
    // *****************************************************************
    foreach(var item in ReadCase())
    {
      local.ExternalFplsRequest.Assign(local.Init1);
      local.WriteCase.Flag = "N";

      // *****************************************************************
      // Check if there is an AP or AR for the case, if there is no AP or AR
      // for the case dont write this record into the file. If the case has
      // only one AR with no AP and the AR is organization dont write the
      // record. Look for an AR or AP for this case who is a client.
      // *****************************************************************
      foreach(var item1 in ReadCaseRoleCsePerson())
      {
        local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
        UseSiReadCsePersonBatch();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (IsExitState("ADABAS_READ_UNSUCCESSFUL"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = "Person not found on ADABAS: " + entities
              .CsePerson.Number;
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ALL_OK";

            continue;
          }
          else
          {
            goto ReadEach;
          }
        }

        // *****************************************************************
        // Eliminate the special characters from the first name, last name.
        // *****************************************************************
        UseCabFcrFormatNames1();

        // *****************************************************************
        // Skip the person unless they have two of the three following items
        // present: SSN, DOB or first and last name.
        // *****************************************************************
        if (!Equal(local.CsePersonsWorkSet.Dob, local.Null1.Date) && !
          Lt(local.CsePersonsWorkSet.Ssn, "000000001"))
        {
          local.WriteCase.Flag = "Y";

          break;
        }

        if (!Equal(local.CsePersonsWorkSet.Dob, local.Null1.Date) && !
          IsEmpty(local.CsePersonsWorkSet.FirstName) && !
          IsEmpty(local.CsePersonsWorkSet.LastName))
        {
          local.WriteCase.Flag = "Y";

          break;
        }

        if (!Lt(local.CsePersonsWorkSet.Ssn, "000000001") && !
          IsEmpty(local.CsePersonsWorkSet.FirstName) && !
          IsEmpty(local.CsePersonsWorkSet.LastName))
        {
          local.WriteCase.Flag = "Y";

          break;
        }
      }

      if (AsChar(local.WriteCase.Flag) != 'Y')
      {
        continue;
      }

      ++local.CaseCount.Count;
      ++local.TotalCount.Count;
      local.EabFileHandling.Action = "WRITE";
      local.RecordIdentifier.Text3 = "FC";
      local.ActionTypeCode.Text1 = "A";
      local.CaseId.Text10 = entities.Case1.Number;
      local.CaseType.Text1 = "F";

      // *****************************************************************
      // Check the existence of a child support order that is applicable to this
      // case
      // *****************************************************************
      local.OrderIndicator.Text1 = "N";

      foreach(var item1 in ReadCaseRole2())
      {
        if (ReadLegalActionCaseRole())
        {
          local.OrderIndicator.Text1 = "Y";

          break;
        }
      }

      // *****************************************************************
      // Write the Case record to the extract file.
      // *****************************************************************
      UseOeEabWriteFcrRequests2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        break;
      }

      foreach(var item1 in ReadCaseRole3())
      {
        local.ExternalFplsRequest.Assign(local.Init1);
        local.AdditionalSsn1.Ssn = "";
        local.AdditionalSsn2.Ssn = "";
        local.AdditionalFirstName1.Text16 = "";
        local.AdditionalFirstName2.Text16 = "";
        local.AdditionalFirstName3.Text16 = "";
        local.AdditionalFirstName4.Text16 = "";
        local.AdditionalMiddleName1.Text16 = "";
        local.AdditionalMiddleName2.Text16 = "";
        local.AdditionalMiddleName3.Text16 = "";
        local.AdditionalMiddleName4.Text16 = "";
        local.AdditionalLastName1.Text32 = "";
        local.AdditionalLastName2.Text32 = "";
        local.AdditionalLastName3.Text32 = "";
        local.AdditionalLastName4.Text32 = "";
        local.WritePerson.Flag = "N";
        local.IsDob.Flag = "N";
        local.IsName.Flag = "N";
        local.IsSsn.Flag = "N";

        if (ReadCsePerson())
        {
          local.CsePersonsWorkSet.Number = entities.CsePerson.Number;

          // *****************************************************************
          // Get the name, sex, DOB informations from ADABASE
          // *****************************************************************
          UseSiReadCsePersonBatch();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (IsExitState("ADABAS_READ_UNSUCCESSFUL"))
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail = "Person not found on ADABAS: " + entities
                .CsePerson.Number;
              UseCabErrorReport();
              ExitState = "ACO_NN0000_ALL_OK";

              continue;
            }
            else
            {
              goto ReadEach;
            }
          }

          // *****************************************************************
          // Eliminate the special characters from the first name, last name.
          // *****************************************************************
          UseCabFcrFormatNames1();

          // *****************************************************************
          // Skip the record if it doesnt have 2 of the three below.
          // Full Name
          // DOB
          // SSN
          // *****************************************************************
          if (!Equal(entities.CaseRole.Type1, "CH"))
          {
            if (!IsEmpty(local.CsePersonsWorkSet.FirstName) && !
              IsEmpty(local.CsePersonsWorkSet.LastName))
            {
              local.IsName.Flag = "Y";
            }

            if (!Lt(local.CsePersonsWorkSet.Ssn, "000000001"))
            {
              local.IsSsn.Flag = "Y";
            }

            if (!Equal(local.CsePersonsWorkSet.Dob, local.Null1.Date))
            {
              local.IsDob.Flag = "Y";
            }

            if (AsChar(local.IsName.Flag) == 'Y' && AsChar
              (local.IsSsn.Flag) == 'Y' || AsChar(local.IsName.Flag) == 'Y' && AsChar
              (local.IsDob.Flag) == 'Y' || AsChar(local.IsSsn.Flag) == 'Y' && AsChar
              (local.IsDob.Flag) == 'Y')
            {
              local.WritePerson.Flag = "Y";
            }
          }
          else
          {
            if (!Lt(local.CsePersonsWorkSet.Ssn, "000000001"))
            {
              local.WritePerson.Flag = "Y";
            }

            if (!Equal(local.CsePersonsWorkSet.Dob, local.Null1.Date))
            {
              if (Lt(local.CsePersonsWorkSet.Dob,
                AddMonths(local.ProgramProcessingInfo.ProcessDate, -6)))
              {
                local.WritePerson.Flag = "Y";
              }
            }
          }

          if (AsChar(local.WritePerson.Flag) != 'Y')
          {
            continue;
          }

          ++local.PersonsCount.Count;
          ++local.TotalCount.Count;
          local.EabFileHandling.Action = "WRITE";
          local.RecordIdentifier.Text3 = "FP";
          local.ExternalFplsRequest.LocalCode = "";
          local.ActionTypeCode.Text1 = "A";
          local.CaseType.Text1 = "F";
          local.ExternalFplsRequest.UsersField = "";
          local.ExternalFplsRequest.StationNumber = "";
          local.ExternalFplsRequest.CollectAllResponsesTogether = "N";

          switch(TrimEnd(entities.CaseRole.Type1))
          {
            case "CH":
              // *****************************************************************
              // Sex is mandatory field. Default to "M" if CH.
              // *****************************************************************
              if (IsEmpty(local.CsePersonsWorkSet.Sex))
              {
                local.CsePersonsWorkSet.Sex = "M";
              }

              local.ParticipantType.Text3 = "CH";

              break;
            case "AP":
              // *****************************************************************
              // Sex is mandatory field. Default to "M" if AP.
              // *****************************************************************
              if (IsEmpty(local.CsePersonsWorkSet.Sex))
              {
                local.CsePersonsWorkSet.Sex = "M";
              }

              if (AsChar(local.CsePersonsWorkSet.Sex) == 'M')
              {
                if (ReadCaseRole1())
                {
                  local.ParticipantType.Text3 = "PF";
                }
                else
                {
                  local.ParticipantType.Text3 = "NP";
                }
              }
              else
              {
                local.ParticipantType.Text3 = "NP";
              }

              break;
            case "AR":
              // *****************************************************************
              // Sex is mandatory field. Default to "F" if AR.
              // *****************************************************************
              if (IsEmpty(local.CsePersonsWorkSet.Sex))
              {
                local.CsePersonsWorkSet.Sex = "F";
              }

              local.ParticipantType.Text3 = "CP";

              break;
            default:
              break;
          }

          if (!IsEmpty(entities.CsePerson.FamilyViolenceIndicator))
          {
            local.FamilyViolence.Text3 = "FV";
          }
          else
          {
            local.FamilyViolence.Text3 = "";
          }

          local.ExternalFplsRequest.ApCsePersonNumber =
            local.CsePersonsWorkSet.Number;
          local.ExternalFplsRequest.Sex = local.CsePersonsWorkSet.Sex;
          local.ExternalFplsRequest.ApDateOfBirth = local.CsePersonsWorkSet.Dob;
          local.ExternalFplsRequest.Ssn = local.CsePersonsWorkSet.Ssn;
          local.ExternalFplsRequest.ApFirstName =
            local.CsePersonsWorkSet.FirstName;
          local.ExternalFplsRequest.ApMiddleName =
            local.CsePersonsWorkSet.MiddleInitial;
          local.ExternalFplsRequest.ApFirstLastName =
            local.CsePersonsWorkSet.LastName;

          // *****************************************************************
          // Eliminate the special characters from the City of Birth. PR116735
          // *****************************************************************
          UseCabFcrFormatCityOfBirth();

          if (!IsEmpty(local.CsePerson.BirthPlaceCity))
          {
            if (!IsEmpty(entities.CsePerson.BirthPlaceState))
            {
              // *****************************************************************
              // Validate State of Birth is in FIPS table. PR116735
              // *****************************************************************
              UseCabFcrFormatStateCode();

              if (!IsEmpty(local.CsePerson.BirthPlaceState))
              {
                local.ExternalFplsRequest.ApCityOfBirth =
                  local.CsePerson.BirthPlaceCity ?? Spaces(16);
                local.ExternalFplsRequest.ApStateOrCountryOfBirth =
                  local.CsePerson.BirthPlaceState ?? Spaces(3);
              }
            }
            else if (!IsEmpty(entities.CsePerson.BirthplaceCountry))
            {
              local.ExternalFplsRequest.ApCityOfBirth =
                local.CsePerson.BirthPlaceCity ?? Spaces(16);
              local.ExternalFplsRequest.ApStateOrCountryOfBirth =
                entities.CsePerson.BirthplaceCountry + "*";
            }
          }

          // *****************************************************************
          // EIiminate special characters from father's first name, last name.
          // *****************************************************************
          if (!IsEmpty(entities.CaseRole.FathersFirstName) && !
            IsEmpty(entities.CaseRole.FathersLastName))
          {
            local.BlanksToBeRemoved.FirstName =
              entities.CaseRole.FathersFirstName ?? Spaces(12);
            local.BlanksToBeRemoved.LastName =
              entities.CaseRole.FathersLastName ?? Spaces(17);
            local.BlanksToBeRemoved.MiddleInitial =
              entities.CaseRole.FathersMiddleInitial ?? Spaces(1);
            UseCabFcrFormatNames2();
            local.ExternalFplsRequest.ApsFathersFirstName =
              local.BlanksRemoved.FirstName;
            local.ExternalFplsRequest.ApsFathersMi =
              entities.CaseRole.FathersMiddleInitial ?? Spaces(1);
            local.ExternalFplsRequest.ApsFathersLastName =
              local.BlanksRemoved.LastName;
          }

          if (!IsEmpty(entities.CaseRole.MothersFirstName) && !
            IsEmpty(entities.CaseRole.MothersMaidenLastName))
          {
            // *****************************************************************
            // EIiminate special characters from mother's first name, last name.
            // *****************************************************************
            local.BlanksToBeRemoved.FirstName =
              entities.CaseRole.MothersFirstName ?? Spaces(12);
            local.BlanksToBeRemoved.LastName =
              entities.CaseRole.MothersMaidenLastName ?? Spaces(17);
            local.BlanksToBeRemoved.MiddleInitial =
              entities.CaseRole.MothersMiddleInitial ?? Spaces(1);
            UseCabFcrFormatNames2();
            local.ExternalFplsRequest.ApsMothersFirstName =
              local.BlanksRemoved.FirstName;
            local.ExternalFplsRequest.ApsMothersMi =
              entities.CaseRole.MothersMiddleInitial ?? Spaces(1);
            local.ExternalFplsRequest.ApsMothersMaidenName =
              local.BlanksRemoved.LastName;
          }

          // *****************************************************************
          // Get any additional names or social security numbers using EAB.
          // *****************************************************************
          local.Batch.Flag = "Y";
          UseCabRetrieveAliasesAndAltSsn();

          for(local.Aliases.Index = 0; local.Aliases.Index < local
            .Aliases.Count; ++local.Aliases.Index)
          {
            if (!local.Aliases.CheckSize())
            {
              break;
            }

            switch(local.Aliases.Index + 1)
            {
              case 1:
                local.AdditionalFirstName1.Text16 =
                  local.Aliases.Item.Gname.FirstName;
                local.AdditionalMiddleName1.Text16 =
                  local.Aliases.Item.Gname.MiddleInitial;
                local.AdditionalLastName1.Text32 =
                  local.Aliases.Item.Gname.LastName;

                break;
              case 2:
                local.AdditionalFirstName2.Text16 =
                  local.Aliases.Item.Gname.FirstName;
                local.AdditionalMiddleName2.Text16 =
                  local.Aliases.Item.Gname.MiddleInitial;
                local.AdditionalLastName2.Text32 =
                  local.Aliases.Item.Gname.LastName;

                break;
              case 3:
                local.AdditionalFirstName3.Text16 =
                  local.Aliases.Item.Gname.FirstName;
                local.AdditionalMiddleName3.Text16 =
                  local.Aliases.Item.Gname.MiddleInitial;
                local.AdditionalLastName3.Text32 =
                  local.Aliases.Item.Gname.LastName;

                break;
              case 4:
                local.AdditionalFirstName4.Text16 =
                  local.Aliases.Item.Gname.FirstName;
                local.AdditionalMiddleName4.Text16 =
                  local.Aliases.Item.Gname.MiddleInitial;
                local.AdditionalLastName4.Text32 =
                  local.Aliases.Item.Gname.LastName;

                goto AfterCycle1;
              default:
                break;
            }
          }

AfterCycle1:

          local.Aliases.CheckIndex();

          for(local.AlternateSnn.Index = 0; local.AlternateSnn.Index < local
            .AlternateSnn.Count; ++local.AlternateSnn.Index)
          {
            if (!local.AlternateSnn.CheckSize())
            {
              break;
            }

            switch(local.AlternateSnn.Index + 1)
            {
              case 1:
                local.AdditionalSsn1.Ssn = local.AlternateSnn.Item.Gssn.Ssn;

                break;
              case 2:
                local.AdditionalSsn2.Ssn = local.AlternateSnn.Item.Gssn.Ssn;

                goto AfterCycle2;
              default:
                break;
            }
          }

AfterCycle2:

          local.AlternateSnn.CheckIndex();
          local.ExternalFplsRequest.CpSsn = "";
          local.Irs1099.Flag = "";
          local.NewMemberId.ApCsePersonNumber = "";

          // ******************************************************************
          // Date : 05-31-2000
          // Sree
          // In order to remove duplicate member numbers getting created in the 
          // file, before writing the person record, check if there is an FPLS
          // request for this person if yes write a staus 'CS', else write as a
          // normal record.
          // *******************************************************************
          if (ReadFplsLocateRequest())
          {
            local.Irs1099.Flag = "";
            local.LocateSource1.Text3 = "ALL";
            local.ExternalFplsRequest.StationNumber = "CS";
            local.ExternalFplsRequest.UsersField =
              entities.FplsLocateRequest.CaseId ?? Spaces(7);
            ++local.LocateCount.Count;

            // **********************************************************************************************
            // 11/09/2005
            // 
            // DDupree              WR00258947
            // The following if was added in response to the changes in the fcr 
            // minor release
            // 5-02. A locate request for a child in a IV-D case can now be 
            // requested. Also
            // updated the send request field to include A03.
            // **********************************************************************************************
            if (Equal(local.ParticipantType.Text3, "CH"))
            {
            }
            else
            {
              local.ParticipantType.Text3 = "";
            }

            try
            {
              UpdateFplsLocateRequest();
              ++local.NumberOfUpdates.Count;
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "OE0000_FPLS_REQUEST_NU";

                  goto ReadEach;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "OE0000_FPLS_REQUEST_PV";

                  goto ReadEach;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
          else
          {
            local.Irs1099.Flag = "";
            local.LocateSource1.Text3 = "";
          }

          // *****************************************************************
          // Write the Person record to the extract file.
          // *****************************************************************
          UseOeEabWriteFcrRequests1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            goto ReadEach;
          }
        }
      }
    }

ReadEach:

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabExtractExitStateMessage();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ALL_OK";
      UseOeB411Close();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
    else
    {
      UseOeB411Close();
    }
  }

  private static void MoveAlternateSsnToAlternateSnn(
    CabRetrieveAliasesAndAltSsn.Export.AlternateSsnGroup source,
    Local.AlternateSnnGroup target)
  {
    target.Gssn.Ssn = source.Gssn.Ssn;
  }

  private static void MoveCsePerson1(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.BirthPlaceState = source.BirthPlaceState;
  }

  private static void MoveCsePerson2(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.BirthPlaceCity = source.BirthPlaceCity;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveEabFileHandling(EabFileHandling source,
    EabFileHandling target)
  {
    target.Action = source.Action;
    target.Status = source.Status;
  }

  private static void MoveNamesToAliases(CabRetrieveAliasesAndAltSsn.Export.
    NamesGroup source, Local.AliasesGroup target)
  {
    target.Gname.Assign(source.Gnames);
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

  private void UseCabFcrFormatCityOfBirth()
  {
    var useImport = new CabFcrFormatCityOfBirth.Import();
    var useExport = new CabFcrFormatCityOfBirth.Export();

    MoveCsePerson2(entities.CsePerson, useImport.CsePerson);

    Call(CabFcrFormatCityOfBirth.Execute, useImport, useExport);

    MoveCsePerson2(useExport.CsePerson, local.CsePerson);
  }

  private void UseCabFcrFormatNames1()
  {
    var useImport = new CabFcrFormatNames.Import();
    var useExport = new CabFcrFormatNames.Export();

    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(CabFcrFormatNames.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
  }

  private void UseCabFcrFormatNames2()
  {
    var useImport = new CabFcrFormatNames.Import();
    var useExport = new CabFcrFormatNames.Export();

    useImport.CsePersonsWorkSet.Assign(local.BlanksToBeRemoved);

    Call(CabFcrFormatNames.Execute, useImport, useExport);

    local.BlanksRemoved.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseCabFcrFormatStateCode()
  {
    var useImport = new CabFcrFormatStateCode.Import();
    var useExport = new CabFcrFormatStateCode.Export();

    MoveCsePerson1(entities.CsePerson, useImport.CsePerson);
    useImport.Process.Date = local.ProcessingDate.Date;

    Call(CabFcrFormatStateCode.Execute, useImport, useExport);

    MoveCsePerson1(useExport.CsePerson, local.CsePerson);
  }

  private void UseCabRetrieveAliasesAndAltSsn()
  {
    var useImport = new CabRetrieveAliasesAndAltSsn.Import();
    var useExport = new CabRetrieveAliasesAndAltSsn.Export();

    useImport.Batch.Flag = local.Batch.Flag;
    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(CabRetrieveAliasesAndAltSsn.Execute, useImport, useExport);

    useExport.AlternateSsn.CopyTo(
      local.AlternateSnn, MoveAlternateSsnToAlternateSnn);
    useExport.Names.CopyTo(local.Aliases, MoveNamesToAliases);
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseOeB411Close()
  {
    var useImport = new OeB411Close.Import();
    var useExport = new OeB411Close.Export();

    useImport.Import1099Count.Count = local.Local1099Count.Count;
    useImport.LocateCount.Count = local.LocateCount.Count;
    useImport.TotalCount.Count = local.TotalCount.Count;
    useImport.PersonsCount.Count = local.PersonsCount.Count;
    useImport.CaseCount.Count = local.CaseCount.Count;
    useImport.ProcessingDate.Date = local.ProcessingDate.Date;

    Call(OeB411Close.Execute, useImport, useExport);
  }

  private void UseOeB411Housekeeping()
  {
    var useImport = new OeB411Housekeeping.Import();
    var useExport = new OeB411Housekeeping.Export();

    Call(OeB411Housekeeping.Execute, useImport, useExport);

    local.ProcessingDate.Date = useExport.ProcessingDate.Date;
    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
    local.CurrentDateLessFive.Date = useExport.CurrentDateLessFive.Date;
  }

  private void UseOeEabWriteFcrRequests1()
  {
    var useImport = new OeEabWriteFcrRequests.Import();
    var useExport = new OeEabWriteFcrRequests.Export();

    useImport.AdditionalSsn1.Ssn = local.AdditionalSsn1.Ssn;
    useImport.AdditionalSsn2.Ssn = local.AdditionalSsn2.Ssn;
    useImport.ExternalFplsRequest.Assign(local.ExternalFplsRequest);
    useImport.RecordIdentifier.Text3 = local.RecordIdentifier.Text3;
    useImport.ActionTypeCode.Text1 = local.ActionTypeCode.Text1;
    useImport.CaseId.Text10 = local.CaseId.Text10;
    useImport.CaseType.Text1 = local.CaseType.Text1;
    useImport.OrderIndicator.Text1 = local.OrderIndicator.Text1;
    useImport.ParticipantType.Text3 = local.ParticipantType.Text3;
    useImport.FamilyViolence.Text3 = local.FamilyViolence.Text3;
    useImport.AdditionalFirstName1.Text16 = local.AdditionalFirstName1.Text16;
    useImport.AdditionalMiddleName1.Text16 = local.AdditionalMiddleName1.Text16;
    useImport.AdditionalLastName1.Text32 = local.AdditionalLastName1.Text32;
    useImport.AdditionalFirstName2.Text16 = local.AdditionalFirstName2.Text16;
    useImport.AdditionalMiddleName2.Text16 = local.AdditionalMiddleName2.Text16;
    useImport.AdditionalLastName2.Text32 = local.AdditionalLastName2.Text32;
    useImport.AdditionalFirstName3.Text16 = local.AdditionalFirstName3.Text16;
    useImport.AdditionalMiddleName3.Text16 = local.AdditionalMiddleName3.Text16;
    useImport.AdditionalLastName3.Text32 = local.AdditionalLastName3.Text32;
    useImport.AdditionalFirstName4.Text16 = local.AdditionalFirstName4.Text16;
    useImport.AdditionalMiddleName4.Text16 = local.AdditionalMiddleName4.Text16;
    useImport.AddtionalLastName4.Text32 = local.AdditionalLastName4.Text32;
    useImport.NewMemberId.ApCsePersonNumber =
      local.NewMemberId.ApCsePersonNumber;
    useImport.Irs1099.Flag = local.Irs1099.Flag;
    useImport.LocateSource1.Text3 = local.LocateSource1.Text3;
    useImport.ProcessDate.Date = local.ProcessingDate.Date;
    MoveEabFileHandling(local.EabFileHandling, useImport.EabFileHandling);
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(OeEabWriteFcrRequests.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseOeEabWriteFcrRequests2()
  {
    var useImport = new OeEabWriteFcrRequests.Import();
    var useExport = new OeEabWriteFcrRequests.Export();

    useImport.ExternalFplsRequest.Assign(local.ExternalFplsRequest);
    useImport.RecordIdentifier.Text3 = local.RecordIdentifier.Text3;
    useImport.ActionTypeCode.Text1 = local.ActionTypeCode.Text1;
    useImport.CaseId.Text10 = local.CaseId.Text10;
    useImport.CaseType.Text1 = local.CaseType.Text1;
    useImport.OrderIndicator.Text1 = local.OrderIndicator.Text1;
    useImport.ProcessDate.Date = local.ProcessingDate.Date;
    MoveEabFileHandling(local.EabFileHandling, useImport.EabFileHandling);
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(OeEabWriteFcrRequests.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSiReadCsePersonBatch()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      null,
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadCaseRole1()
  {
    entities.Other.Populated = false;

    return Read("ReadCaseRole1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.ProcessingDate.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Other.CasNumber = db.GetString(reader, 0);
        entities.Other.CspNumber = db.GetString(reader, 1);
        entities.Other.Type1 = db.GetString(reader, 2);
        entities.Other.Identifier = db.GetInt32(reader, 3);
        entities.Other.StartDate = db.GetNullableDate(reader, 4);
        entities.Other.EndDate = db.GetNullableDate(reader, 5);
        entities.Other.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Other.Type1);
      });
  }

  private IEnumerable<bool> ReadCaseRole2()
  {
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRole2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.ProcessingDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.MothersFirstName = db.GetNullableString(reader, 6);
        entities.CaseRole.MothersMiddleInitial =
          db.GetNullableString(reader, 7);
        entities.CaseRole.FathersLastName = db.GetNullableString(reader, 8);
        entities.CaseRole.FathersMiddleInitial =
          db.GetNullableString(reader, 9);
        entities.CaseRole.FathersFirstName = db.GetNullableString(reader, 10);
        entities.CaseRole.MothersMaidenLastName =
          db.GetNullableString(reader, 11);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRole3()
  {
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRole3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "endDate", local.ProcessingDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.MothersFirstName = db.GetNullableString(reader, 6);
        entities.CaseRole.MothersMiddleInitial =
          db.GetNullableString(reader, 7);
        entities.CaseRole.FathersLastName = db.GetNullableString(reader, 8);
        entities.CaseRole.FathersMiddleInitial =
          db.GetNullableString(reader, 9);
        entities.CaseRole.FathersFirstName = db.GetNullableString(reader, 10);
        entities.CaseRole.MothersMaidenLastName =
          db.GetNullableString(reader, 11);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRoleCsePerson()
  {
    entities.CsePerson.Populated = false;
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRoleCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.ProcessingDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.MothersFirstName = db.GetNullableString(reader, 6);
        entities.CaseRole.MothersMiddleInitial =
          db.GetNullableString(reader, 7);
        entities.CaseRole.FathersLastName = db.GetNullableString(reader, 8);
        entities.CaseRole.FathersMiddleInitial =
          db.GetNullableString(reader, 9);
        entities.CaseRole.FathersFirstName = db.GetNullableString(reader, 10);
        entities.CaseRole.MothersMaidenLastName =
          db.GetNullableString(reader, 11);
        entities.CsePerson.Type1 = db.GetString(reader, 12);
        entities.CsePerson.BirthPlaceState = db.GetNullableString(reader, 13);
        entities.CsePerson.BirthPlaceCity = db.GetNullableString(reader, 14);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 15);
        entities.CsePerson.BirthplaceCountry = db.GetNullableString(reader, 16);
        entities.CsePerson.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.CaseRole.CspNumber);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.BirthPlaceState = db.GetNullableString(reader, 2);
        entities.CsePerson.BirthPlaceCity = db.GetNullableString(reader, 3);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 4);
        entities.CsePerson.BirthplaceCountry = db.GetNullableString(reader, 5);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadFplsLocateRequest()
  {
    entities.FplsLocateRequest.Populated = false;

    return Read("ReadFplsLocateRequest",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "requestSentDate1",
          local.CurrentDateLessFive.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "requestSentDate2", local.Null1.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.FplsLocateRequest.CspNumber = db.GetString(reader, 0);
        entities.FplsLocateRequest.Identifier = db.GetInt32(reader, 1);
        entities.FplsLocateRequest.TransactionStatus =
          db.GetNullableString(reader, 2);
        entities.FplsLocateRequest.CaseId = db.GetNullableString(reader, 3);
        entities.FplsLocateRequest.LastUpdatedBy = db.GetString(reader, 4);
        entities.FplsLocateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.FplsLocateRequest.RequestSentDate =
          db.GetNullableDate(reader, 6);
        entities.FplsLocateRequest.SendRequestTo =
          db.GetNullableString(reader, 7);
        entities.FplsLocateRequest.Populated = true;
      });
  }

  private bool ReadLegalActionCaseRole()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);
    entities.LegalActionCaseRole.Populated = false;

    return Read("ReadLegalActionCaseRole",
      (db, command) =>
      {
        db.SetInt32(command, "croIdentifier", entities.CaseRole.Identifier);
        db.SetString(command, "croType", entities.CaseRole.Type1);
        db.SetString(command, "cspNumber", entities.CaseRole.CspNumber);
        db.SetString(command, "casNumber", entities.CaseRole.CasNumber);
        db.SetNullableDate(
          command, "filedDt", local.Null1.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDt", local.ProcessingDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalActionCaseRole.CasNumber = db.GetString(reader, 0);
        entities.LegalActionCaseRole.CspNumber = db.GetString(reader, 1);
        entities.LegalActionCaseRole.CroType = db.GetString(reader, 2);
        entities.LegalActionCaseRole.CroIdentifier = db.GetInt32(reader, 3);
        entities.LegalActionCaseRole.LgaId = db.GetInt32(reader, 4);
        entities.LegalActionCaseRole.InitialCreationInd =
          db.GetString(reader, 5);
        entities.LegalActionCaseRole.Populated = true;
        CheckValid<LegalActionCaseRole>("CroType",
          entities.LegalActionCaseRole.CroType);
      });
  }

  private void UpdateFplsLocateRequest()
  {
    System.Diagnostics.Debug.Assert(entities.FplsLocateRequest.Populated);

    var transactionStatus = "S";
    var lastUpdatedBy = global.TranCode;
    var lastUpdatedTimestamp = Now();
    var requestSentDate = local.ProgramProcessingInfo.ProcessDate;
    var sendRequestTo = "A01A02A03C01C02E01E02F01";

    entities.FplsLocateRequest.Populated = false;
    Update("UpdateFplsLocateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "transactionStatus", transactionStatus);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTimes", lastUpdatedTimestamp);
        db.SetNullableDate(command, "requestSentDate", requestSentDate);
        db.SetNullableString(command, "sendRequestTo", sendRequestTo);
        db.
          SetString(command, "cspNumber", entities.FplsLocateRequest.CspNumber);
          
        db.
          SetInt32(command, "identifier", entities.FplsLocateRequest.Identifier);
          
      });

    entities.FplsLocateRequest.TransactionStatus = transactionStatus;
    entities.FplsLocateRequest.LastUpdatedBy = lastUpdatedBy;
    entities.FplsLocateRequest.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.FplsLocateRequest.RequestSentDate = requestSentDate;
    entities.FplsLocateRequest.SendRequestTo = sendRequestTo;
    entities.FplsLocateRequest.Populated = true;
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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A AlternateSnnGroup group.</summary>
    [Serializable]
    public class AlternateSnnGroup
    {
      /// <summary>
      /// A value of Gssn.
      /// </summary>
      [JsonPropertyName("gssn")]
      public CsePersonsWorkSet Gssn
      {
        get => gssn ??= new();
        set => gssn = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private CsePersonsWorkSet gssn;
    }

    /// <summary>A AliasesGroup group.</summary>
    [Serializable]
    public class AliasesGroup
    {
      /// <summary>
      /// A value of Gname.
      /// </summary>
      [JsonPropertyName("gname")]
      public CsePersonsWorkSet Gname
      {
        get => gname ??= new();
        set => gname = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private CsePersonsWorkSet gname;
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
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    /// <summary>
    /// A value of Batch.
    /// </summary>
    [JsonPropertyName("batch")]
    public Common Batch
    {
      get => batch ??= new();
      set => batch = value;
    }

    /// <summary>
    /// Gets a value of AlternateSnn.
    /// </summary>
    [JsonIgnore]
    public Array<AlternateSnnGroup> AlternateSnn => alternateSnn ??= new(
      AlternateSnnGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of AlternateSnn for json serialization.
    /// </summary>
    [JsonPropertyName("alternateSnn")]
    [Computed]
    public IList<AlternateSnnGroup> AlternateSnn_Json
    {
      get => alternateSnn;
      set => AlternateSnn.Assign(value);
    }

    /// <summary>
    /// Gets a value of Aliases.
    /// </summary>
    [JsonIgnore]
    public Array<AliasesGroup> Aliases => aliases ??= new(
      AliasesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Aliases for json serialization.
    /// </summary>
    [JsonPropertyName("aliases")]
    [Computed]
    public IList<AliasesGroup> Aliases_Json
    {
      get => aliases;
      set => Aliases.Assign(value);
    }

    /// <summary>
    /// A value of Local1099Count.
    /// </summary>
    [JsonPropertyName("local1099Count")]
    public Common Local1099Count
    {
      get => local1099Count ??= new();
      set => local1099Count = value;
    }

    /// <summary>
    /// A value of LocateCount.
    /// </summary>
    [JsonPropertyName("locateCount")]
    public Common LocateCount
    {
      get => locateCount ??= new();
      set => locateCount = value;
    }

    /// <summary>
    /// A value of WritePerson.
    /// </summary>
    [JsonPropertyName("writePerson")]
    public Common WritePerson
    {
      get => writePerson ??= new();
      set => writePerson = value;
    }

    /// <summary>
    /// A value of IsName.
    /// </summary>
    [JsonPropertyName("isName")]
    public Common IsName
    {
      get => isName ??= new();
      set => isName = value;
    }

    /// <summary>
    /// A value of IsDob.
    /// </summary>
    [JsonPropertyName("isDob")]
    public Common IsDob
    {
      get => isDob ??= new();
      set => isDob = value;
    }

    /// <summary>
    /// A value of IsSsn.
    /// </summary>
    [JsonPropertyName("isSsn")]
    public Common IsSsn
    {
      get => isSsn ??= new();
      set => isSsn = value;
    }

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
    /// A value of WriteCase.
    /// </summary>
    [JsonPropertyName("writeCase")]
    public Common WriteCase
    {
      get => writeCase ??= new();
      set => writeCase = value;
    }

    /// <summary>
    /// A value of BlanksToBeRemoved.
    /// </summary>
    [JsonPropertyName("blanksToBeRemoved")]
    public CsePersonsWorkSet BlanksToBeRemoved
    {
      get => blanksToBeRemoved ??= new();
      set => blanksToBeRemoved = value;
    }

    /// <summary>
    /// A value of BlanksRemoved.
    /// </summary>
    [JsonPropertyName("blanksRemoved")]
    public CsePersonsWorkSet BlanksRemoved
    {
      get => blanksRemoved ??= new();
      set => blanksRemoved = value;
    }

    /// <summary>
    /// A value of Position.
    /// </summary>
    [JsonPropertyName("position")]
    public Common Position
    {
      get => position ??= new();
      set => position = value;
    }

    /// <summary>
    /// A value of IsFound.
    /// </summary>
    [JsonPropertyName("isFound")]
    public Common IsFound
    {
      get => isFound ??= new();
      set => isFound = value;
    }

    /// <summary>
    /// A value of TempFips.
    /// </summary>
    [JsonPropertyName("tempFips")]
    public WorkArea TempFips
    {
      get => tempFips ??= new();
      set => tempFips = value;
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
    /// A value of AdditionalLastName4.
    /// </summary>
    [JsonPropertyName("additionalLastName4")]
    public WorkArea AdditionalLastName4
    {
      get => additionalLastName4 ??= new();
      set => additionalLastName4 = value;
    }

    /// <summary>
    /// A value of AdditionalMiddleName4.
    /// </summary>
    [JsonPropertyName("additionalMiddleName4")]
    public WorkArea AdditionalMiddleName4
    {
      get => additionalMiddleName4 ??= new();
      set => additionalMiddleName4 = value;
    }

    /// <summary>
    /// A value of AdditionalFirstName4.
    /// </summary>
    [JsonPropertyName("additionalFirstName4")]
    public WorkArea AdditionalFirstName4
    {
      get => additionalFirstName4 ??= new();
      set => additionalFirstName4 = value;
    }

    /// <summary>
    /// A value of AdditionalLastName3.
    /// </summary>
    [JsonPropertyName("additionalLastName3")]
    public WorkArea AdditionalLastName3
    {
      get => additionalLastName3 ??= new();
      set => additionalLastName3 = value;
    }

    /// <summary>
    /// A value of AdditionalMiddleName3.
    /// </summary>
    [JsonPropertyName("additionalMiddleName3")]
    public WorkArea AdditionalMiddleName3
    {
      get => additionalMiddleName3 ??= new();
      set => additionalMiddleName3 = value;
    }

    /// <summary>
    /// A value of AdditionalFirstName3.
    /// </summary>
    [JsonPropertyName("additionalFirstName3")]
    public WorkArea AdditionalFirstName3
    {
      get => additionalFirstName3 ??= new();
      set => additionalFirstName3 = value;
    }

    /// <summary>
    /// A value of AdditionalLastName2.
    /// </summary>
    [JsonPropertyName("additionalLastName2")]
    public WorkArea AdditionalLastName2
    {
      get => additionalLastName2 ??= new();
      set => additionalLastName2 = value;
    }

    /// <summary>
    /// A value of AdditionalMiddleName2.
    /// </summary>
    [JsonPropertyName("additionalMiddleName2")]
    public WorkArea AdditionalMiddleName2
    {
      get => additionalMiddleName2 ??= new();
      set => additionalMiddleName2 = value;
    }

    /// <summary>
    /// A value of AdditionalFirstName2.
    /// </summary>
    [JsonPropertyName("additionalFirstName2")]
    public WorkArea AdditionalFirstName2
    {
      get => additionalFirstName2 ??= new();
      set => additionalFirstName2 = value;
    }

    /// <summary>
    /// A value of AdditionalLastName1.
    /// </summary>
    [JsonPropertyName("additionalLastName1")]
    public WorkArea AdditionalLastName1
    {
      get => additionalLastName1 ??= new();
      set => additionalLastName1 = value;
    }

    /// <summary>
    /// A value of AdditionalMiddleName1.
    /// </summary>
    [JsonPropertyName("additionalMiddleName1")]
    public WorkArea AdditionalMiddleName1
    {
      get => additionalMiddleName1 ??= new();
      set => additionalMiddleName1 = value;
    }

    /// <summary>
    /// A value of AdditionalFirstName1.
    /// </summary>
    [JsonPropertyName("additionalFirstName1")]
    public WorkArea AdditionalFirstName1
    {
      get => additionalFirstName1 ??= new();
      set => additionalFirstName1 = value;
    }

    /// <summary>
    /// A value of AdditionalSsn1.
    /// </summary>
    [JsonPropertyName("additionalSsn1")]
    public ExternalFplsRequest AdditionalSsn1
    {
      get => additionalSsn1 ??= new();
      set => additionalSsn1 = value;
    }

    /// <summary>
    /// A value of AdditionalSsn2.
    /// </summary>
    [JsonPropertyName("additionalSsn2")]
    public ExternalFplsRequest AdditionalSsn2
    {
      get => additionalSsn2 ??= new();
      set => additionalSsn2 = value;
    }

    /// <summary>
    /// A value of LocateSource1.
    /// </summary>
    [JsonPropertyName("locateSource1")]
    public WorkArea LocateSource1
    {
      get => locateSource1 ??= new();
      set => locateSource1 = value;
    }

    /// <summary>
    /// A value of Irs1099.
    /// </summary>
    [JsonPropertyName("irs1099")]
    public Common Irs1099
    {
      get => irs1099 ??= new();
      set => irs1099 = value;
    }

    /// <summary>
    /// A value of NewMemberId.
    /// </summary>
    [JsonPropertyName("newMemberId")]
    public ExternalFplsRequest NewMemberId
    {
      get => newMemberId ??= new();
      set => newMemberId = value;
    }

    /// <summary>
    /// A value of FamilyViolence.
    /// </summary>
    [JsonPropertyName("familyViolence")]
    public WorkArea FamilyViolence
    {
      get => familyViolence ??= new();
      set => familyViolence = value;
    }

    /// <summary>
    /// A value of ParticipantType.
    /// </summary>
    [JsonPropertyName("participantType")]
    public WorkArea ParticipantType
    {
      get => participantType ??= new();
      set => participantType = value;
    }

    /// <summary>
    /// A value of OrderIndicator.
    /// </summary>
    [JsonPropertyName("orderIndicator")]
    public TextWorkArea OrderIndicator
    {
      get => orderIndicator ??= new();
      set => orderIndicator = value;
    }

    /// <summary>
    /// A value of CaseType.
    /// </summary>
    [JsonPropertyName("caseType")]
    public TextWorkArea CaseType
    {
      get => caseType ??= new();
      set => caseType = value;
    }

    /// <summary>
    /// A value of CaseId.
    /// </summary>
    [JsonPropertyName("caseId")]
    public WorkArea CaseId
    {
      get => caseId ??= new();
      set => caseId = value;
    }

    /// <summary>
    /// A value of ActionTypeCode.
    /// </summary>
    [JsonPropertyName("actionTypeCode")]
    public TextWorkArea ActionTypeCode
    {
      get => actionTypeCode ??= new();
      set => actionTypeCode = value;
    }

    /// <summary>
    /// A value of RecordIdentifier.
    /// </summary>
    [JsonPropertyName("recordIdentifier")]
    public WorkArea RecordIdentifier
    {
      get => recordIdentifier ??= new();
      set => recordIdentifier = value;
    }

    /// <summary>
    /// A value of TotalCount.
    /// </summary>
    [JsonPropertyName("totalCount")]
    public Common TotalCount
    {
      get => totalCount ??= new();
      set => totalCount = value;
    }

    /// <summary>
    /// A value of PersonsCount.
    /// </summary>
    [JsonPropertyName("personsCount")]
    public Common PersonsCount
    {
      get => personsCount ??= new();
      set => personsCount = value;
    }

    /// <summary>
    /// A value of CaseCount.
    /// </summary>
    [JsonPropertyName("caseCount")]
    public Common CaseCount
    {
      get => caseCount ??= new();
      set => caseCount = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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
    /// A value of ProgramRun.
    /// </summary>
    [JsonPropertyName("programRun")]
    public ProgramRun ProgramRun
    {
      get => programRun ??= new();
      set => programRun = value;
    }

    /// <summary>
    /// A value of TotalUpdates.
    /// </summary>
    [JsonPropertyName("totalUpdates")]
    public Common TotalUpdates
    {
      get => totalUpdates ??= new();
      set => totalUpdates = value;
    }

    /// <summary>
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public DateWorkArea NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
    }

    /// <summary>
    /// A value of CurrentDateLessFive.
    /// </summary>
    [JsonPropertyName("currentDateLessFive")]
    public DateWorkArea CurrentDateLessFive
    {
      get => currentDateLessFive ??= new();
      set => currentDateLessFive = value;
    }

    /// <summary>
    /// A value of NumberOfUpdates.
    /// </summary>
    [JsonPropertyName("numberOfUpdates")]
    public Common NumberOfUpdates
    {
      get => numberOfUpdates ??= new();
      set => numberOfUpdates = value;
    }

    /// <summary>
    /// A value of ExternalFplsRequest.
    /// </summary>
    [JsonPropertyName("externalFplsRequest")]
    public ExternalFplsRequest ExternalFplsRequest
    {
      get => externalFplsRequest ??= new();
      set => externalFplsRequest = value;
    }

    /// <summary>
    /// A value of Init1.
    /// </summary>
    [JsonPropertyName("init1")]
    public ExternalFplsRequest Init1
    {
      get => init1 ??= new();
      set => init1 = value;
    }

    /// <summary>
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public DateWorkArea Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
    }

    private CsePerson csePerson;
    private ExitStateWorkArea exitStateWorkArea;
    private Common batch;
    private Array<AlternateSnnGroup> alternateSnn;
    private Array<AliasesGroup> aliases;
    private Common local1099Count;
    private Common locateCount;
    private Common writePerson;
    private Common isName;
    private Common isDob;
    private Common isSsn;
    private DateWorkArea null1;
    private Common writeCase;
    private CsePersonsWorkSet blanksToBeRemoved;
    private CsePersonsWorkSet blanksRemoved;
    private Common position;
    private Common isFound;
    private WorkArea tempFips;
    private CsePersonsWorkSet csePersonsWorkSet;
    private WorkArea additionalLastName4;
    private WorkArea additionalMiddleName4;
    private WorkArea additionalFirstName4;
    private WorkArea additionalLastName3;
    private WorkArea additionalMiddleName3;
    private WorkArea additionalFirstName3;
    private WorkArea additionalLastName2;
    private WorkArea additionalMiddleName2;
    private WorkArea additionalFirstName2;
    private WorkArea additionalLastName1;
    private WorkArea additionalMiddleName1;
    private WorkArea additionalFirstName1;
    private ExternalFplsRequest additionalSsn1;
    private ExternalFplsRequest additionalSsn2;
    private WorkArea locateSource1;
    private Common irs1099;
    private ExternalFplsRequest newMemberId;
    private WorkArea familyViolence;
    private WorkArea participantType;
    private TextWorkArea orderIndicator;
    private TextWorkArea caseType;
    private WorkArea caseId;
    private TextWorkArea actionTypeCode;
    private WorkArea recordIdentifier;
    private Common totalCount;
    private Common personsCount;
    private Common caseCount;
    private DateWorkArea processingDate;
    private EabFileHandling eabFileHandling;
    private ProgramProcessingInfo programProcessingInfo;
    private EabReportSend eabReportSend;
    private ProgramRun programRun;
    private Common totalUpdates;
    private DateWorkArea nullDate;
    private DateWorkArea currentDateLessFive;
    private Common numberOfUpdates;
    private ExternalFplsRequest externalFplsRequest;
    private ExternalFplsRequest init1;
    private DateWorkArea zdel;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Other.
    /// </summary>
    [JsonPropertyName("other")]
    public CaseRole Other
    {
      get => other ??= new();
      set => other = value;
    }

    /// <summary>
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
    }

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
    /// A value of Starved.
    /// </summary>
    [JsonPropertyName("starved")]
    public CsePerson Starved
    {
      get => starved ??= new();
      set => starved = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of FplsLocateRequest.
    /// </summary>
    [JsonPropertyName("fplsLocateRequest")]
    public FplsLocateRequest FplsLocateRequest
    {
      get => fplsLocateRequest ??= new();
      set => fplsLocateRequest = value;
    }

    private CaseRole other;
    private LegalActionCaseRole legalActionCaseRole;
    private LegalAction legalAction;
    private CsePerson starved;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private Case1 case1;
    private FplsLocateRequest fplsLocateRequest;
  }
#endregion
}
