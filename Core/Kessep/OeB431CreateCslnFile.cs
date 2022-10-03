// Program: OE_B431_CREATE_CSLN_FILE, ID: 945242773, model: 746.
// Short name: SWEB431P
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B431_CREATE_CSLN_FILE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB431CreateCslnFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B431_CREATE_CSLN_FILE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB431CreateCslnFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB431CreateCslnFile.
  /// </summary>
  public OeB431CreateCslnFile(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***********************
    // DEVELOPMENT AND CHANGE LOG
    // *********************************
    // 12-2012  A Hockman    Initial development.
    // 11-2013  A Hockman  changed read of debt detail to include obligation 
    // type and
    //                      make the read more elaborate to not include current 
    // as part of
    //                       total arrears owed
    // 02-2014  A Hockman changed code to clear debt when err is encountered and
    // we switch
    //                   to another court order.  The code was in there to do so
    // but needed
    //                    moved out one bracket.
    // 03-2017  A Hockman  cq55972 changed read to get kids that are inactive on
    // case.
    // ***********************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    // ********************************************
    //        PPI and checkpoint restart
    //         control and error reports
    // ********************************************
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.Max.Date = new DateTime(2099, 12, 31);
    local.Start.Count = 1;
    local.Current.Count = 1;
    local.CurrentPosition.Count = 1;
    local.FieldNumber.Count = 0;
    local.IncludeArrearsOnly.Flag = "";

    // **********************************************
    // Retrieve the MPPI parms: state attorney info.
    // **********************************************
    do
    {
      local.Postion.Text1 =
        Substring(local.ProgramProcessingInfo.ParameterList,
        local.CurrentPosition.Count, 1);

      if (AsChar(local.Postion.Text1) == ';')
      {
        ++local.FieldNumber.Count;
        local.WorkArea.Text15 = "";

        switch(local.FieldNumber.Count)
        {
          case 1:
            if (local.Current.Count == 1)
            {
              local.PrepByAttyFirstName.Text12 = "";
            }
            else
            {
              local.PrepByAttyFirstName.Text12 =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.Start.Count, local.Current.Count - 1);
            }

            local.Start.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            break;
          case 2:
            if (local.Current.Count == 1)
            {
              local.PrepByAttyMiddleInitia.Text1 = "";
            }
            else
            {
              local.PrepByAttyMiddleInitia.Text1 =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.Start.Count, local.Current.Count - 1);
            }

            local.Start.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            break;
          case 3:
            if (local.Current.Count == 1)
            {
              local.PrepByAttyLastName.Text17 = "";
            }
            else
            {
              local.PrepByAttyLastName.Text17 =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.Start.Count, local.Current.Count - 1);
            }

            local.Start.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            break;
          case 4:
            if (local.Current.Count == 1)
            {
              local.PrepByAttyCertificate.Text10 = "";
            }
            else
            {
              local.PrepByAttyCertificate.Text10 =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.Start.Count, local.Current.Count - 1);
            }

            local.Start.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            break;
          case 5:
            if (local.Current.Count == 1)
            {
              local.PrepByAttyAddress1.Text60 = "";
            }
            else
            {
              local.PrepByAttyAddress1.Text60 =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.Start.Count, local.Current.Count - 1);
            }

            local.Start.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            break;
          case 6:
            if (local.Current.Count == 1)
            {
              local.PrepByAttyAddress2.Text25 = "";
            }
            else
            {
              local.PrepByAttyAddress2.Text25 =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.Start.Count, local.Current.Count - 1);
            }

            local.Start.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            break;
          case 7:
            if (local.Current.Count == 1)
            {
              local.PrepByAttyCity.Text15 = "";
            }
            else
            {
              local.PrepByAttyCity.Text15 =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.Start.Count, local.Current.Count - 1);
            }

            local.Start.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            break;
          case 8:
            if (local.Current.Count == 1)
            {
              local.PrepByAttyState.Text2 = "";
            }
            else
            {
              local.PrepByAttyState.Text2 =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.Start.Count, local.Current.Count - 1);
            }

            local.Start.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            break;
          case 9:
            if (local.Current.Count == 1)
            {
              local.PrepByAttyZip.Text9 = "";
            }
            else
            {
              local.PrepByAttyZip.Text9 =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.Start.Count, local.Current.Count - 1);
            }

            local.Start.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            break;
          case 10:
            if (local.Current.Count == 1)
            {
              local.PrepByAttyPhone.Text10 = "";
            }
            else
            {
              local.PrepByAttyPhone.Text10 =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.Start.Count, local.Current.Count - 1);
            }

            local.Start.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            break;
          case 11:
            if (local.Current.Count == 1)
            {
              local.PrepByAttyFax.Text10 = "";
            }
            else
            {
              local.PrepByAttyFax.Text10 =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.Start.Count, local.Current.Count - 1);
            }

            local.Start.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            break;
          case 12:
            if (local.Current.Count == 1)
            {
              local.PrepByAttyEmail.Text50 = "";
            }
            else
            {
              local.PrepByAttyEmail.Text50 =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.Start.Count, local.Current.Count - 1);
            }

            local.Start.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            break;
          default:
            break;
        }
      }
      else if (AsChar(local.Postion.Text1) == '*')
      {
        break;

        // *******************************************
        // end of parms so we need to get out of loop
        // *******************************************
      }

      ++local.CurrentPosition.Count;
      ++local.Current.Count;
    }
    while(!Equal(global.Command, "COMMAND"));

    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmChkpntRestart();
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = "SWEEB431";
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered opening control report.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ************************************************
    // *    Call External to Open the Flat File.      *
    // ************************************************
    local.PassArea.FileInstruction = "OPEN";
    UseOeEabWriteCslnFile10();

    if (!Equal(local.PassArea.TextReturnCode, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error in file open for 'oe_eab_write_CSLN_file'.";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "FILE_OPEN_ERROR";

      return;
    }

    local.FileOpened.Flag = "Y";
    local.PassArea.FileInstruction = "WRITE";
    local.HeaderRecord.Flag = "Y";
    local.PassArea.FileInstruction = "WRITE";
    local.RecordType.Text2 = "00";
    local.FileHeaderFileName.Text30 = "KANSAS CSLN SUBMISSION FILE";
    local.FileHeaderTimestamp.Timestamp = Now();
    local.FileHeaderRecordType.Text2 = "00";

    // *************************
    // Write header 00 record
    // *************************
    UseOeEabWriteCslnFile9();

    if (!Equal(local.PassArea.TextReturnCode, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error in writing header record in external file  for 'oe_eab_write_csln_file'.";
        
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.ProcessingDate.Date = local.ProgramProcessingInfo.ProcessDate;
    UseCabFirstAndLastDateOfMonth();
    local.Restart.CaseNumber = "";
    local.CourtOrder.Index = -1;

    // *** Main read
    foreach(var item in ReadAdministrativeActCertification())
    {
      if (Equal(entities.AdministrativeActCertification.CaseNumber,
        local.AdministrativeActCertification.CaseNumber))
      {
        continue;
      }

      if (AsChar(entities.AdministrativeActCertification.
        EtypeFinancialInstitution) == 'Y')
      {
        // *******************************************************************
        // note that administrative_act_certification case_number is mis-named
        // it is actually a PERSON NUMBER not a CASE NUMBER.
        // *******************************************************************
        local.AdministrativeActCertification.CaseNumber =
          entities.AdministrativeActCertification.CaseNumber;

        continue;
      }

      if (Equal(entities.AdministrativeActCertification.Type1, "FDSO") && !
        Lt(entities.AdministrativeActCertification.DecertifiedDate,
        entities.AdministrativeActCertification.TakenDate))
      {
        local.AdministrativeActCertification.CaseNumber =
          entities.AdministrativeActCertification.CaseNumber;

        continue;
      }

      if (!Lt(entities.AdministrativeActCertification.CurrentAmount, 500))
      {
        // *******************************************************************
        // note that administrative_act_certification case_number is mis-named
        // it is actually a PERSON NUMBER not a CASE NUMBER.
        // *******************************************************************
        if (!IsEmpty(local.AdministrativeActCertification.CaseNumber))
        {
          if (!IsEmpty(local.StandardCourtOrderNumbe.Text20))
          {
            if (local.ScreenOwedAmounts.ArrearsAmountOwed == 0)
            {
              goto Test1;
            }

            if (!ReadTribunalFips())
            {
              goto Test1;
            }

            local.StandardCourtOrderNumbe.Text20 =
              local.Previous.StandardNumber ?? Spaces(20);
            local.IwoCabErrorMessages.Update.ErrorType.Text50 = "";
            local.IwoCabErrorMessages.Update.ErrorCourtCase.StandardNumber = "";
            UseCabGetIwoAmtsForCslnFile();

            // edited this read and removed the child case role end date greater
            // than the local ppi date line  AH 3/17/17
            local.ChildPrevious.Number = "";

            if (ReadCsePerson2())
            {
              local.ChildPrevious.Number = entities.ChCsePerson.Number;
            }

            if (IsEmpty(local.ChildPrevious.Number))
            {
              // *****************************************************************
              //  create error for when no children are found for this court 
              // order
              // ******************************************************************
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Court order without children, NCP: " + local
                .AdministrativeActCertification.CaseNumber + " Court order: " +
                (local.Previous.StandardNumber ?? "");
              UseCabErrorReport2();

              goto Test1;
            }

            // **************
            // 1ST Get CP data
            // **************
            if (local.ScreenOwedAmounts.CurrentAmountOwed > 0 || local
              .ScreenOwedAmounts.ArrearsAmountOwed > 0)
            {
              local.IwoFound.Flag = "";
              local.Iwo.CreatedTstamp = local.Null1.Timestamp;

              foreach(var item1 in ReadLegalAction())
              {
                local.Iwo.CreatedTstamp = entities.LegalAction.CreatedTstamp;

                if (Equal(entities.LegalAction.ActionTaken, "IWOTERM") || Equal
                  (entities.LegalAction.ActionTaken, "IWONOTKT"))
                {
                  local.IwoFound.Flag = "N";

                  break;
                }

                if (Equal(entities.LegalAction.ActionTaken, "IWOMODO") || Equal
                  (entities.LegalAction.ActionTaken, "IWONOTKM") || Equal
                  (entities.LegalAction.ActionTaken, "IWONOTKS") || Equal
                  (entities.LegalAction.ActionTaken, "ORDIWO2") || Equal
                  (entities.LegalAction.ActionTaken, "NOIIWON") || Equal
                  (entities.LegalAction.ActionTaken, "IWO"))
                {
                  local.IwoFound.Flag = "Y";

                  break;
                }
                else
                {
                  local.IwoFound.Flag = "N";

                  break;
                }
              }

              if (AsChar(local.IwoFound.Flag) == 'Y')
              {
                if (ReadCsePersonCase2())
                {
                  ++local.CourtOrder.Index;
                  local.CourtOrder.CheckSize();

                  local.CourtOrder.Update.CourtOrderCaseNum.Text10 =
                    entities.Case1.Number;
                  local.Read.Number = entities.Case1.Number;

                  // ************************************************************************
                  //  check to see if person type is C for Client, if not 
                  // default to organization name
                  // ************************************************************************
                  if (AsChar(entities.CsePerson.Type1) == 'C')
                  {
                    local.Cp.Number = entities.CsePerson.Number;
                    UseSiReadCsePersonBatch2();

                    if (!IsEmpty(local.AbendData.AdabasResponseCd))
                    {
                      local.EabFileHandling.Action = "WRITE";
                      local.EabReportSend.RptDetail =
                        "Error getting CP   data for CSLN file.  Case: " + entities
                        .Case1.Number + " Person # " + entities
                        .CsePerson.Number + " SSN: " + local.Cp.Ssn;
                      UseCabErrorReport2();

                      if (!Equal(local.EabFileHandling.Status, "OK"))
                      {
                        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                        return;
                      }
                    }

                    local.CourtOrder.Update.Cp.FirstName = local.Cp.FirstName;
                    local.CourtOrder.Update.Cp.LastName = local.Cp.LastName;
                    local.CourtOrder.Update.Cp.MiddleInitial =
                      local.Cp.MiddleInitial;
                    local.CourtOrder.Update.Cp.Number = local.Cp.Number;
                  }
                  else
                  {
                    local.CourtOrder.Update.Cp.FirstName = "";
                    local.CourtOrder.Update.Cp.LastName = "";
                    local.CourtOrder.Update.Cp.MiddleInitial = "";
                    local.CourtOrder.Update.CpOrganization.Text33 =
                      entities.CsePerson.OrganizationName ?? Spaces(33);
                  }
                }
                else
                {
                  // first look for cp with active child
                  if (ReadCaseCsePerson1())
                  {
                    ++local.CourtOrder.Index;
                    local.CourtOrder.CheckSize();

                    local.CourtOrder.Update.CourtOrderCaseNum.Text10 =
                      entities.Case1.Number;
                    local.Read.Number = entities.Case1.Number;

                    // ************************************************************************
                    //  check to see if person type is C for Client, if not 
                    // default to organization name
                    // ************************************************************************
                    if (AsChar(entities.CsePerson.Type1) == 'C')
                    {
                      local.Cp.Number = entities.CsePerson.Number;
                      UseSiReadCsePersonBatch2();

                      if (!IsEmpty(local.AbendData.AdabasResponseCd))
                      {
                        local.EabFileHandling.Action = "WRITE";
                        local.EabReportSend.RptDetail =
                          "Error getting CP   data for CSLN file.  Case: " + entities
                          .Case1.Number + " Person # " + entities
                          .CsePerson.Number + " SSN: " + local.Cp.Ssn;
                        UseCabErrorReport2();

                        if (!Equal(local.EabFileHandling.Status, "OK"))
                        {
                          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                          return;
                        }
                      }

                      local.CourtOrder.Update.Cp.FirstName = local.Cp.FirstName;
                      local.CourtOrder.Update.Cp.LastName = local.Cp.LastName;
                      local.CourtOrder.Update.Cp.MiddleInitial =
                        local.Cp.MiddleInitial;
                      local.CourtOrder.Update.Cp.Number = local.Cp.Number;
                    }
                    else
                    {
                      local.CourtOrder.Update.Cp.FirstName = "";
                      local.CourtOrder.Update.Cp.LastName = "";
                      local.CourtOrder.Update.Cp.MiddleInitial = "";
                      local.CourtOrder.Update.CpOrganization.Text33 =
                        entities.CsePerson.OrganizationName ?? Spaces(33);
                    }
                  }
                  else
                  {
                    // ok we didn't find the cp with an active child so lets 
                    // look without the edit for an active child.   11/21/13
                    if (ReadCaseCsePerson2())
                    {
                      ++local.CourtOrder.Index;
                      local.CourtOrder.CheckSize();

                      local.CourtOrder.Update.CourtOrderCaseNum.Text10 =
                        entities.Case1.Number;
                      local.Read.Number = entities.Case1.Number;

                      // ************************************************************************
                      //  check to see if person type is C for Client, if not 
                      // default to organization name
                      // ************************************************************************
                      if (AsChar(entities.CsePerson.Type1) == 'C')
                      {
                        local.Cp.Number = entities.CsePerson.Number;
                        UseSiReadCsePersonBatch2();

                        if (!IsEmpty(local.AbendData.AdabasResponseCd))
                        {
                          local.EabFileHandling.Action = "WRITE";
                          local.EabReportSend.RptDetail =
                            "Error getting CP   data for CSLN file.  Case: " + entities
                            .Case1.Number + " Person # " + entities
                            .CsePerson.Number + " SSN: " + local.Cp.Ssn;
                          UseCabErrorReport2();

                          if (!Equal(local.EabFileHandling.Status, "OK"))
                          {
                            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                            return;
                          }
                        }

                        local.CourtOrder.Update.Cp.FirstName =
                          local.Cp.FirstName;
                        local.CourtOrder.Update.Cp.LastName = local.Cp.LastName;
                        local.CourtOrder.Update.Cp.MiddleInitial =
                          local.Cp.MiddleInitial;
                        local.CourtOrder.Update.Cp.Number = local.Cp.Number;
                      }
                      else
                      {
                        local.CourtOrder.Update.Cp.FirstName = "";
                        local.CourtOrder.Update.Cp.LastName = "";
                        local.CourtOrder.Update.Cp.MiddleInitial = "";
                        local.CourtOrder.Update.CpOrganization.Text33 =
                          entities.CsePerson.OrganizationName ?? Spaces(33);
                      }
                    }
                    else
                    {
                      local.EabFileHandling.Action = "WRITE";
                      local.EabReportSend.RptDetail =
                        " CP err for CSLN file. nf in inactive Case: " + entities
                        .Case1.Number + " CP Person # " + entities
                        .CsePerson.Number + "child: " + local
                        .ChildPrevious.Number;
                      UseCabErrorReport2();

                      ++local.CourtOrder.Index;
                      local.CourtOrder.CheckSize();

                      local.CourtOrder.Update.Cp.FirstName = "";
                      local.CourtOrder.Update.Cp.LastName = "";
                      local.CourtOrder.Update.Cp.MiddleInitial = "";
                      local.CourtOrder.Update.CpOrganization.Text33 = "";
                    }
                  }
                }
              }
              else
              {
                // look for the CP with an active child
                if (ReadCaseCsePerson1())
                {
                  ++local.CourtOrder.Index;
                  local.CourtOrder.CheckSize();

                  local.CourtOrder.Update.CourtOrderCaseNum.Text10 =
                    entities.Case1.Number;
                  local.Read.Number = entities.Case1.Number;

                  // ************************************************************************
                  //  check to see if person type is C for Client, if not 
                  // default to organization name
                  // ************************************************************************
                  if (AsChar(entities.CsePerson.Type1) == 'C')
                  {
                    local.Cp.Number = entities.CsePerson.Number;
                    UseSiReadCsePersonBatch2();

                    if (!IsEmpty(local.AbendData.AdabasResponseCd))
                    {
                      local.EabFileHandling.Action = "WRITE";
                      local.EabReportSend.RptDetail =
                        "Error getting CP   data for CSLN file.  Case: " + entities
                        .Case1.Number + " Person # " + entities
                        .CsePerson.Number + " SSN: " + local.Cp.Ssn;
                      UseCabErrorReport2();

                      if (!Equal(local.EabFileHandling.Status, "OK"))
                      {
                        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                        return;
                      }
                    }

                    local.CourtOrder.Update.Cp.FirstName = local.Cp.FirstName;
                    local.CourtOrder.Update.Cp.LastName = local.Cp.LastName;
                    local.CourtOrder.Update.Cp.MiddleInitial =
                      local.Cp.MiddleInitial;
                    local.CourtOrder.Update.Cp.Number = local.Cp.Number;
                  }
                  else
                  {
                    local.CourtOrder.Update.Cp.FirstName = "";
                    local.CourtOrder.Update.Cp.LastName = "";
                    local.CourtOrder.Update.Cp.MiddleInitial = "";
                    local.CourtOrder.Update.CpOrganization.Text33 =
                      entities.CsePerson.OrganizationName ?? Spaces(33);
                  }
                }
                else
                {
                  // now look for the cp with an inactive child
                  if (ReadCaseCsePerson2())
                  {
                    ++local.CourtOrder.Index;
                    local.CourtOrder.CheckSize();

                    local.CourtOrder.Update.CourtOrderCaseNum.Text10 =
                      entities.Case1.Number;
                    local.Read.Number = entities.Case1.Number;

                    // ************************************************************************
                    //  check to see if person type is C for Client, if not 
                    // default to organization name
                    // ************************************************************************
                    if (AsChar(entities.CsePerson.Type1) == 'C')
                    {
                      local.Cp.Number = entities.CsePerson.Number;
                      UseSiReadCsePersonBatch2();

                      if (!IsEmpty(local.AbendData.AdabasResponseCd))
                      {
                        local.EabFileHandling.Action = "WRITE";
                        local.EabReportSend.RptDetail =
                          "Error getting CP   data for CSLN file.  Case: " + entities
                          .Case1.Number + " Person # " + entities
                          .CsePerson.Number + " SSN: " + local.Cp.Ssn;
                        UseCabErrorReport2();

                        if (!Equal(local.EabFileHandling.Status, "OK"))
                        {
                          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                          return;
                        }
                      }

                      local.CourtOrder.Update.Cp.FirstName = local.Cp.FirstName;
                      local.CourtOrder.Update.Cp.LastName = local.Cp.LastName;
                      local.CourtOrder.Update.Cp.MiddleInitial =
                        local.Cp.MiddleInitial;
                      local.CourtOrder.Update.Cp.Number = local.Cp.Number;
                    }
                    else
                    {
                      local.CourtOrder.Update.Cp.FirstName = "";
                      local.CourtOrder.Update.Cp.LastName = "";
                      local.CourtOrder.Update.Cp.MiddleInitial = "";
                      local.CourtOrder.Update.CpOrganization.Text33 =
                        entities.CsePerson.OrganizationName ?? Spaces(33);
                    }
                  }
                  else
                  {
                    local.EabFileHandling.Action = "WRITE";
                    local.EabReportSend.RptDetail =
                      " CP err for CSLN file. nf in inactive Case: " + entities
                      .Case1.Number + " CP Person # " + entities
                      .CsePerson.Number + "child: " + local
                      .ChildPrevious.Number;
                    UseCabErrorReport2();

                    ++local.CourtOrder.Index;
                    local.CourtOrder.CheckSize();

                    local.CourtOrder.Update.Cp.FirstName = "";
                    local.CourtOrder.Update.Cp.LastName = "";
                    local.CourtOrder.Update.Cp.MiddleInitial = "";
                    local.CourtOrder.Update.CpOrganization.Text33 = "";
                  }
                }
              }
            }
            else if (ReadCsePersonCase1())
            {
              ++local.CourtOrder.Index;
              local.CourtOrder.CheckSize();

              local.CourtOrder.Update.CourtOrderCaseNum.Text10 =
                entities.Case1.Number;
              local.Read.Number = entities.Case1.Number;

              // ************************************************************************
              //  check to see if person type is C for Client, if not default to
              // organization name
              // ************************************************************************
              if (AsChar(entities.CsePerson.Type1) == 'C')
              {
                local.Cp.Number = entities.CsePerson.Number;
                UseSiReadCsePersonBatch2();

                if (!IsEmpty(local.AbendData.AdabasResponseCd))
                {
                  local.EabFileHandling.Action = "WRITE";
                  local.EabReportSend.RptDetail =
                    "Error getting CP   data for CSLN file.  Case: " + entities
                    .Case1.Number + " Person # " + entities.CsePerson.Number + " SSN: " +
                    local.Cp.Ssn;
                  UseCabErrorReport2();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                    return;
                  }
                }

                local.CourtOrder.Update.Cp.FirstName = local.Cp.FirstName;
                local.CourtOrder.Update.Cp.LastName = local.Cp.LastName;
                local.CourtOrder.Update.Cp.MiddleInitial =
                  local.Cp.MiddleInitial;
                local.CourtOrder.Update.Cp.Number = local.Cp.Number;
              }
              else
              {
                local.CourtOrder.Update.Cp.FirstName = "";
                local.CourtOrder.Update.Cp.LastName = "";
                local.CourtOrder.Update.Cp.MiddleInitial = "";
                local.CourtOrder.Update.CpOrganization.Text33 =
                  entities.CsePerson.OrganizationName ?? Spaces(33);
              }
            }
            else
            {
              // now look for cp with inactive child
              if (ReadCaseCsePerson2())
              {
                ++local.CourtOrder.Index;
                local.CourtOrder.CheckSize();

                local.CourtOrder.Update.CourtOrderCaseNum.Text10 =
                  entities.Case1.Number;
                local.Read.Number = entities.Case1.Number;

                // ************************************************************************
                //  check to see if person type is C for Client, if not default 
                // to organization name
                // ************************************************************************
                if (AsChar(entities.CsePerson.Type1) == 'C')
                {
                  local.Cp.Number = entities.CsePerson.Number;
                  UseSiReadCsePersonBatch2();

                  if (!IsEmpty(local.AbendData.AdabasResponseCd))
                  {
                    local.EabFileHandling.Action = "WRITE";
                    local.EabReportSend.RptDetail =
                      "Error getting CP   data for CSLN file.  Case: " + entities
                      .Case1.Number + " Person # " + entities
                      .CsePerson.Number + " SSN: " + local.Cp.Ssn;
                    UseCabErrorReport2();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                      return;
                    }
                  }

                  local.CourtOrder.Update.Cp.FirstName = local.Cp.FirstName;
                  local.CourtOrder.Update.Cp.LastName = local.Cp.LastName;
                  local.CourtOrder.Update.Cp.MiddleInitial =
                    local.Cp.MiddleInitial;
                  local.CourtOrder.Update.Cp.Number = local.Cp.Number;
                }
                else
                {
                  local.CourtOrder.Update.Cp.FirstName = "";
                  local.CourtOrder.Update.Cp.LastName = "";
                  local.CourtOrder.Update.Cp.MiddleInitial = "";
                  local.CourtOrder.Update.CpOrganization.Text33 =
                    entities.CsePerson.OrganizationName ?? Spaces(33);
                }
              }
              else
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  " CP err for CSLN file. nf in inactive Case: " + entities
                  .Case1.Number + " CP Person # " + entities
                  .CsePerson.Number + "child: " + local.ChildPrevious.Number;
                UseCabErrorReport2();

                ++local.CourtOrder.Index;
                local.CourtOrder.CheckSize();

                local.CourtOrder.Update.Cp.FirstName = "";
                local.CourtOrder.Update.Cp.LastName = "";
                local.CourtOrder.Update.Cp.MiddleInitial = "";
                local.CourtOrder.Update.CpOrganization.Text33 = "";
              }
            }

            // ******************************************************
            //  Find Private Attorney for NCP and this court case number
            // ******************************************************
            if (ReadPersonPrivateAttorney2())
            {
              local.NcpAttyFirstName.Text12 =
                entities.PersonPrivateAttorney.FirstName ?? Spaces(12);
              local.NcpAttyLastName.Text17 =
                entities.PersonPrivateAttorney.LastName ?? Spaces(17);
              local.NcpAttyMiddleInitial.Text1 =
                entities.PersonPrivateAttorney.MiddleInitial ?? Spaces(1);
              local.NcpAttorney.Identifier =
                entities.PersonPrivateAttorney.Identifier;
              local.PrivateAttorneyAddress.Street1 = "";
              local.PrivateAttorneyAddress.Street2 = "";
              local.PrivateAttorneyAddress.ZipCode4 = "";
              local.PrivateAttorneyAddress.ZipCode5 = "";
              local.PrivateAttorneyAddress.City = "";
              local.PrivateAttorneyAddress.State = "";

              if (ReadPrivateAttorneyAddress())
              {
                local.PrivateAttorneyAddress.Street1 =
                  entities.PrivateAttorneyAddress.Street1;
                local.PrivateAttorneyAddress.Street2 =
                  entities.PrivateAttorneyAddress.Street2;
                local.PrivateAttorneyAddress.ZipCode4 =
                  entities.PrivateAttorneyAddress.ZipCode4;
                local.PrivateAttorneyAddress.ZipCode5 =
                  entities.PrivateAttorneyAddress.ZipCode5;
                local.PrivateAttorneyAddress.City =
                  entities.PrivateAttorneyAddress.City;
                local.PrivateAttorneyAddress.State =
                  entities.PrivateAttorneyAddress.State;
              }
            }
            else
            {
              local.NcpAttyFirstName.Text12 = "";
              local.NcpAttyLastName.Text17 = "";
              local.NcpAttyMiddleInitial.Text1 = "";
              local.PrivateAttorneyAddress.Street1 = "";
              local.PrivateAttorneyAddress.Street2 = "";
              local.PrivateAttorneyAddress.ZipCode4 = "";
              local.PrivateAttorneyAddress.ZipCode5 = "";
              local.PrivateAttorneyAddress.City = "";
              local.PrivateAttorneyAddress.State = "";
            }

            local.ChildPrevious.Number = "";

            foreach(var item1 in ReadCsePerson4())
            {
              if (Equal(entities.ChCsePerson.Number, local.Cp.Number))
              {
                continue;
              }

              if (!Equal(entities.ChCsePerson.Number, local.ChildPrevious.Number))
                
              {
                // ******************************
                // now set up the child record
                // ******************************
                if (IsEmpty(local.ChildPrevious.Number))
                {
                  local.CourtOrder.Item.ChildByCourtOrder.Index = -1;
                  local.CourtOrder.Item.ChildByCourtOrder.Count = 0;
                }

                ++local.CourtOrder.Item.ChildByCourtOrder.Index;
                local.CourtOrder.Item.ChildByCourtOrder.CheckSize();

                local.CourtOrder.Update.ChildByCourtOrder.Update.Child.Number =
                  entities.ChCsePerson.Number;
                UseSiReadCsePersonBatch3();

                if (!IsEmpty(local.AbendData.AdabasResponseCd))
                {
                  local.EabFileHandling.Action = "WRITE";
                  local.EabReportSend.RptDetail =
                    "Error getting child   data for CSLN file.  Case: " + entities
                    .Case1.Number + " Person # " + entities.CsePerson.Number + " SSN: " +
                    local.CourtOrder.Item.ChildByCourtOrder.Item.Child.Ssn;
                  UseCabErrorReport2();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                    return;
                  }
                }

                // *** ADDED SETS HERE FOR LOCAL GROUP CHILD ATTY NAME ETC
                local.CourtOrder.Update.ChildByCourtOrder.Update.
                  ChildNcpAttyFrstNm.Text12 = local.NcpAttyFirstName.Text12;
                local.CourtOrder.Update.ChildByCourtOrder.Update.
                  ChildNcpAttyLstNm.Text17 = local.NcpAttyLastName.Text17;
                local.CourtOrder.Update.ChildByCourtOrder.Update.ChildNcpAttyMi.
                  Text1 = local.NcpAttyMiddleInitial.Text1;
                local.CourtOrder.Update.ChildByCourtOrder.Update.ChildNcpAtty.
                  Street1 = local.PrivateAttorneyAddress.Street1 ?? "";
                local.CourtOrder.Update.ChildByCourtOrder.Update.ChildNcpAtty.
                  Street2 = local.PrivateAttorneyAddress.Street2 ?? "";
                local.CourtOrder.Update.ChildByCourtOrder.Update.ChildNcpAtty.
                  City = local.PrivateAttorneyAddress.City ?? "";
                local.CourtOrder.Update.ChildByCourtOrder.Update.ChildNcpAtty.
                  State = local.PrivateAttorneyAddress.State ?? "";
                local.CourtOrder.Update.ChildByCourtOrder.Update.ChildNcpAtty.
                  ZipCode5 = local.PrivateAttorneyAddress.ZipCode5 ?? "";
                local.CourtOrder.Update.ChildByCourtOrder.Update.ChildNcpAtty.
                  ZipCode4 = local.PrivateAttorneyAddress.ZipCode4 ?? "";
                local.ChildPrevious.Number = entities.ChCsePerson.Number;
                local.CourtOrder.Update.ChildByCourtOrder.Update.
                  ChildStandardNum.Text20 = local.Previous.StandardNumber ?? Spaces
                  (20);
              }
            }

            if (IsEmpty(local.ChildPrevious.Number))
            {
              // *****************************************************************
              //  create error for when no children are found for this court 
              // order
              // ******************************************************************
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Court order without children, NCP: " + local
                .AdministrativeActCertification.CaseNumber + " Court order: " +
                (local.Previous.StandardNumber ?? "");
              UseCabErrorReport2();

              goto Test1;
            }

            local.CourtOrder1.ArrearsAmountOwed = 0;
            local.CourtOrder1.CurrentAmountOwed = 0;

            for(local.IwoGroupExtract.Index = 0; local.IwoGroupExtract.Index < local
              .IwoGroupExtract.Count; ++local.IwoGroupExtract.Index)
            {
              if (!local.IwoGroupExtract.CheckSize())
              {
                break;
              }

              if (Equal(local.Previous.StandardNumber,
                local.IwoGroupExtract.Item.CourtCase.StandardNumber))
              {
                if (local.IwoGroupExtract.Item.IwoAmounts.ArrearsAmountOwed > 0)
                {
                  local.CourtOrder1.ArrearsAmountOwed += local.IwoGroupExtract.
                    Item.IwoAmounts.ArrearsAmountOwed;
                }

                if (local.IwoGroupExtract.Item.IwoAmounts.CurrentAmountOwed > 0)
                {
                  local.CourtOrder1.CurrentAmountOwed += local.IwoGroupExtract.
                    Item.IwoAmounts.CurrentAmountOwed;
                }
              }
            }

            local.IwoGroupExtract.CheckIndex();

            if (local.CourtOrder1.ArrearsAmountOwed > 0)
            {
              local.CourtOrder.Update.ArrearsSupWithhdAmt.Text8 =
                NumberToString((long)(local.CourtOrder1.ArrearsAmountOwed * 100),
                8, 8);
            }
            else
            {
              local.CourtOrder.Update.ArrearsSupWithhdAmt.Text8 = "";
            }

            if (local.CourtOrder1.CurrentAmountOwed > 0)
            {
              local.CourtOrder.Update.CurrSupWithholdAmt.Text8 =
                NumberToString((long)(local.CourtOrder1.CurrentAmountOwed * 100),
                8, 8);
            }
            else
            {
              local.CourtOrder.Update.CurrSupWithholdAmt.Text8 = "";
            }

            local.NcpTotal.ArrearsAmountOwed += local.ScreenOwedAmounts.
              ArrearsAmountOwed;
            ++local.CourtOrderCount.Count;
            local.CourtOrder.Update.StandardCourtOrder.Text20 =
              local.Previous.StandardNumber ?? Spaces(20);
            local.CourtOrder.Update.CourtOrderCountyNme.Text20 =
              entities.Fips.CountyDescription ?? Spaces(20);
            local.CourtOrder.Update.CourtOrderState.Text2 =
              entities.Fips.StateAbbreviation;
            local.CourtOrder.Update.PrepAttyAddr1.Text60 =
              local.PrepByAttyAddress1.Text60;
            local.CourtOrder.Update.PrepAttyAddr2.Text25 =
              local.PrepByAttyAddress2.Text25;
            local.CourtOrder.Update.PrepAttyCertificate.Text10 =
              local.PrepByAttyCertificate.Text10;
            local.CourtOrder.Update.PrepAttyCity.Text15 =
              local.PrepByAttyCity.Text15;
            local.CourtOrder.Update.PrepAttyEmail.Text50 =
              local.PrepByAttyEmail.Text50;
            local.CourtOrder.Update.PrepAttyFax.Text10 =
              local.PrepByAttyFax.Text10;
            local.CourtOrder.Update.PrepAttyFirstName.Text12 =
              local.PrepByAttyFirstName.Text12;
            local.CourtOrder.Update.PrepAttyLastName.Text17 =
              local.PrepByAttyLastName.Text17;
            local.CourtOrder.Update.PrepAttyMi.Text1 =
              local.PrepByAttyMiddleInitia.Text1;
            local.CourtOrder.Update.PrepAttyPhone.Text10 =
              local.PrepByAttyPhone.Text10;
            local.CourtOrder.Update.PrepAttyState.Text2 =
              local.PrepByAttyState.Text2;
            local.CourtOrder.Update.PrepAttyZip.Text9 =
              local.PrepByAttyZip.Text9;
            local.CourtOrder.Update.ArrearsAsOfDate.Date =
              local.ProcessingDate.Date;
            local.CourtOrder.Update.ArrearsByStandNumb.Text12 =
              NumberToString((long)(local.ScreenOwedAmounts.ArrearsAmountOwed *
              100), 4, 12);
            local.Read.Number = "";

            foreach(var item1 in ReadCourtCaption())
            {
              switch(entities.CourtCaption.Number)
              {
                case 1:
                  local.CourtOrder.Update.LegalCaptionLine1.Text40 =
                    entities.CourtCaption.Line ?? Spaces(40);

                  break;
                case 2:
                  local.CourtOrder.Update.LegalCaptionLine2.Text40 =
                    entities.CourtCaption.Line ?? Spaces(40);

                  break;
                case 3:
                  local.CourtOrder.Update.LegalCaptionLine3.Text40 =
                    entities.CourtCaption.Line ?? Spaces(40);

                  break;
                case 4:
                  local.CourtOrder.Update.LegalCaptionLine4.Text40 =
                    entities.CourtCaption.Line ?? Spaces(40);

                  break;
                case 5:
                  local.CourtOrder.Update.LegalCaptionLine5.Text40 =
                    entities.CourtCaption.Line ?? Spaces(40);

                  break;
                case 6:
                  local.CourtOrder.Update.LegalCaptionLine6.Text40 =
                    entities.CourtCaption.Line ?? Spaces(40);

                  break;
                case 7:
                  local.CourtOrder.Update.LegalCaptionLine7.Text40 =
                    entities.CourtCaption.Line ?? Spaces(40);

                  break;
                case 8:
                  local.CourtOrder.Update.LegalCaptionLine8.Text40 =
                    entities.CourtCaption.Line ?? Spaces(40);

                  break;
                case 9:
                  local.CourtOrder.Update.LegalCaptionLine9.Text40 =
                    entities.CourtCaption.Line ?? Spaces(40);

                  break;
                case 10:
                  local.CourtOrder.Update.LegalCaptionLine10.Text40 =
                    entities.CourtCaption.Line ?? Spaces(40);

                  break;
                case 11:
                  local.CourtOrder.Update.LegalCaptionLine11.Text40 =
                    entities.CourtCaption.Line ?? Spaces(40);

                  break;
                case 12:
                  local.CourtOrder.Update.LegalCaptionLine12.Text40 =
                    entities.CourtCaption.Line ?? Spaces(40);

                  break;
                case 13:
                  local.CourtOrder.Update.LegalCaptionLine13.Text40 =
                    entities.CourtCaption.Line ?? Spaces(40);

                  break;
                default:
                  break;
              }
            }
          }

Test1:

          local.ScreenOwedAmounts.ArrearsAmountOwed = 0;
          local.ScreenOwedAmounts.CurrentAmountOwed = 0;
          local.StandardCourtOrderNumbe.Text20 = "";

          // *****************************************************************
          // NCP changed so we are going to write out the previous NCP.
          // first record is the NCP 01 record type
          // *****************************************************************
          local.PassArea.FileInstruction = "WRITE";
          local.RecordType.Text2 = "01";
          local.NcpRecordArrsBalance.Text14 =
            NumberToString((long)(local.NcpTotal.ArrearsAmountOwed * 100), 2, 14);
            

          if (local.NcpTotal.ArrearsAmountOwed > 499 && local
            .CourtOrderCount.Count > 0)
          {
            // ****************************************************
            //   Write 01 NCP  record
            // ****************************************************
            UseOeEabWriteCslnFile2();

            if (!Equal(local.PassArea.TextReturnCode, "OK"))
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Error in writing header record in external file  for 'oe_eab_write_csln_file'.";
                
              UseCabErrorReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }

              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ++local.NcpCount.Count;

            // ****************************************************
            // now we will do the court orders  type 10 for the previous NCP
            // ****************************************************
            local.CourtOrder.Index = 0;

            for(var limit = local.CourtOrder.Count; local.CourtOrder.Index < limit
              ; ++local.CourtOrder.Index)
            {
              if (!local.CourtOrder.CheckSize())
              {
                break;
              }

              // ****************************************************
              //   Write 10 court order  record
              // ****************************************************
              local.PassArea.FileInstruction = "WRITE";
              local.RecordType.Text2 = "10";
              UseOeEabWriteCslnFile5();

              if (!Equal(local.PassArea.TextReturnCode, "OK"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Error in writing court order record in external file  for 'oe_eab_write_csln_file'.";
                  
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                  return;
                }

                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }
              else
              {
                // changed to .01 to get correct amt on hist record
                local.FinanceWorkAttributes.NumericalDollarValue =
                  StringToNumber(local.CourtOrder.Item.ArrearsByStandNumb.Text12)
                  * 0.01M;
                UseFnCabReturnTextDollars();

                // *********************************************
                //  write one history record per court order
                // *********************************************
                local.Infrastructure.SituationNumber = 0;
                local.Infrastructure.ReasonCode = "CSLNSUBMISSION";
                local.Infrastructure.EventId = 1;
                local.Infrastructure.EventType = "ADMINACT";
                local.Infrastructure.ProcessStatus = "Q";
                local.Infrastructure.UserId = "SWEEB431";
                local.Infrastructure.BusinessObjectCd = "ENF";
                local.Infrastructure.ReferenceDate = local.ProcessingDate.Date;
                local.Infrastructure.CreatedBy =
                  entities.ProgramProcessingInfo.Name;
                local.Infrastructure.EventDetailName =
                  "SUBMITTED TO CHILD SUPPORT LIEN NETWORK";
                local.Infrastructure.CsePersonNumber =
                  entities.NcpCsePerson.Number;
                local.Infrastructure.CaseNumber =
                  local.CourtOrder.Item.CourtOrderCaseNum.Text10;
                local.Infrastructure.DenormText12 =
                  entities.LegalAction.StandardNumber;
                local.Infrastructure.Detail =
                  "CSLN submission for Ct Order: " + TrimEnd
                  (local.CourtOrder.Item.StandardCourtOrder.Text20) + " arrears: $" +
                  local.FinanceWorkAttributes.TextDollarValue;
                UseSpCabCreateInfrastructure();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                }
              }

              local.CourtOrder.Item.ChildByCourtOrder.Index = 0;

              for(var limit1 = local.CourtOrder.Item.ChildByCourtOrder.Count; local
                .CourtOrder.Item.ChildByCourtOrder.Index < limit1; ++
                local.CourtOrder.Item.ChildByCourtOrder.Index)
              {
                if (!local.CourtOrder.Item.ChildByCourtOrder.CheckSize())
                {
                  break;
                }

                // **********************************************
                // write 20  CHILD  record for the previous NCP
                // **********************************************
                local.PassArea.FileInstruction = "WRITE";
                local.RecordType.Text2 = "20";

                if (Equal(local.CourtOrder.Item.ChildByCourtOrder.Item.
                  ChildStandardNum.Text20,
                  local.CourtOrder.Item.StandardCourtOrder.Text20))
                {
                  UseOeEabWriteCslnFile7();

                  if (!Equal(local.PassArea.TextReturnCode, "OK"))
                  {
                    local.EabFileHandling.Action = "WRITE";
                    local.EabReportSend.RptDetail =
                      "Error in writing child record in external file  for 'oe_eab_write_csln_file'.";
                      
                    UseCabErrorReport2();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                      return;
                    }

                    ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                    return;
                  }
                }
              }

              local.CourtOrder.Item.ChildByCourtOrder.CheckIndex();
            }

            local.CourtOrder.CheckIndex();

            // ******************************************************
            //   now we will do the end record (89)  for the previous NCP
            // ******************************************************
            local.PassArea.FileInstruction = "WRITE";
            local.RecordType.Text2 = "89";
            UseOeEabWriteCslnFile6();

            if (!Equal(local.PassArea.TextReturnCode, "OK"))
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail = local.NcpRecordName.Number + "Error in writing end ncp record in external file  for oe_eab_write_csln_file.";
                
              UseCabErrorReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }

              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }
          }

          local.NcpTotal.ArrearsAmountOwed = 0;
          local.NcpRecordName.Dob = new DateTime(1, 1, 1);
          local.NcpRecordName.FirstName = "";
          local.NcpRecordName.LastName = "";
          local.NcpRecordName.MiddleInitial = "";
          local.NcpRecordName.Number = "";
          local.NcpRecordName.Ssn = "";
          local.NcpCsePersonAddress.Street1 = "";
          local.NcpCsePersonAddress.Street2 = "";
          local.NcpCsePersonAddress.City = "";
          local.NcpCsePersonAddress.State = "";
          local.NcpCsePersonAddress.ZipCode = "";
          local.NcpAddressVerified.Flag = "";
          local.NcpRecordArrsBalance.Text14 = "";
          local.CourtOrderCount.Count = 0;
          local.NcpAttyFirstName.Text12 = "";
          local.NcpAttyLastName.Text17 = "";
          local.NcpAttyMiddleInitial.Text1 = "";
          local.PrivateAttorneyAddress.Street1 = "";
          local.PrivateAttorneyAddress.Street2 = "";
          local.PrivateAttorneyAddress.ZipCode4 = "";
          local.PrivateAttorneyAddress.ZipCode5 = "";
          local.PrivateAttorneyAddress.City = "";
          local.PrivateAttorneyAddress.State = "";

          for(local.CourtOrder.Index = 0; local.CourtOrder.Index < local
            .CourtOrder.Count; ++local.CourtOrder.Index)
          {
            if (!local.CourtOrder.CheckSize())
            {
              break;
            }

            local.CourtOrder.Update.CpOrganization.Text33 = "";
            local.CourtOrder.Update.ArrearsAsOfDate.Date =
              new DateTime(1, 1, 1);
            local.CourtOrder.Update.ArrearsByStandNumb.Text12 = "";
            local.CourtOrder.Update.ArrearsSupWithhdAmt.Text8 = "";
            local.CourtOrder.Update.ArrearsSupWithhdAmt.Text8 = "";
            local.CourtOrder.Update.CourtOrderCaseNum.Text10 = "";
            local.CourtOrder.Update.CourtOrderCity.Text15 = "";
            local.CourtOrder.Update.CourtOrderCountyNme.Text20 = "";
            local.CourtOrder.Update.CourtOrderState.Text2 = "";
            local.CourtOrder.Update.Cp.FirstName = "";
            local.CourtOrder.Update.Cp.LastName = "";
            local.CourtOrder.Update.Cp.MiddleInitial = "";
            local.CourtOrder.Update.CurrSupWithholdAmt.Text8 = "";
            local.CourtOrder.Update.LegalCaptionLine1.Text40 = "";
            local.CourtOrder.Update.LegalCaptionLine10.Text40 = "";
            local.CourtOrder.Update.LegalCaptionLine11.Text40 = "";
            local.CourtOrder.Update.LegalCaptionLine12.Text40 = "";
            local.CourtOrder.Update.LegalCaptionLine13.Text40 = "";
            local.CourtOrder.Update.LegalCaptionLine2.Text40 = "";
            local.CourtOrder.Update.LegalCaptionLine3.Text40 = "";
            local.CourtOrder.Update.LegalCaptionLine4.Text40 = "";
            local.CourtOrder.Update.LegalCaptionLine5.Text40 = "";
            local.CourtOrder.Update.LegalCaptionLine6.Text40 = "";
            local.CourtOrder.Update.LegalCaptionLine7.Text40 = "";
            local.CourtOrder.Update.LegalCaptionLine8.Text40 = "";
            local.CourtOrder.Update.LegalCaptionLine9.Text40 = "";
            local.CourtOrder.Update.PrepAttyAddr1.Text60 = "";
            local.CourtOrder.Update.PrepAttyAddr2.Text25 = "";
            local.CourtOrder.Update.PrepAttyCertificate.Text10 = "";
            local.CourtOrder.Update.PrepAttyCity.Text15 = "";
            local.CourtOrder.Update.PrepAttyEmail.Text50 = "";
            local.CourtOrder.Update.PrepAttyFax.Text10 = "";
            local.CourtOrder.Update.PrepAttyFirstName.Text12 = "";
            local.CourtOrder.Update.PrepAttyLastName.Text17 = "";
            local.CourtOrder.Update.PrepAttyMi.Text1 = "";
            local.CourtOrder.Update.PrepAttyPhone.Text10 = "";
            local.CourtOrder.Update.PrepAttyPhone.Text10 = "";
            local.CourtOrder.Update.PrepAttyState.Text2 = "";
            local.CourtOrder.Update.StandardCourtOrder.Text20 = "";

            for(local.CourtOrder.Item.ChildByCourtOrder.Index = 0; local
              .CourtOrder.Item.ChildByCourtOrder.Index < local
              .CourtOrder.Item.ChildByCourtOrder.Count; ++
              local.CourtOrder.Item.ChildByCourtOrder.Index)
            {
              if (!local.CourtOrder.Item.ChildByCourtOrder.CheckSize())
              {
                break;
              }

              local.CourtOrder.Update.ChildByCourtOrder.Update.Child.Dob =
                new DateTime(1, 1, 1);
              local.CourtOrder.Update.ChildByCourtOrder.Update.Child.FirstName =
                "";
              local.CourtOrder.Update.ChildByCourtOrder.Update.Child.LastName =
                "";
              local.CourtOrder.Update.ChildByCourtOrder.Update.Child.
                MiddleInitial = "";
              local.CourtOrder.Update.ChildByCourtOrder.Update.ChildStandardNum.
                Text20 = "";
              local.CourtOrder.Update.ChildByCourtOrder.Update.
                ChildNcpAttyFrstNm.Text12 = "";
              local.CourtOrder.Update.ChildByCourtOrder.Update.
                ChildNcpAttyLstNm.Text17 = "";
              local.CourtOrder.Update.ChildByCourtOrder.Update.ChildNcpAttyMi.
                Text1 = "";
              local.CourtOrder.Update.ChildByCourtOrder.Update.ChildNcpAtty.
                Street1 = "";
              local.CourtOrder.Update.ChildByCourtOrder.Update.ChildNcpAtty.
                Street2 = "";
              local.CourtOrder.Update.ChildByCourtOrder.Update.ChildNcpAtty.
                City = "";
              local.CourtOrder.Update.ChildByCourtOrder.Update.ChildNcpAtty.
                State = "";
              local.CourtOrder.Update.ChildByCourtOrder.Update.ChildNcpAtty.
                ZipCode5 = "";
              local.CourtOrder.Update.ChildByCourtOrder.Update.ChildNcpAtty.
                ZipCode4 = "";
            }

            local.CourtOrder.Item.ChildByCourtOrder.CheckIndex();
            local.CourtOrder.Item.ChildByCourtOrder.Count = 0;
            local.CourtOrder.Item.ChildByCourtOrder.Index = -1;
          }

          local.CourtOrder.CheckIndex();
          local.CourtOrder.Count = 0;
          local.CourtOrder.Index = -1;
        }

        local.AdministrativeActCertification.CaseNumber =
          entities.AdministrativeActCertification.CaseNumber;
        local.NcpRecordName.Number =
          entities.AdministrativeActCertification.CaseNumber;

        // *************
        // get NCP data
        // *************
        UseSiReadCsePersonBatch1();

        if (!IsEmpty(local.AbendData.AdabasResponseCd))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error getting NCP   data for CSLN file.  Case: " + entities
            .Case1.Number + " Person # " + entities.CsePerson.Number + " SSN: " +
            local.NcpRecordName.Ssn;
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }

        UseSiGetCsePersonMailingAddr();

        if (!ReadCsePerson3())
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "CSE Person not found, NCP Person# " + local
            .NcpRecordName.Number + "Last Name:" + local
            .NcpRecordName.LastName;
          UseCabErrorReport2();
        }

        // eliminating obligation types 9 bad check, 4 iv-d recovery, 5 irs 
        // negative recovery, 6 ar misdirected payment,
        //  7 ap misdirected payment, 8 non-case person misdirected pmt, 15, 
        // genetic fee test
        foreach(var item1 in ReadObligationLegalActionObligationType())
        {
          if (AsChar(entities.Obligation.PrimarySecondaryCode) == 'S')
          {
            goto ReadEach;
          }

          if (!Equal(entities.LegalAction.StandardNumber,
            local.StandardCourtOrderNumbe.Text20) && !
            IsEmpty(local.StandardCourtOrderNumbe.Text20))
          {
            if (local.ScreenOwedAmounts.ArrearsAmountOwed == 0)
            {
              goto Test3;
            }

            if (!ReadTribunalFips())
            {
              continue;
            }

            local.StandardCourtOrderNumbe.Text20 =
              local.Previous.StandardNumber ?? Spaces(20);
            local.LegalAction.CourtCaseNumber =
              entities.LegalAction.CourtCaseNumber;
            local.IwoCabErrorMessages.Update.ErrorType.Text50 = "";
            local.IwoCabErrorMessages.Update.ErrorCourtCase.StandardNumber = "";
            UseCabGetIwoAmtsForCslnFile();

            // edited read below to remove line about ch case role end date 
            // greater or equal to local ppi date  AH 3/21/17
            local.ChildPrevious.Number = "";

            if (ReadCsePerson2())
            {
              local.ChildPrevious.Number = entities.ChCsePerson.Number;
            }

            if (IsEmpty(local.ChildPrevious.Number))
            {
              // *****************************************************************
              //  create error for when no children are found for this court 
              // order
              // ******************************************************************
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Court order without children, NCP: " + local
                .AdministrativeActCertification.CaseNumber + " Court order: " +
                (local.Previous.StandardNumber ?? "");
              UseCabErrorReport2();
            }
            else
            {
              // copy starts here ***
              // ****************
              //  2ND Get CP data
              // ****************
              if (local.ScreenOwedAmounts.CurrentAmountOwed > 0 || local
                .ScreenOwedAmounts.ArrearsAmountOwed > 0)
              {
                local.IwoFound.Flag = "";
                local.Iwo.CreatedTstamp = local.Null1.Timestamp;

                foreach(var item2 in ReadLegalAction())
                {
                  local.Iwo.CreatedTstamp = entities.LegalAction.CreatedTstamp;

                  if (Equal(entities.LegalAction.ActionTaken, "IWOTERM") || Equal
                    (entities.LegalAction.ActionTaken, "IWONOTKT"))
                  {
                    local.IwoFound.Flag = "N";

                    break;
                  }

                  if (Equal(entities.LegalAction.ActionTaken, "IWOMODO") || Equal
                    (entities.LegalAction.ActionTaken, "IWONOTKM") || Equal
                    (entities.LegalAction.ActionTaken, "IWONOTKS") || Equal
                    (entities.LegalAction.ActionTaken, "ORDIWO2") || Equal
                    (entities.LegalAction.ActionTaken, "NOIIWON") || Equal
                    (entities.LegalAction.ActionTaken, "IWO"))
                  {
                    local.IwoFound.Flag = "Y";

                    break;
                  }
                  else
                  {
                    local.IwoFound.Flag = "N";

                    break;
                  }
                }

                if (AsChar(local.IwoFound.Flag) == 'Y')
                {
                  if (ReadCsePersonCase2())
                  {
                    ++local.CourtOrder.Index;
                    local.CourtOrder.CheckSize();

                    local.CourtOrder.Update.CourtOrderCaseNum.Text10 =
                      entities.Case1.Number;
                    local.Read.Number = entities.Case1.Number;

                    // ************************************************************************
                    //  check to see if person type is C for Client, if not 
                    // default to organization name
                    // ************************************************************************
                    if (AsChar(entities.CsePerson.Type1) == 'C')
                    {
                      local.Cp.Number = entities.CsePerson.Number;
                      UseSiReadCsePersonBatch2();

                      if (!IsEmpty(local.AbendData.AdabasResponseCd))
                      {
                        local.EabFileHandling.Action = "WRITE";
                        local.EabReportSend.RptDetail =
                          "Error getting CP   data for CSLN file.  Case: " + entities
                          .Case1.Number + " Person # " + entities
                          .CsePerson.Number + " SSN: " + local.Cp.Ssn;
                        UseCabErrorReport2();

                        if (!Equal(local.EabFileHandling.Status, "OK"))
                        {
                          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                          return;
                        }
                      }

                      local.CourtOrder.Update.Cp.FirstName = local.Cp.FirstName;
                      local.CourtOrder.Update.Cp.LastName = local.Cp.LastName;
                      local.CourtOrder.Update.Cp.MiddleInitial =
                        local.Cp.MiddleInitial;
                      local.CourtOrder.Update.Cp.Number = local.Cp.Number;
                    }
                    else
                    {
                      local.CourtOrder.Update.Cp.FirstName = "";
                      local.CourtOrder.Update.Cp.LastName = "";
                      local.CourtOrder.Update.Cp.MiddleInitial = "";
                      local.CourtOrder.Update.CpOrganization.Text33 =
                        entities.CsePerson.OrganizationName ?? Spaces(33);
                    }
                  }
                  else
                  {
                    // first look for cp with active child
                    if (ReadCaseCsePerson1())
                    {
                      ++local.CourtOrder.Index;
                      local.CourtOrder.CheckSize();

                      local.CourtOrder.Update.CourtOrderCaseNum.Text10 =
                        entities.Case1.Number;
                      local.Read.Number = entities.Case1.Number;

                      // ************************************************************************
                      //  check to see if person type is C for Client, if not 
                      // default to organization name
                      // ************************************************************************
                      if (AsChar(entities.CsePerson.Type1) == 'C')
                      {
                        local.Cp.Number = entities.CsePerson.Number;
                        UseSiReadCsePersonBatch2();

                        if (!IsEmpty(local.AbendData.AdabasResponseCd))
                        {
                          local.EabFileHandling.Action = "WRITE";
                          local.EabReportSend.RptDetail =
                            "Error getting CP   data for CSLN file.  Case: " + entities
                            .Case1.Number + " Person # " + entities
                            .CsePerson.Number + " SSN: " + local.Cp.Ssn;
                          UseCabErrorReport2();

                          if (!Equal(local.EabFileHandling.Status, "OK"))
                          {
                            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                            return;
                          }
                        }

                        local.CourtOrder.Update.Cp.FirstName =
                          local.Cp.FirstName;
                        local.CourtOrder.Update.Cp.LastName = local.Cp.LastName;
                        local.CourtOrder.Update.Cp.MiddleInitial =
                          local.Cp.MiddleInitial;
                        local.CourtOrder.Update.Cp.Number = local.Cp.Number;
                      }
                      else
                      {
                        local.CourtOrder.Update.Cp.FirstName = "";
                        local.CourtOrder.Update.Cp.LastName = "";
                        local.CourtOrder.Update.Cp.MiddleInitial = "";
                        local.CourtOrder.Update.CpOrganization.Text33 =
                          entities.CsePerson.OrganizationName ?? Spaces(33);
                      }
                    }
                    else
                    {
                      // ok we didn't find the cp with an active child so lets 
                      // look without the edit for an active child.   11/21/13
                      if (ReadCaseCsePerson2())
                      {
                        ++local.CourtOrder.Index;
                        local.CourtOrder.CheckSize();

                        local.CourtOrder.Update.CourtOrderCaseNum.Text10 =
                          entities.Case1.Number;
                        local.Read.Number = entities.Case1.Number;

                        // ************************************************************************
                        //  check to see if person type is C for Client, if not 
                        // default to organization name
                        // ************************************************************************
                        if (AsChar(entities.CsePerson.Type1) == 'C')
                        {
                          local.Cp.Number = entities.CsePerson.Number;
                          UseSiReadCsePersonBatch2();

                          if (!IsEmpty(local.AbendData.AdabasResponseCd))
                          {
                            local.EabFileHandling.Action = "WRITE";
                            local.EabReportSend.RptDetail =
                              "Error getting CP   data for CSLN file.  Case: " +
                              entities.Case1.Number + " Person # " + entities
                              .CsePerson.Number + " SSN: " + local.Cp.Ssn;
                            UseCabErrorReport2();

                            if (!Equal(local.EabFileHandling.Status, "OK"))
                            {
                              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                              return;
                            }
                          }

                          local.CourtOrder.Update.Cp.FirstName =
                            local.Cp.FirstName;
                          local.CourtOrder.Update.Cp.LastName =
                            local.Cp.LastName;
                          local.CourtOrder.Update.Cp.MiddleInitial =
                            local.Cp.MiddleInitial;
                          local.CourtOrder.Update.Cp.Number = local.Cp.Number;
                        }
                        else
                        {
                          local.CourtOrder.Update.Cp.FirstName = "";
                          local.CourtOrder.Update.Cp.LastName = "";
                          local.CourtOrder.Update.Cp.MiddleInitial = "";
                          local.CourtOrder.Update.CpOrganization.Text33 =
                            entities.CsePerson.OrganizationName ?? Spaces(33);
                        }
                      }
                      else
                      {
                        local.EabFileHandling.Action = "WRITE";
                        local.EabReportSend.RptDetail =
                          " CP err for CSLN file. nf in inactive Case: " + entities
                          .Case1.Number + " CP Person # " + entities
                          .CsePerson.Number + "child: " + local
                          .ChildPrevious.Number;
                        UseCabErrorReport2();

                        ++local.CourtOrder.Index;
                        local.CourtOrder.CheckSize();

                        local.CourtOrder.Update.Cp.FirstName = "";
                        local.CourtOrder.Update.Cp.LastName = "";
                        local.CourtOrder.Update.Cp.MiddleInitial = "";
                        local.CourtOrder.Update.CpOrganization.Text33 = "";
                      }
                    }
                  }
                }
                else
                {
                  // look for the CP with an active child
                  if (ReadCaseCsePerson1())
                  {
                    ++local.CourtOrder.Index;
                    local.CourtOrder.CheckSize();

                    local.CourtOrder.Update.CourtOrderCaseNum.Text10 =
                      entities.Case1.Number;
                    local.Read.Number = entities.Case1.Number;

                    // ************************************************************************
                    //  check to see if person type is C for Client, if not 
                    // default to organization name
                    // ************************************************************************
                    if (AsChar(entities.CsePerson.Type1) == 'C')
                    {
                      local.Cp.Number = entities.CsePerson.Number;
                      UseSiReadCsePersonBatch2();

                      if (!IsEmpty(local.AbendData.AdabasResponseCd))
                      {
                        local.EabFileHandling.Action = "WRITE";
                        local.EabReportSend.RptDetail =
                          "Error getting CP   data for CSLN file.  Case: " + entities
                          .Case1.Number + " Person # " + entities
                          .CsePerson.Number + " SSN: " + local.Cp.Ssn;
                        UseCabErrorReport2();

                        if (!Equal(local.EabFileHandling.Status, "OK"))
                        {
                          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                          return;
                        }
                      }

                      local.CourtOrder.Update.Cp.FirstName = local.Cp.FirstName;
                      local.CourtOrder.Update.Cp.LastName = local.Cp.LastName;
                      local.CourtOrder.Update.Cp.MiddleInitial =
                        local.Cp.MiddleInitial;
                      local.CourtOrder.Update.Cp.Number = local.Cp.Number;
                    }
                    else
                    {
                      local.CourtOrder.Update.Cp.FirstName = "";
                      local.CourtOrder.Update.Cp.LastName = "";
                      local.CourtOrder.Update.Cp.MiddleInitial = "";
                      local.CourtOrder.Update.CpOrganization.Text33 =
                        entities.CsePerson.OrganizationName ?? Spaces(33);
                    }
                  }
                  else
                  {
                    // now look for the cp with an inactive child
                    if (ReadCaseCsePerson2())
                    {
                      ++local.CourtOrder.Index;
                      local.CourtOrder.CheckSize();

                      local.CourtOrder.Update.CourtOrderCaseNum.Text10 =
                        entities.Case1.Number;
                      local.Read.Number = entities.Case1.Number;

                      // ************************************************************************
                      //  check to see if person type is C for Client, if not 
                      // default to organization name
                      // ************************************************************************
                      if (AsChar(entities.CsePerson.Type1) == 'C')
                      {
                        local.Cp.Number = entities.CsePerson.Number;
                        UseSiReadCsePersonBatch2();

                        if (!IsEmpty(local.AbendData.AdabasResponseCd))
                        {
                          local.EabFileHandling.Action = "WRITE";
                          local.EabReportSend.RptDetail =
                            "Error getting CP   data for CSLN file.  Case: " + entities
                            .Case1.Number + " Person # " + entities
                            .CsePerson.Number + " SSN: " + local.Cp.Ssn;
                          UseCabErrorReport2();

                          if (!Equal(local.EabFileHandling.Status, "OK"))
                          {
                            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                            return;
                          }
                        }

                        local.CourtOrder.Update.Cp.FirstName =
                          local.Cp.FirstName;
                        local.CourtOrder.Update.Cp.LastName = local.Cp.LastName;
                        local.CourtOrder.Update.Cp.MiddleInitial =
                          local.Cp.MiddleInitial;
                        local.CourtOrder.Update.Cp.Number = local.Cp.Number;
                      }
                      else
                      {
                        local.CourtOrder.Update.Cp.FirstName = "";
                        local.CourtOrder.Update.Cp.LastName = "";
                        local.CourtOrder.Update.Cp.MiddleInitial = "";
                        local.CourtOrder.Update.CpOrganization.Text33 =
                          entities.CsePerson.OrganizationName ?? Spaces(33);
                      }
                    }
                    else
                    {
                      local.EabFileHandling.Action = "WRITE";
                      local.EabReportSend.RptDetail =
                        " CP err for CSLN file. nf in inactive Case: " + entities
                        .Case1.Number + " CP Person # " + entities
                        .CsePerson.Number + "child: " + local
                        .ChildPrevious.Number;
                      UseCabErrorReport2();

                      ++local.CourtOrder.Index;
                      local.CourtOrder.CheckSize();

                      local.CourtOrder.Update.Cp.FirstName = "";
                      local.CourtOrder.Update.Cp.LastName = "";
                      local.CourtOrder.Update.Cp.MiddleInitial = "";
                      local.CourtOrder.Update.CpOrganization.Text33 = "";
                    }
                  }
                }
              }
              else if (ReadCsePersonCase1())
              {
                ++local.CourtOrder.Index;
                local.CourtOrder.CheckSize();

                local.CourtOrder.Update.CourtOrderCaseNum.Text10 =
                  entities.Case1.Number;
                local.Read.Number = entities.Case1.Number;

                // ************************************************************************
                //  check to see if person type is C for Client, if not default 
                // to organization name
                // ************************************************************************
                if (AsChar(entities.CsePerson.Type1) == 'C')
                {
                  local.Cp.Number = entities.CsePerson.Number;
                  UseSiReadCsePersonBatch2();

                  if (!IsEmpty(local.AbendData.AdabasResponseCd))
                  {
                    local.EabFileHandling.Action = "WRITE";
                    local.EabReportSend.RptDetail =
                      "Error getting CP   data for CSLN file.  Case: " + entities
                      .Case1.Number + " Person # " + entities
                      .CsePerson.Number + " SSN: " + local.Cp.Ssn;
                    UseCabErrorReport2();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                      return;
                    }
                  }

                  local.CourtOrder.Update.Cp.FirstName = local.Cp.FirstName;
                  local.CourtOrder.Update.Cp.LastName = local.Cp.LastName;
                  local.CourtOrder.Update.Cp.MiddleInitial =
                    local.Cp.MiddleInitial;
                  local.CourtOrder.Update.Cp.Number = local.Cp.Number;
                }
                else
                {
                  local.CourtOrder.Update.Cp.FirstName = "";
                  local.CourtOrder.Update.Cp.LastName = "";
                  local.CourtOrder.Update.Cp.MiddleInitial = "";
                  local.CourtOrder.Update.CpOrganization.Text33 =
                    entities.CsePerson.OrganizationName ?? Spaces(33);
                }
              }
              else
              {
                // now look for cp with inactive child
                if (ReadCaseCsePerson2())
                {
                  ++local.CourtOrder.Index;
                  local.CourtOrder.CheckSize();

                  local.CourtOrder.Update.CourtOrderCaseNum.Text10 =
                    entities.Case1.Number;
                  local.Read.Number = entities.Case1.Number;

                  // ************************************************************************
                  //  check to see if person type is C for Client, if not 
                  // default to organization name
                  // ************************************************************************
                  if (AsChar(entities.CsePerson.Type1) == 'C')
                  {
                    local.Cp.Number = entities.CsePerson.Number;
                    UseSiReadCsePersonBatch2();

                    if (!IsEmpty(local.AbendData.AdabasResponseCd))
                    {
                      local.EabFileHandling.Action = "WRITE";
                      local.EabReportSend.RptDetail =
                        "Error getting CP   data for CSLN file.  Case: " + entities
                        .Case1.Number + " Person # " + entities
                        .CsePerson.Number + " SSN: " + local.Cp.Ssn;
                      UseCabErrorReport2();

                      if (!Equal(local.EabFileHandling.Status, "OK"))
                      {
                        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                        return;
                      }
                    }

                    local.CourtOrder.Update.Cp.FirstName = local.Cp.FirstName;
                    local.CourtOrder.Update.Cp.LastName = local.Cp.LastName;
                    local.CourtOrder.Update.Cp.MiddleInitial =
                      local.Cp.MiddleInitial;
                    local.CourtOrder.Update.Cp.Number = local.Cp.Number;
                  }
                  else
                  {
                    local.CourtOrder.Update.Cp.FirstName = "";
                    local.CourtOrder.Update.Cp.LastName = "";
                    local.CourtOrder.Update.Cp.MiddleInitial = "";
                    local.CourtOrder.Update.CpOrganization.Text33 =
                      entities.CsePerson.OrganizationName ?? Spaces(33);
                  }
                }
                else
                {
                  local.EabFileHandling.Action = "WRITE";
                  local.EabReportSend.RptDetail =
                    " CP err for CSLN file. nf in inactive Case: " + entities
                    .Case1.Number + " CP Person # " + entities
                    .CsePerson.Number + "child: " + local.ChildPrevious.Number;
                  UseCabErrorReport2();

                  ++local.CourtOrder.Index;
                  local.CourtOrder.CheckSize();

                  local.CourtOrder.Update.Cp.FirstName = "";
                  local.CourtOrder.Update.Cp.LastName = "";
                  local.CourtOrder.Update.Cp.MiddleInitial = "";
                  local.CourtOrder.Update.CpOrganization.Text33 = "";
                }
              }

              // ******************************************************
              //  Find Private Attorney for NCP and this court case number
              // ******************************************************
              if (ReadPersonPrivateAttorney2())
              {
                local.NcpAttyFirstName.Text12 =
                  entities.PersonPrivateAttorney.FirstName ?? Spaces(12);
                local.NcpAttyLastName.Text17 =
                  entities.PersonPrivateAttorney.LastName ?? Spaces(17);
                local.NcpAttyMiddleInitial.Text1 =
                  entities.PersonPrivateAttorney.MiddleInitial ?? Spaces(1);
                local.NcpAttorney.Identifier =
                  entities.PersonPrivateAttorney.Identifier;
                local.PrivateAttorneyAddress.Street1 = "";
                local.PrivateAttorneyAddress.Street2 = "";
                local.PrivateAttorneyAddress.ZipCode4 = "";
                local.PrivateAttorneyAddress.ZipCode5 = "";
                local.PrivateAttorneyAddress.City = "";
                local.PrivateAttorneyAddress.State = "";

                if (ReadPrivateAttorneyAddress())
                {
                  local.PrivateAttorneyAddress.Street1 =
                    entities.PrivateAttorneyAddress.Street1;
                  local.PrivateAttorneyAddress.Street2 =
                    entities.PrivateAttorneyAddress.Street2;
                  local.PrivateAttorneyAddress.ZipCode4 =
                    entities.PrivateAttorneyAddress.ZipCode4;
                  local.PrivateAttorneyAddress.ZipCode5 =
                    entities.PrivateAttorneyAddress.ZipCode5;
                  local.PrivateAttorneyAddress.City =
                    entities.PrivateAttorneyAddress.City;
                  local.PrivateAttorneyAddress.State =
                    entities.PrivateAttorneyAddress.State;
                }
              }
              else
              {
                local.NcpAttyFirstName.Text12 = "";
                local.NcpAttyLastName.Text17 = "";
                local.NcpAttyMiddleInitial.Text1 = "";
                local.PrivateAttorneyAddress.Street1 = "";
                local.PrivateAttorneyAddress.Street2 = "";
                local.PrivateAttorneyAddress.ZipCode4 = "";
                local.PrivateAttorneyAddress.ZipCode5 = "";
                local.PrivateAttorneyAddress.City = "";
                local.PrivateAttorneyAddress.State = "";
              }

              // ****************
              // Get child data
              // ****************
              local.ChildPrevious.Number = "";

              foreach(var item2 in ReadCsePerson4())
              {
                if (Equal(entities.ChCsePerson.Number, local.Cp.Number))
                {
                  continue;
                }

                if (!Equal(entities.ChCsePerson.Number,
                  local.ChildPrevious.Number))
                {
                  // now set up the child record
                  if (IsEmpty(local.ChildPrevious.Number))
                  {
                    local.CourtOrder.Item.ChildByCourtOrder.Index = -1;
                    local.CourtOrder.Item.ChildByCourtOrder.Count = 0;
                  }

                  ++local.CourtOrder.Item.ChildByCourtOrder.Index;
                  local.CourtOrder.Item.ChildByCourtOrder.CheckSize();

                  local.CourtOrder.Update.ChildByCourtOrder.Update.Child.
                    Number = entities.ChCsePerson.Number;
                  UseSiReadCsePersonBatch3();

                  if (!IsEmpty(local.AbendData.AdabasResponseCd))
                  {
                    local.EabFileHandling.Action = "WRITE";
                    local.EabReportSend.RptDetail =
                      "Error getting child   data for CSLN file.  Case: " + entities
                      .Case1.Number + " Person # " + entities
                      .CsePerson.Number + " SSN: " + local
                      .CourtOrder.Item.ChildByCourtOrder.Item.Child.Ssn;
                    UseCabErrorReport2();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                      return;
                    }
                  }

                  // *** ADDED SETS HERE FOR LOCAL GROUP CHILD ATTY NAME ETC
                  local.CourtOrder.Update.ChildByCourtOrder.Update.
                    ChildNcpAttyFrstNm.Text12 = local.NcpAttyFirstName.Text12;
                  local.CourtOrder.Update.ChildByCourtOrder.Update.
                    ChildNcpAttyLstNm.Text17 = local.NcpAttyLastName.Text17;
                  local.CourtOrder.Update.ChildByCourtOrder.Update.
                    ChildNcpAttyMi.Text1 = local.NcpAttyMiddleInitial.Text1;
                  local.CourtOrder.Update.ChildByCourtOrder.Update.ChildNcpAtty.
                    Street1 = local.PrivateAttorneyAddress.Street1 ?? "";
                  local.CourtOrder.Update.ChildByCourtOrder.Update.ChildNcpAtty.
                    Street2 = local.PrivateAttorneyAddress.Street2 ?? "";
                  local.CourtOrder.Update.ChildByCourtOrder.Update.ChildNcpAtty.
                    City = local.PrivateAttorneyAddress.City ?? "";
                  local.CourtOrder.Update.ChildByCourtOrder.Update.ChildNcpAtty.
                    State = local.PrivateAttorneyAddress.State ?? "";
                  local.CourtOrder.Update.ChildByCourtOrder.Update.ChildNcpAtty.
                    ZipCode5 = local.PrivateAttorneyAddress.ZipCode5 ?? "";
                  local.CourtOrder.Update.ChildByCourtOrder.Update.ChildNcpAtty.
                    ZipCode4 = local.PrivateAttorneyAddress.ZipCode4 ?? "";
                  local.ChildPrevious.Number = entities.ChCsePerson.Number;
                  local.CourtOrder.Update.ChildByCourtOrder.Update.
                    ChildStandardNum.Text20 = local.Previous.StandardNumber ?? Spaces
                    (20);
                }
              }

              if (IsEmpty(local.ChildPrevious.Number))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Court order without children, NCP: " + local
                  .AdministrativeActCertification.CaseNumber + " Court order: " +
                  (local.Previous.StandardNumber ?? "");
                UseCabErrorReport2();
                local.ScreenOwedAmounts.ArrearsAmountOwed = 0;
                local.ScreenOwedAmounts.CurrentAmountOwed = 0;

                goto Test2;
              }

              local.NcpTotal.ArrearsAmountOwed += local.ScreenOwedAmounts.
                ArrearsAmountOwed;
              local.CourtOrder1.CurrentAmountOwed = 0;
              local.CourtOrder1.ArrearsAmountOwed = 0;

              for(local.IwoGroupExtract.Index = 0; local
                .IwoGroupExtract.Index < local.IwoGroupExtract.Count; ++
                local.IwoGroupExtract.Index)
              {
                if (!local.IwoGroupExtract.CheckSize())
                {
                  break;
                }

                if (Equal(local.Previous.StandardNumber,
                  local.IwoGroupExtract.Item.CourtCase.StandardNumber))
                {
                  if (local.IwoGroupExtract.Item.IwoAmounts.
                    ArrearsAmountOwed > 0)
                  {
                    local.CourtOrder1.ArrearsAmountOwed += local.
                      IwoGroupExtract.Item.IwoAmounts.ArrearsAmountOwed;
                  }

                  if (local.IwoGroupExtract.Item.IwoAmounts.
                    CurrentAmountOwed > 0)
                  {
                    local.CourtOrder1.CurrentAmountOwed += local.
                      IwoGroupExtract.Item.IwoAmounts.CurrentAmountOwed;
                  }
                }
              }

              local.IwoGroupExtract.CheckIndex();

              if (local.CourtOrder1.ArrearsAmountOwed > 0)
              {
                local.CourtOrder.Update.ArrearsSupWithhdAmt.Text8 =
                  NumberToString((long)(local.CourtOrder1.ArrearsAmountOwed * 100
                  ), 8, 8);
              }
              else
              {
                local.CourtOrder.Update.ArrearsSupWithhdAmt.Text8 = "";
              }

              if (local.CourtOrder1.CurrentAmountOwed > 0)
              {
                local.CourtOrder.Update.CurrSupWithholdAmt.Text8 =
                  NumberToString((long)(local.CourtOrder1.CurrentAmountOwed * 100
                  ), 8, 8);
              }
              else
              {
                local.CourtOrder.Update.CurrSupWithholdAmt.Text8 = "";
              }

              ++local.CourtOrderCount.Count;
              local.CourtOrder.Update.StandardCourtOrder.Text20 =
                local.Previous.StandardNumber ?? Spaces(20);
              local.CourtOrder.Update.CourtOrderCountyNme.Text20 =
                entities.Fips.CountyDescription ?? Spaces(20);
              local.CourtOrder.Update.CourtOrderState.Text2 =
                entities.Fips.StateAbbreviation;
              local.CourtOrder.Update.PrepAttyAddr1.Text60 =
                local.PrepByAttyAddress1.Text60;
              local.CourtOrder.Update.PrepAttyAddr2.Text25 =
                local.PrepByAttyAddress2.Text25;
              local.CourtOrder.Update.PrepAttyCertificate.Text10 =
                local.PrepByAttyCertificate.Text10;
              local.CourtOrder.Update.PrepAttyCity.Text15 =
                local.PrepByAttyCity.Text15;
              local.CourtOrder.Update.PrepAttyEmail.Text50 =
                local.PrepByAttyEmail.Text50;
              local.CourtOrder.Update.PrepAttyFax.Text10 =
                local.PrepByAttyFax.Text10;
              local.CourtOrder.Update.PrepAttyFirstName.Text12 =
                local.PrepByAttyFirstName.Text12;
              local.CourtOrder.Update.PrepAttyLastName.Text17 =
                local.PrepByAttyLastName.Text17;
              local.CourtOrder.Update.PrepAttyMi.Text1 =
                local.PrepByAttyMiddleInitia.Text1;
              local.CourtOrder.Update.PrepAttyPhone.Text10 =
                local.PrepByAttyPhone.Text10;
              local.CourtOrder.Update.PrepAttyState.Text2 =
                local.PrepByAttyState.Text2;
              local.CourtOrder.Update.PrepAttyZip.Text9 =
                local.PrepByAttyZip.Text9;
              local.CourtOrder.Update.ArrearsAsOfDate.Date =
                local.ProcessingDate.Date;
              local.CourtOrder.Update.ArrearsByStandNumb.Text12 =
                NumberToString((long)(local.ScreenOwedAmounts.
                  ArrearsAmountOwed * 100), 4, 12);
              local.Read.Number = "";

              foreach(var item2 in ReadCourtCaption())
              {
                switch(entities.CourtCaption.Number)
                {
                  case 1:
                    local.CourtOrder.Update.LegalCaptionLine1.Text40 =
                      entities.CourtCaption.Line ?? Spaces(40);

                    break;
                  case 2:
                    local.CourtOrder.Update.LegalCaptionLine2.Text40 =
                      entities.CourtCaption.Line ?? Spaces(40);

                    break;
                  case 3:
                    local.CourtOrder.Update.LegalCaptionLine3.Text40 =
                      entities.CourtCaption.Line ?? Spaces(40);

                    break;
                  case 4:
                    local.CourtOrder.Update.LegalCaptionLine4.Text40 =
                      entities.CourtCaption.Line ?? Spaces(40);

                    break;
                  case 5:
                    local.CourtOrder.Update.LegalCaptionLine5.Text40 =
                      entities.CourtCaption.Line ?? Spaces(40);

                    break;
                  case 6:
                    local.CourtOrder.Update.LegalCaptionLine6.Text40 =
                      entities.CourtCaption.Line ?? Spaces(40);

                    break;
                  case 7:
                    local.CourtOrder.Update.LegalCaptionLine7.Text40 =
                      entities.CourtCaption.Line ?? Spaces(40);

                    break;
                  case 8:
                    local.CourtOrder.Update.LegalCaptionLine8.Text40 =
                      entities.CourtCaption.Line ?? Spaces(40);

                    break;
                  case 9:
                    local.CourtOrder.Update.LegalCaptionLine9.Text40 =
                      entities.CourtCaption.Line ?? Spaces(40);

                    break;
                  case 10:
                    local.CourtOrder.Update.LegalCaptionLine10.Text40 =
                      entities.CourtCaption.Line ?? Spaces(40);

                    break;
                  case 11:
                    local.CourtOrder.Update.LegalCaptionLine11.Text40 =
                      entities.CourtCaption.Line ?? Spaces(40);

                    break;
                  case 12:
                    local.CourtOrder.Update.LegalCaptionLine12.Text40 =
                      entities.CourtCaption.Line ?? Spaces(40);

                    break;
                  case 13:
                    local.CourtOrder.Update.LegalCaptionLine13.Text40 =
                      entities.CourtCaption.Line ?? Spaces(40);

                    break;
                  default:
                    break;
                }
              }

              // cq 42775 MOVED code into else to fix problem with zeroing out 
              // debts when no
              //                     children for court order found     Anita
            }

Test2:

            local.ScreenOwedAmounts.ArrearsAmountOwed = 0;
            local.ScreenOwedAmounts.CurrentAmountOwed = 0;
          }

Test3:

          local.Previous.Assign(entities.LegalAction);

          // ** prev
          local.StandardCourtOrderNumbe.Text20 =
            entities.LegalAction.StandardNumber ?? Spaces(20);

          foreach(var item2 in ReadDebtDetailObligationType())
          {
            if (AsChar(entities.ObligationType.Classification) == 'A')
            {
              // -- Debts for accruing obligation types are considered
              //         a) current if the due date is within the current month
              //         b) future if the due date is beyond the current month
              //         c) arrears if the due date is less than the first day 
              // of the current month.
              if (!Lt(entities.DebtDetail.DueDt, local.FirstOfTheMonth.Date))
              {
                if (!Lt(entities.DebtDetail.DueDt,
                  AddMonths(local.FirstOfTheMonth.Date, 1)))
                {
                  // --  Do not include future debt amounts in the Current 
                  // amount due calculation
                  continue;
                }

                local.ScreenOwedAmounts.CurrentAmountOwed += entities.
                  DebtDetail.BalanceDueAmt;
              }
              else
              {
                local.ScreenOwedAmounts.ArrearsAmountOwed += entities.
                  DebtDetail.BalanceDueAmt;
              }
            }
            else
            {
              // -- Debts for Non-Accruing obligation types are always 
              // considered Arrears
              local.ScreenOwedAmounts.ArrearsAmountOwed += entities.DebtDetail.
                BalanceDueAmt;
            }
          }

          if (local.ScreenOwedAmounts.ArrearsAmountOwed == 0)
          {
            continue;
          }
        }
      }
      else
      {
        local.AdministrativeActCertification.CaseNumber =
          entities.AdministrativeActCertification.CaseNumber;

        continue;
      }

ReadEach:
      ;
    }

    // *****************************************
    //  last record and end of file begin here
    // *****************************************
    if (!IsEmpty(local.StandardCourtOrderNumbe.Text20))
    {
      if (local.ScreenOwedAmounts.ArrearsAmountOwed == 0)
      {
      }
      else
      {
        if (!ReadTribunalFips())
        {
          goto Test4;
        }

        local.StandardCourtOrderNumbe.Text20 =
          local.Previous.StandardNumber ?? Spaces(20);
        local.IwoCabErrorMessages.Update.ErrorType.Text50 = "";
        local.IwoCabErrorMessages.Update.ErrorCourtCase.StandardNumber = "";
        UseCabGetIwoAmtsForCslnFile();
        local.ChildPrevious.Number = entities.ChCsePerson.Number;

        if (ReadCsePerson1())
        {
          local.ChildPrevious.Number = entities.ChCsePerson.Number;
        }

        if (IsEmpty(local.ChildPrevious.Number))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Court order without children, NCP: " + local
            .AdministrativeActCertification.CaseNumber + " Court order: " + (
              local.Previous.StandardNumber ?? "");
          UseCabErrorReport2();

          goto Test4;
        }

        // *************
        // 3RD get CP data
        // *************
        if (local.ScreenOwedAmounts.CurrentAmountOwed > 0 || local
          .ScreenOwedAmounts.ArrearsAmountOwed > 0)
        {
          local.IwoFound.Flag = "";
          local.Iwo.CreatedTstamp = local.Null1.Timestamp;

          foreach(var item in ReadLegalAction())
          {
            local.Iwo.CreatedTstamp = entities.LegalAction.CreatedTstamp;

            if (Equal(entities.LegalAction.ActionTaken, "IWOTERM") || Equal
              (entities.LegalAction.ActionTaken, "IWONOTKT"))
            {
              local.IwoFound.Flag = "N";

              break;
            }

            if (Equal(entities.LegalAction.ActionTaken, "IWOMODO") || Equal
              (entities.LegalAction.ActionTaken, "IWONOTKM") || Equal
              (entities.LegalAction.ActionTaken, "IWONOTKS") || Equal
              (entities.LegalAction.ActionTaken, "ORDIWO2") || Equal
              (entities.LegalAction.ActionTaken, "NOIIWON") || Equal
              (entities.LegalAction.ActionTaken, "IWO"))
            {
              local.IwoFound.Flag = "Y";

              break;
            }
            else
            {
              local.IwoFound.Flag = "N";

              break;
            }
          }

          if (AsChar(local.IwoFound.Flag) == 'Y')
          {
            if (ReadCsePersonCase2())
            {
              ++local.CourtOrder.Index;
              local.CourtOrder.CheckSize();

              local.CourtOrder.Update.CourtOrderCaseNum.Text10 =
                entities.Case1.Number;
              local.Read.Number = entities.Case1.Number;

              // ************************************************************************
              //  check to see if person type is C for Client, if not default to
              // organization name
              // ************************************************************************
              if (AsChar(entities.CsePerson.Type1) == 'C')
              {
                local.Cp.Number = entities.CsePerson.Number;
                UseSiReadCsePersonBatch2();

                if (!IsEmpty(local.AbendData.AdabasResponseCd))
                {
                  local.EabFileHandling.Action = "WRITE";
                  local.EabReportSend.RptDetail =
                    "Error getting CP   data for CSLN file.  Case: " + entities
                    .Case1.Number + " Person # " + entities.CsePerson.Number + " SSN: " +
                    local.Cp.Ssn;
                  UseCabErrorReport2();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                    return;
                  }
                }

                local.CourtOrder.Update.Cp.FirstName = local.Cp.FirstName;
                local.CourtOrder.Update.Cp.LastName = local.Cp.LastName;
                local.CourtOrder.Update.Cp.MiddleInitial =
                  local.Cp.MiddleInitial;
                local.CourtOrder.Update.Cp.Number = local.Cp.Number;
              }
              else
              {
                local.CourtOrder.Update.Cp.FirstName = "";
                local.CourtOrder.Update.Cp.LastName = "";
                local.CourtOrder.Update.Cp.MiddleInitial = "";
                local.CourtOrder.Update.CpOrganization.Text33 =
                  entities.CsePerson.OrganizationName ?? Spaces(33);
              }
            }
            else
            {
              // first look for cp with active child
              if (ReadCaseCsePerson1())
              {
                ++local.CourtOrder.Index;
                local.CourtOrder.CheckSize();

                local.CourtOrder.Update.CourtOrderCaseNum.Text10 =
                  entities.Case1.Number;
                local.Read.Number = entities.Case1.Number;

                // ************************************************************************
                //  check to see if person type is C for Client, if not default 
                // to organization name
                // ************************************************************************
                if (AsChar(entities.CsePerson.Type1) == 'C')
                {
                  local.Cp.Number = entities.CsePerson.Number;
                  UseSiReadCsePersonBatch2();

                  if (!IsEmpty(local.AbendData.AdabasResponseCd))
                  {
                    local.EabFileHandling.Action = "WRITE";
                    local.EabReportSend.RptDetail =
                      "Error getting CP   data for CSLN file.  Case: " + entities
                      .Case1.Number + " Person # " + entities
                      .CsePerson.Number + " SSN: " + local.Cp.Ssn;
                    UseCabErrorReport2();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                      return;
                    }
                  }

                  local.CourtOrder.Update.Cp.FirstName = local.Cp.FirstName;
                  local.CourtOrder.Update.Cp.LastName = local.Cp.LastName;
                  local.CourtOrder.Update.Cp.MiddleInitial =
                    local.Cp.MiddleInitial;
                  local.CourtOrder.Update.Cp.Number = local.Cp.Number;
                }
                else
                {
                  local.CourtOrder.Update.Cp.FirstName = "";
                  local.CourtOrder.Update.Cp.LastName = "";
                  local.CourtOrder.Update.Cp.MiddleInitial = "";
                  local.CourtOrder.Update.CpOrganization.Text33 =
                    entities.CsePerson.OrganizationName ?? Spaces(33);
                }
              }
              else
              {
                // ok we didn't find the cp with an active child so lets look 
                // without the edit for an active child.   11/21/13
                if (ReadCaseCsePerson2())
                {
                  ++local.CourtOrder.Index;
                  local.CourtOrder.CheckSize();

                  local.CourtOrder.Update.CourtOrderCaseNum.Text10 =
                    entities.Case1.Number;
                  local.Read.Number = entities.Case1.Number;

                  // ************************************************************************
                  //  check to see if person type is C for Client, if not 
                  // default to organization name
                  // ************************************************************************
                  if (AsChar(entities.CsePerson.Type1) == 'C')
                  {
                    local.Cp.Number = entities.CsePerson.Number;
                    UseSiReadCsePersonBatch2();

                    if (!IsEmpty(local.AbendData.AdabasResponseCd))
                    {
                      local.EabFileHandling.Action = "WRITE";
                      local.EabReportSend.RptDetail =
                        "Error getting CP   data for CSLN file.  Case: " + entities
                        .Case1.Number + " Person # " + entities
                        .CsePerson.Number + " SSN: " + local.Cp.Ssn;
                      UseCabErrorReport2();

                      if (!Equal(local.EabFileHandling.Status, "OK"))
                      {
                        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                        return;
                      }
                    }

                    local.CourtOrder.Update.Cp.FirstName = local.Cp.FirstName;
                    local.CourtOrder.Update.Cp.LastName = local.Cp.LastName;
                    local.CourtOrder.Update.Cp.MiddleInitial =
                      local.Cp.MiddleInitial;
                    local.CourtOrder.Update.Cp.Number = local.Cp.Number;
                  }
                  else
                  {
                    local.CourtOrder.Update.Cp.FirstName = "";
                    local.CourtOrder.Update.Cp.LastName = "";
                    local.CourtOrder.Update.Cp.MiddleInitial = "";
                    local.CourtOrder.Update.CpOrganization.Text33 =
                      entities.CsePerson.OrganizationName ?? Spaces(33);
                  }
                }
                else
                {
                  local.EabFileHandling.Action = "WRITE";
                  local.EabReportSend.RptDetail =
                    " CP err for CSLN file. nf in inactive Case: " + entities
                    .Case1.Number + " CP Person # " + entities
                    .CsePerson.Number + "child: " + local.ChildPrevious.Number;
                  UseCabErrorReport2();

                  ++local.CourtOrder.Index;
                  local.CourtOrder.CheckSize();

                  local.CourtOrder.Update.Cp.FirstName = "";
                  local.CourtOrder.Update.Cp.LastName = "";
                  local.CourtOrder.Update.Cp.MiddleInitial = "";
                  local.CourtOrder.Update.CpOrganization.Text33 = "";
                }
              }
            }
          }
          else
          {
            // look for the CP with an active child
            if (ReadCaseCsePerson1())
            {
              ++local.CourtOrder.Index;
              local.CourtOrder.CheckSize();

              local.CourtOrder.Update.CourtOrderCaseNum.Text10 =
                entities.Case1.Number;
              local.Read.Number = entities.Case1.Number;

              // ************************************************************************
              //  check to see if person type is C for Client, if not default to
              // organization name
              // ************************************************************************
              if (AsChar(entities.CsePerson.Type1) == 'C')
              {
                local.Cp.Number = entities.CsePerson.Number;
                UseSiReadCsePersonBatch2();

                if (!IsEmpty(local.AbendData.AdabasResponseCd))
                {
                  local.EabFileHandling.Action = "WRITE";
                  local.EabReportSend.RptDetail =
                    "Error getting CP   data for CSLN file.  Case: " + entities
                    .Case1.Number + " Person # " + entities.CsePerson.Number + " SSN: " +
                    local.Cp.Ssn;
                  UseCabErrorReport2();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                    return;
                  }
                }

                local.CourtOrder.Update.Cp.FirstName = local.Cp.FirstName;
                local.CourtOrder.Update.Cp.LastName = local.Cp.LastName;
                local.CourtOrder.Update.Cp.MiddleInitial =
                  local.Cp.MiddleInitial;
                local.CourtOrder.Update.Cp.Number = local.Cp.Number;
              }
              else
              {
                local.CourtOrder.Update.Cp.FirstName = "";
                local.CourtOrder.Update.Cp.LastName = "";
                local.CourtOrder.Update.Cp.MiddleInitial = "";
                local.CourtOrder.Update.CpOrganization.Text33 =
                  entities.CsePerson.OrganizationName ?? Spaces(33);
              }
            }
            else
            {
              // now look for the cp with an inactive child
              if (ReadCaseCsePerson2())
              {
                ++local.CourtOrder.Index;
                local.CourtOrder.CheckSize();

                local.CourtOrder.Update.CourtOrderCaseNum.Text10 =
                  entities.Case1.Number;
                local.Read.Number = entities.Case1.Number;

                // ************************************************************************
                //  check to see if person type is C for Client, if not default 
                // to organization name
                // ************************************************************************
                if (AsChar(entities.CsePerson.Type1) == 'C')
                {
                  local.Cp.Number = entities.CsePerson.Number;
                  UseSiReadCsePersonBatch2();

                  if (!IsEmpty(local.AbendData.AdabasResponseCd))
                  {
                    local.EabFileHandling.Action = "WRITE";
                    local.EabReportSend.RptDetail =
                      "Error getting CP   data for CSLN file.  Case: " + entities
                      .Case1.Number + " Person # " + entities
                      .CsePerson.Number + " SSN: " + local.Cp.Ssn;
                    UseCabErrorReport2();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                      return;
                    }
                  }

                  local.CourtOrder.Update.Cp.FirstName = local.Cp.FirstName;
                  local.CourtOrder.Update.Cp.LastName = local.Cp.LastName;
                  local.CourtOrder.Update.Cp.MiddleInitial =
                    local.Cp.MiddleInitial;
                  local.CourtOrder.Update.Cp.Number = local.Cp.Number;
                }
                else
                {
                  local.CourtOrder.Update.Cp.FirstName = "";
                  local.CourtOrder.Update.Cp.LastName = "";
                  local.CourtOrder.Update.Cp.MiddleInitial = "";
                  local.CourtOrder.Update.CpOrganization.Text33 =
                    entities.CsePerson.OrganizationName ?? Spaces(33);
                }
              }
              else
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  " CP err for CSLN file. nf in inactive Case: " + entities
                  .Case1.Number + " CP Person # " + entities
                  .CsePerson.Number + "child: " + local.ChildPrevious.Number;
                UseCabErrorReport2();

                ++local.CourtOrder.Index;
                local.CourtOrder.CheckSize();

                local.CourtOrder.Update.Cp.FirstName = "";
                local.CourtOrder.Update.Cp.LastName = "";
                local.CourtOrder.Update.Cp.MiddleInitial = "";
                local.CourtOrder.Update.CpOrganization.Text33 = "";
              }
            }
          }
        }
        else if (ReadCsePersonCase1())
        {
          ++local.CourtOrder.Index;
          local.CourtOrder.CheckSize();

          local.CourtOrder.Update.CourtOrderCaseNum.Text10 =
            entities.Case1.Number;
          local.Read.Number = entities.Case1.Number;

          // ************************************************************************
          //  check to see if person type is C for Client, if not default to 
          // organization name
          // ************************************************************************
          if (AsChar(entities.CsePerson.Type1) == 'C')
          {
            local.Cp.Number = entities.CsePerson.Number;
            UseSiReadCsePersonBatch2();

            if (!IsEmpty(local.AbendData.AdabasResponseCd))
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Error getting CP   data for CSLN file.  Case: " + entities
                .Case1.Number + " Person # " + entities.CsePerson.Number + " SSN: " +
                local.Cp.Ssn;
              UseCabErrorReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }
            }

            local.CourtOrder.Update.Cp.FirstName = local.Cp.FirstName;
            local.CourtOrder.Update.Cp.LastName = local.Cp.LastName;
            local.CourtOrder.Update.Cp.MiddleInitial = local.Cp.MiddleInitial;
            local.CourtOrder.Update.Cp.Number = local.Cp.Number;
          }
          else
          {
            local.CourtOrder.Update.Cp.FirstName = "";
            local.CourtOrder.Update.Cp.LastName = "";
            local.CourtOrder.Update.Cp.MiddleInitial = "";
            local.CourtOrder.Update.CpOrganization.Text33 =
              entities.CsePerson.OrganizationName ?? Spaces(33);
          }
        }
        else
        {
          // now look for cp with inactive child
          if (ReadCaseCsePerson2())
          {
            ++local.CourtOrder.Index;
            local.CourtOrder.CheckSize();

            local.CourtOrder.Update.CourtOrderCaseNum.Text10 =
              entities.Case1.Number;
            local.Read.Number = entities.Case1.Number;

            // ************************************************************************
            //  check to see if person type is C for Client, if not default to 
            // organization name
            // ************************************************************************
            if (AsChar(entities.CsePerson.Type1) == 'C')
            {
              local.Cp.Number = entities.CsePerson.Number;
              UseSiReadCsePersonBatch2();

              if (!IsEmpty(local.AbendData.AdabasResponseCd))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Error getting CP   data for CSLN file.  Case: " + entities
                  .Case1.Number + " Person # " + entities.CsePerson.Number + " SSN: " +
                  local.Cp.Ssn;
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                  return;
                }
              }

              local.CourtOrder.Update.Cp.FirstName = local.Cp.FirstName;
              local.CourtOrder.Update.Cp.LastName = local.Cp.LastName;
              local.CourtOrder.Update.Cp.MiddleInitial = local.Cp.MiddleInitial;
              local.CourtOrder.Update.Cp.Number = local.Cp.Number;
            }
            else
            {
              local.CourtOrder.Update.Cp.FirstName = "";
              local.CourtOrder.Update.Cp.LastName = "";
              local.CourtOrder.Update.Cp.MiddleInitial = "";
              local.CourtOrder.Update.CpOrganization.Text33 =
                entities.CsePerson.OrganizationName ?? Spaces(33);
            }
          }
          else
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              " CP err for CSLN file. nf in inactive Case: " + entities
              .Case1.Number + " CP Person # " + entities.CsePerson.Number + "child: " +
              local.ChildPrevious.Number;
            UseCabErrorReport2();

            ++local.CourtOrder.Index;
            local.CourtOrder.CheckSize();

            local.CourtOrder.Update.Cp.FirstName = "";
            local.CourtOrder.Update.Cp.LastName = "";
            local.CourtOrder.Update.Cp.MiddleInitial = "";
            local.CourtOrder.Update.CpOrganization.Text33 = "";
          }
        }

        // ******************************************************
        //  Find Private Attorney for NCP and this court case number
        // ******************************************************
        if (ReadPersonPrivateAttorney1())
        {
          local.NcpAttyFirstName.Text12 =
            entities.PersonPrivateAttorney.FirstName ?? Spaces(12);
          local.NcpAttyLastName.Text17 =
            entities.PersonPrivateAttorney.LastName ?? Spaces(17);
          local.NcpAttyMiddleInitial.Text1 =
            entities.PersonPrivateAttorney.MiddleInitial ?? Spaces(1);
          local.NcpAttorney.Identifier =
            entities.PersonPrivateAttorney.Identifier;
          local.PrivateAttorneyAddress.Street1 = "";
          local.PrivateAttorneyAddress.Street2 = "";
          local.PrivateAttorneyAddress.ZipCode4 = "";
          local.PrivateAttorneyAddress.ZipCode5 = "";
          local.PrivateAttorneyAddress.City = "";
          local.PrivateAttorneyAddress.State = "";

          if (ReadPrivateAttorneyAddress())
          {
            local.PrivateAttorneyAddress.Street1 =
              entities.PrivateAttorneyAddress.Street1;
            local.PrivateAttorneyAddress.Street2 =
              entities.PrivateAttorneyAddress.Street2;
            local.PrivateAttorneyAddress.ZipCode4 =
              entities.PrivateAttorneyAddress.ZipCode4;
            local.PrivateAttorneyAddress.ZipCode5 =
              entities.PrivateAttorneyAddress.ZipCode5;
            local.PrivateAttorneyAddress.City =
              entities.PrivateAttorneyAddress.City;
            local.PrivateAttorneyAddress.State =
              entities.PrivateAttorneyAddress.State;
          }
        }
        else
        {
          local.NcpAttyFirstName.Text12 = "";
          local.NcpAttyLastName.Text17 = "";
          local.NcpAttyMiddleInitial.Text1 = "";
          local.PrivateAttorneyAddress.Street1 = "";
          local.PrivateAttorneyAddress.Street2 = "";
          local.PrivateAttorneyAddress.ZipCode4 = "";
          local.PrivateAttorneyAddress.ZipCode5 = "";
          local.PrivateAttorneyAddress.City = "";
          local.PrivateAttorneyAddress.State = "";
        }

        // *** GET CHILD DATA
        local.ChildPrevious.Number = "";

        foreach(var item in ReadCsePerson4())
        {
          if (Equal(entities.ChCsePerson.Number, local.Cp.Number))
          {
            continue;
          }

          if (!Equal(entities.ChCsePerson.Number, local.ChildPrevious.Number))
          {
            // ****************************
            // now set up the CHILD record
            // ****************************
            if (IsEmpty(local.ChildPrevious.Number))
            {
              local.CourtOrder.Item.ChildByCourtOrder.Index = -1;
              local.CourtOrder.Item.ChildByCourtOrder.Count = 0;
            }

            ++local.CourtOrder.Item.ChildByCourtOrder.Index;
            local.CourtOrder.Item.ChildByCourtOrder.CheckSize();

            local.CourtOrder.Update.ChildByCourtOrder.Update.Child.Number =
              entities.ChCsePerson.Number;
            UseSiReadCsePersonBatch3();

            if (!IsEmpty(local.AbendData.AdabasResponseCd))
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Error getting child   data for CSLN file.  Case: " + entities
                .Case1.Number + " Person # " + entities.CsePerson.Number + " SSN: " +
                local.CourtOrder.Item.ChildByCourtOrder.Item.Child.Ssn;
              UseCabErrorReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }
            }

            // *** ADDED SETS HERE FOR LOCAL GROUP CHILD ATTY NAME ETC
            local.CourtOrder.Update.ChildByCourtOrder.Update.ChildNcpAttyFrstNm.
              Text12 = local.NcpAttyFirstName.Text12;
            local.CourtOrder.Update.ChildByCourtOrder.Update.ChildNcpAttyLstNm.
              Text17 = local.NcpAttyLastName.Text17;
            local.CourtOrder.Update.ChildByCourtOrder.Update.ChildNcpAttyMi.
              Text1 = local.NcpAttyMiddleInitial.Text1;
            local.CourtOrder.Update.ChildByCourtOrder.Update.ChildNcpAtty.
              Street1 = local.PrivateAttorneyAddress.Street1 ?? "";
            local.CourtOrder.Update.ChildByCourtOrder.Update.ChildNcpAtty.
              Street2 = local.PrivateAttorneyAddress.Street2 ?? "";
            local.CourtOrder.Update.ChildByCourtOrder.Update.ChildNcpAtty.City =
              local.PrivateAttorneyAddress.City ?? "";
            local.CourtOrder.Update.ChildByCourtOrder.Update.ChildNcpAtty.
              State = local.PrivateAttorneyAddress.State ?? "";
            local.CourtOrder.Update.ChildByCourtOrder.Update.ChildNcpAtty.
              ZipCode5 = local.PrivateAttorneyAddress.ZipCode5 ?? "";
            local.CourtOrder.Update.ChildByCourtOrder.Update.ChildNcpAtty.
              ZipCode4 = local.PrivateAttorneyAddress.ZipCode4 ?? "";
            local.ChildPrevious.Number = entities.ChCsePerson.Number;
            local.CourtOrder.Update.ChildByCourtOrder.Update.ChildStandardNum.
              Text20 = local.Previous.StandardNumber ?? Spaces(20);
          }
        }

        if (IsEmpty(local.ChildPrevious.Number))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Court order without children, NCP: " + local
            .AdministrativeActCertification.CaseNumber + " Court order: " + (
              local.Previous.StandardNumber ?? "");
          UseCabErrorReport2();

          goto Test4;
        }

        local.CourtOrder1.ArrearsAmountOwed = 0;
        local.CourtOrder1.CurrentAmountOwed = 0;

        for(local.IwoGroupExtract.Index = 0; local.IwoGroupExtract.Index < local
          .IwoGroupExtract.Count; ++local.IwoGroupExtract.Index)
        {
          if (!local.IwoGroupExtract.CheckSize())
          {
            break;
          }

          if (Equal(local.Previous.StandardNumber,
            local.IwoGroupExtract.Item.CourtCase.StandardNumber))
          {
            if (local.IwoGroupExtract.Item.IwoAmounts.ArrearsAmountOwed > 0)
            {
              local.CourtOrder1.ArrearsAmountOwed += local.IwoGroupExtract.Item.
                IwoAmounts.ArrearsAmountOwed;
            }

            if (local.IwoGroupExtract.Item.IwoAmounts.CurrentAmountOwed > 0)
            {
              local.CourtOrder1.CurrentAmountOwed += local.IwoGroupExtract.Item.
                IwoAmounts.CurrentAmountOwed;
            }
          }
        }

        local.IwoGroupExtract.CheckIndex();

        if (local.CourtOrder1.ArrearsAmountOwed > 0)
        {
          local.CourtOrder.Update.ArrearsSupWithhdAmt.Text8 =
            NumberToString((long)(local.CourtOrder1.ArrearsAmountOwed * 100), 8,
            8);
        }
        else
        {
          local.CourtOrder.Update.ArrearsSupWithhdAmt.Text8 = "";
        }

        if (local.CourtOrder1.CurrentAmountOwed > 0)
        {
          local.CourtOrder.Update.CurrSupWithholdAmt.Text8 =
            NumberToString((long)(local.CourtOrder1.CurrentAmountOwed * 100), 8,
            8);
        }
        else
        {
          local.CourtOrder.Update.CurrSupWithholdAmt.Text8 = "";
        }

        local.NcpTotal.ArrearsAmountOwed += local.ScreenOwedAmounts.
          ArrearsAmountOwed;
        ++local.CourtOrderCount.Count;
        local.CourtOrder.Update.StandardCourtOrder.Text20 =
          local.Previous.StandardNumber ?? Spaces(20);
        local.CourtOrder.Update.CourtOrderCountyNme.Text20 =
          entities.Fips.CountyDescription ?? Spaces(20);
        local.CourtOrder.Update.CourtOrderState.Text2 =
          entities.Fips.StateAbbreviation;
        local.CourtOrder.Update.PrepAttyAddr1.Text60 =
          local.PrepByAttyAddress1.Text60;
        local.CourtOrder.Update.PrepAttyAddr2.Text25 =
          local.PrepByAttyAddress2.Text25;
        local.CourtOrder.Update.PrepAttyCertificate.Text10 =
          local.PrepByAttyCertificate.Text10;
        local.CourtOrder.Update.PrepAttyCity.Text15 =
          local.PrepByAttyCity.Text15;
        local.CourtOrder.Update.PrepAttyEmail.Text50 =
          local.PrepByAttyEmail.Text50;
        local.CourtOrder.Update.PrepAttyFax.Text10 = local.PrepByAttyFax.Text10;
        local.CourtOrder.Update.PrepAttyFirstName.Text12 =
          local.PrepByAttyFirstName.Text12;
        local.CourtOrder.Update.PrepAttyLastName.Text17 =
          local.PrepByAttyLastName.Text17;
        local.CourtOrder.Update.PrepAttyMi.Text1 =
          local.PrepByAttyMiddleInitia.Text1;
        local.CourtOrder.Update.PrepAttyPhone.Text10 =
          local.PrepByAttyPhone.Text10;
        local.CourtOrder.Update.PrepAttyState.Text2 =
          local.PrepByAttyState.Text2;
        local.CourtOrder.Update.PrepAttyZip.Text9 = local.PrepByAttyZip.Text9;
        local.CourtOrder.Update.ArrearsAsOfDate.Date =
          local.ProcessingDate.Date;
        local.CourtOrder.Update.ArrearsByStandNumb.Text12 =
          NumberToString((long)(local.ScreenOwedAmounts.ArrearsAmountOwed * 100),
          4, 12);
        local.Read.Number = "";

        foreach(var item in ReadCourtCaption())
        {
          switch(entities.CourtCaption.Number)
          {
            case 1:
              local.CourtOrder.Update.LegalCaptionLine1.Text40 =
                entities.CourtCaption.Line ?? Spaces(40);

              break;
            case 2:
              local.CourtOrder.Update.LegalCaptionLine2.Text40 =
                entities.CourtCaption.Line ?? Spaces(40);

              break;
            case 3:
              local.CourtOrder.Update.LegalCaptionLine3.Text40 =
                entities.CourtCaption.Line ?? Spaces(40);

              break;
            case 4:
              local.CourtOrder.Update.LegalCaptionLine4.Text40 =
                entities.CourtCaption.Line ?? Spaces(40);

              break;
            case 5:
              local.CourtOrder.Update.LegalCaptionLine5.Text40 =
                entities.CourtCaption.Line ?? Spaces(40);

              break;
            case 6:
              local.CourtOrder.Update.LegalCaptionLine6.Text40 =
                entities.CourtCaption.Line ?? Spaces(40);

              break;
            case 7:
              local.CourtOrder.Update.LegalCaptionLine7.Text40 =
                entities.CourtCaption.Line ?? Spaces(40);

              break;
            case 8:
              local.CourtOrder.Update.LegalCaptionLine8.Text40 =
                entities.CourtCaption.Line ?? Spaces(40);

              break;
            case 9:
              local.CourtOrder.Update.LegalCaptionLine9.Text40 =
                entities.CourtCaption.Line ?? Spaces(40);

              break;
            case 10:
              local.CourtOrder.Update.LegalCaptionLine10.Text40 =
                entities.CourtCaption.Line ?? Spaces(40);

              break;
            case 11:
              local.CourtOrder.Update.LegalCaptionLine11.Text40 =
                entities.CourtCaption.Line ?? Spaces(40);

              break;
            case 12:
              local.CourtOrder.Update.LegalCaptionLine12.Text40 =
                entities.CourtCaption.Line ?? Spaces(40);

              break;
            case 13:
              local.CourtOrder.Update.LegalCaptionLine13.Text40 =
                entities.CourtCaption.Line ?? Spaces(40);

              break;
            default:
              break;
          }
        }
      }
    }

Test4:

    local.NcpRecordArrsBalance.Text14 =
      NumberToString((long)(local.NcpTotal.ArrearsAmountOwed * 100), 2, 14);

    if (local.NcpTotal.ArrearsAmountOwed > 499)
    {
      if (local.NcpCount.Count >= 1)
      {
        // ****************************************************
        //   Write 01 NCP  record
        // ****************************************************
        ++local.NcpCount.Count;
        local.PassArea.FileInstruction = "WRITE";
        local.RecordType.Text2 = "01";
        UseOeEabWriteCslnFile3();

        if (!Equal(local.PassArea.TextReturnCode, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error in writing last NCP record in external file  for 'oe_eab_write_csln_file'.";
            
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        // ****************************************************
        // now we will do the court order for the previous ncp
        // ****************************************************
        local.CourtOrder.Index = 0;

        for(var limit = local.CourtOrder.Count; local.CourtOrder.Index < limit; ++
          local.CourtOrder.Index)
        {
          if (!local.CourtOrder.CheckSize())
          {
            break;
          }

          // ****************************************************
          //   Write 10 court order  record
          // ****************************************************
          local.PassArea.FileInstruction = "WRITE";
          local.RecordType.Text2 = "10";
          UseOeEabWriteCslnFile4();

          if (!Equal(local.PassArea.TextReturnCode, "OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error in writing last end ncp in external file  for 'oe_eab_write_csln_file'.";
              
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          local.CourtOrder.Item.ChildByCourtOrder.Index = 0;

          for(var limit1 = local.CourtOrder.Item.ChildByCourtOrder.Count; local
            .CourtOrder.Item.ChildByCourtOrder.Index < limit1; ++
            local.CourtOrder.Item.ChildByCourtOrder.Index)
          {
            if (!local.CourtOrder.Item.ChildByCourtOrder.CheckSize())
            {
              break;
            }

            // **********************************************
            // write 20  CHILD record for the previous NCP
            // **********************************************
            local.PassArea.FileInstruction = "WRITE";
            local.RecordType.Text2 = "20";

            if (Equal(local.CourtOrder.Item.ChildByCourtOrder.Item.
              ChildStandardNum.Text20,
              local.CourtOrder.Item.StandardCourtOrder.Text20))
            {
              UseOeEabWriteCslnFile7();

              if (!Equal(local.PassArea.TextReturnCode, "OK"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Error in writing last child record in external file  for 'oe_eab_write_csln_file'.";
                  
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                  return;
                }

                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }
            }
          }

          local.CourtOrder.Item.ChildByCourtOrder.CheckIndex();
        }

        local.CourtOrder.CheckIndex();

        // ***************************************************
        // now we will do the end record for the previous NCP
        // ***************************************************
        local.PassArea.FileInstruction = "WRITE";
        local.RecordType.Text2 = "89";
        UseOeEabWriteCslnFile6();

        if (!Equal(local.PassArea.TextReturnCode, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error in writing last court order record in external file  for 'oe_eab_write_csln_file'.";
            
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }
    }

    // **************************************
    // now we will do the end of file record
    // **************************************
    local.PassArea.FileInstruction = "WRITE";
    local.RecordType.Text2 = "99";
    local.FileTrailerNcpCount.Text6 =
      NumberToString(local.NcpCount.Count, 10, 6);
    UseOeEabWriteCslnFile8();

    if (!Equal(local.PassArea.TextReturnCode, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error in writing file trailer record in external file  for 'oe_eab_write_csln_file'.";
        
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail =
      "Total Number of CSLN NCP Records Written:  " + NumberToString
      (local.NcpCount.Count, 15);
    local.EabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writing control report(Total number of CSLN NCP Records Written).";
        
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    if (AsChar(local.FileOpened.Flag) == 'Y')
    {
      // ******************************************
      // *      Close External CSLN  File         *
      // ******************************************
      local.PassArea.FileInstruction = "CLOSE";
      UseOeEabWriteCslnFile1();

      if (!Equal(local.PassArea.TextReturnCode, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error in closing external file  for 'oe_eab_write_CSLN_file'.";
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ExitState = "FILE_CLOSE_ERROR";

        return;
      }

      local.EabFileHandling.Action = "CLOSE";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error encountered while closing control report.";
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
    }
  }

  private static void MoveCsePersonAddress(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.LastName = source.LastName;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveErrorMessages(CabGetIwoAmtsForCslnFile.Export.
    ErrorMessagesGroup source, Local.IwoCabErrorMessagesGroup target)
  {
    target.ErrorCourtCase.StandardNumber = source.ErrorCourtCase.StandardNumber;
    target.ErrorType.Text50 = source.ErrorType.Text50;
  }

  private static void MoveExtract(CabGetIwoAmtsForCslnFile.Export.
    ExtractGroup source, Local.IwoGroupExtractGroup target)
  {
    target.MaxIwoPct.MaxWithholdingPercent =
      source.MaxPct.MaxWithholdingPercent;
    target.CourtCase.StandardNumber = source.CourtCase.StandardNumber;
    MoveScreenOwedAmounts(source.Amounts, target.IwoAmounts);
    target.Client.Assign(source.Client);
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

  private static void MoveScreenOwedAmounts(ScreenOwedAmounts source,
    ScreenOwedAmounts target)
  {
    target.CurrentAmountOwed = source.CurrentAmountOwed;
    target.ArrearsAmountOwed = source.ArrearsAmountOwed;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabFirstAndLastDateOfMonth()
  {
    var useImport = new CabFirstAndLastDateOfMonth.Import();
    var useExport = new CabFirstAndLastDateOfMonth.Export();

    useImport.DateWorkArea.Date = local.ProcessingDate.Date;

    Call(CabFirstAndLastDateOfMonth.Execute, useImport, useExport);

    local.LastOfTheMonth.Date = useExport.Last.Date;
    local.FirstOfTheMonth.Date = useExport.First.Date;
  }

  private void UseCabGetIwoAmtsForCslnFile()
  {
    var useImport = new CabGetIwoAmtsForCslnFile.Import();
    var useExport = new CabGetIwoAmtsForCslnFile.Export();

    useImport.StandardCourtOrder.Text20 = local.StandardCourtOrderNumbe.Text20;
    MoveCsePersonsWorkSet(local.NcpRecordName, useImport.CsePersonsWorkSet);
    useImport.CsePerson.Number = entities.NcpCsePerson.Number;

    Call(CabGetIwoAmtsForCslnFile.Execute, useImport, useExport);

    useExport.Extract.CopyTo(local.IwoGroupExtract, MoveExtract);
    useExport.ErrorMessages.
      CopyTo(local.IwoCabErrorMessages, MoveErrorMessages);
  }

  private void UseFnCabReturnTextDollars()
  {
    var useImport = new FnCabReturnTextDollars.Import();
    var useExport = new FnCabReturnTextDollars.Export();

    useImport.FinanceWorkAttributes.NumericalDollarValue =
      local.FinanceWorkAttributes.NumericalDollarValue;

    Call(FnCabReturnTextDollars.Execute, useImport, useExport);

    local.FinanceWorkAttributes.TextDollarValue =
      useExport.FinanceWorkAttributes.TextDollarValue;
  }

  private void UseOeEabWriteCslnFile1()
  {
    var useImport = new OeEabWriteCslnFile.Import();
    var useExport = new OeEabWriteCslnFile.Export();

    Call(OeEabWriteCslnFile.Execute, useImport, useExport);
  }

  private void UseOeEabWriteCslnFile2()
  {
    var useImport = new OeEabWriteCslnFile.Import();
    var useExport = new OeEabWriteCslnFile.Export();

    useImport.NcpRecord.Assign(local.NcpRecordName);
    MoveCsePersonAddress(local.NcpCsePersonAddress, useImport.Ncp);
    useImport.ArrsBalanceAsOfDate.Date = local.ProcessingDate.Date;
    useImport.NcpRecordArrsBalance.Text14 = local.NcpRecordArrsBalance.Text14;
    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    useImport.RecordType.Text2 = local.RecordType.Text2;
    useImport.NcpAddressVerified.Flag = local.NcpAddressVerified.Flag;
    useImport.CourtOrderCount.Count = local.CourtOrderCount.Count;
    useImport.ProcessDate.Date = local.ProcessingDate.Date;
    useImport.PrivateAttorneyAddress.Assign(local.PrivateAttorneyAddress);
    useExport.External.Assign(local.PassArea);

    Call(OeEabWriteCslnFile.Execute, useImport, useExport);

    local.PassArea.Assign(useExport.External);
  }

  private void UseOeEabWriteCslnFile3()
  {
    var useImport = new OeEabWriteCslnFile.Import();
    var useExport = new OeEabWriteCslnFile.Export();

    useImport.NcpRecord.Assign(local.NcpRecordName);
    MoveCsePersonAddress(local.NcpCsePersonAddress, useImport.Ncp);
    useImport.ArrsBalanceAsOfDate.Date = local.ProcessingDate.Date;
    useImport.NcpRecordArrsBalance.Text14 = local.NcpRecordArrsBalance.Text14;
    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    useImport.RecordType.Text2 = local.RecordType.Text2;
    useImport.NcpAddressVerified.Flag = local.NcpAddressVerified.Flag;
    useImport.CourtOrderCount.Count = local.CourtOrderCount.Count;
    useImport.ProcessDate.Date = local.ProcessingDate.Date;
    useExport.External.Assign(local.PassArea);

    Call(OeEabWriteCslnFile.Execute, useImport, useExport);

    local.PassArea.Assign(useExport.External);
  }

  private void UseOeEabWriteCslnFile4()
  {
    var useImport = new OeEabWriteCslnFile.Import();
    var useExport = new OeEabWriteCslnFile.Export();

    useImport.Cp.Assign(local.CourtOrder.Item.Cp);
    useImport.StandardCourtOrderNumb.Text20 =
      local.CourtOrder.Item.StandardCourtOrder.Text20;
    useImport.CourtOrderState.Text2 =
      local.CourtOrder.Item.CourtOrderState.Text2;
    useImport.CourtOrderCity.Text15 =
      local.CourtOrder.Item.CourtOrderCity.Text15;
    useImport.ArrsBalanceAsOfDate.Date = local.ProcessingDate.Date;
    useImport.OrganizationName.Text33 =
      local.CourtOrder.Item.CpOrganization.Text33;
    useImport.CaseNumber.Text10 =
      local.CourtOrder.Item.CourtOrderCaseNum.Text10;
    useImport.CountyFipsName.Text20 =
      local.CourtOrder.Item.CourtOrderCountyNme.Text20;
    useImport.CurrSuppWithholdingAmt.Text8 =
      local.CourtOrder.Item.CurrSupWithholdAmt.Text8;
    useImport.ArrsSupportWithAmt.Text8 = local.ArrsSupportWithAmt.Text8;
    useImport.LegalCapationLine13.Text40 =
      local.CourtOrder.Item.LegalCaptionLine13.Text40;
    useImport.LegalCaptionLine7.Text40 =
      local.CourtOrder.Item.LegalCaptionLine7.Text40;
    useImport.LegalCaptionLine8.Text40 =
      local.CourtOrder.Item.LegalCaptionLine8.Text40;
    useImport.LegalCaptionLine9.Text40 =
      local.CourtOrder.Item.LegalCaptionLine9.Text40;
    useImport.LegalCaptionLine10.Text40 =
      local.CourtOrder.Item.LegalCaptionLine10.Text40;
    useImport.LegalCaptionLine11.Text40 =
      local.CourtOrder.Item.LegalCaptionLine11.Text40;
    useImport.LegalCaptionLine12.Text40 =
      local.CourtOrder.Item.LegalCaptionLine12.Text40;
    useImport.LegalCaptionLine4.Text40 =
      local.CourtOrder.Item.LegalCaptionLine4.Text40;
    useImport.LegalCaptionLine5.Text40 =
      local.CourtOrder.Item.LegalCaptionLine5.Text40;
    useImport.LegalCaptionLine6.Text40 =
      local.CourtOrder.Item.LegalCaptionLine6.Text40;
    useImport.LegalCaptionLine3.Text40 =
      local.CourtOrder.Item.LegalCaptionLine3.Text40;
    useImport.LegalCaptionLine2.Text40 =
      local.CourtOrder.Item.LegalCaptionLine2.Text40;
    useImport.LegalCaptionLine1.Text40 =
      local.CourtOrder.Item.LegalCaptionLine1.Text40;
    useImport.ArrsByStandardNumber.Text12 =
      local.CourtOrder.Item.ArrearsByStandNumb.Text12;
    useImport.LocalPrepByAttyFirstName.Text12 =
      local.CourtOrder.Item.PrepAttyFirstName.Text12;
    useImport.PrepByAttyMiddleIniti.Text1 =
      local.CourtOrder.Item.PrepAttyMi.Text1;
    useImport.PrepByAttyLastName.Text17 =
      local.CourtOrder.Item.PrepAttyLastName.Text17;
    useImport.PrepByAttyCertificate.Text10 =
      local.CourtOrder.Item.PrepAttyCertificate.Text10;
    useImport.PrepByAttyAddress1.Text60 =
      local.CourtOrder.Item.PrepAttyAddr1.Text60;
    useImport.PrepByAttyAddress2.Text25 =
      local.CourtOrder.Item.PrepAttyAddr2.Text25;
    useImport.PrepByAttyCity.Text15 = local.CourtOrder.Item.PrepAttyCity.Text15;
    useImport.PrepByAttyState.Text2 = local.CourtOrder.Item.PrepAttyState.Text2;
    useImport.PrepByAttyPhone.Text10 =
      local.CourtOrder.Item.PrepAttyPhone.Text10;
    useImport.PrepByAttyFax.Text10 = local.CourtOrder.Item.PrepAttyFax.Text10;
    useImport.PrepByAttyEmail.Text50 =
      local.CourtOrder.Item.PrepAttyEmail.Text50;
    useImport.ArrsStdNmbrArrsAsOf.Date =
      local.CourtOrder.Item.ArrearsAsOfDate.Date;
    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    useImport.RecordType.Text2 = local.RecordType.Text2;
    useImport.ProcessDate.Date = local.ProcessingDate.Date;
    useImport.PrepByAttyZip.Text9 = local.PrepByAttyZip.Text9;
    useExport.External.Assign(local.PassArea);

    Call(OeEabWriteCslnFile.Execute, useImport, useExport);

    local.PassArea.Assign(useExport.External);
  }

  private void UseOeEabWriteCslnFile5()
  {
    var useImport = new OeEabWriteCslnFile.Import();
    var useExport = new OeEabWriteCslnFile.Export();

    useImport.Cp.Assign(local.CourtOrder.Item.Cp);
    useImport.StandardCourtOrderNumb.Text20 =
      local.CourtOrder.Item.StandardCourtOrder.Text20;
    useImport.CourtOrderState.Text2 =
      local.CourtOrder.Item.CourtOrderState.Text2;
    useImport.CourtOrderCity.Text15 =
      local.CourtOrder.Item.CourtOrderCity.Text15;
    useImport.ArrsBalanceAsOfDate.Date = local.ProcessingDate.Date;
    useImport.OrganizationName.Text33 =
      local.CourtOrder.Item.CpOrganization.Text33;
    useImport.CaseNumber.Text10 =
      local.CourtOrder.Item.CourtOrderCaseNum.Text10;
    useImport.CountyFipsName.Text20 =
      local.CourtOrder.Item.CourtOrderCountyNme.Text20;
    useImport.CurrSuppWithholdingAmt.Text8 =
      local.CourtOrder.Item.CurrSupWithholdAmt.Text8;
    useImport.ArrsSupportWithAmt.Text8 =
      local.CourtOrder.Item.ArrearsSupWithhdAmt.Text8;
    useImport.LegalCapationLine13.Text40 =
      local.CourtOrder.Item.LegalCaptionLine13.Text40;
    useImport.LegalCaptionLine7.Text40 =
      local.CourtOrder.Item.LegalCaptionLine7.Text40;
    useImport.LegalCaptionLine8.Text40 =
      local.CourtOrder.Item.LegalCaptionLine8.Text40;
    useImport.LegalCaptionLine9.Text40 =
      local.CourtOrder.Item.LegalCaptionLine9.Text40;
    useImport.LegalCaptionLine10.Text40 =
      local.CourtOrder.Item.LegalCaptionLine10.Text40;
    useImport.LegalCaptionLine11.Text40 =
      local.CourtOrder.Item.LegalCaptionLine11.Text40;
    useImport.LegalCaptionLine12.Text40 =
      local.CourtOrder.Item.LegalCaptionLine12.Text40;
    useImport.LegalCaptionLine4.Text40 =
      local.CourtOrder.Item.LegalCaptionLine4.Text40;
    useImport.LegalCaptionLine5.Text40 =
      local.CourtOrder.Item.LegalCaptionLine5.Text40;
    useImport.LegalCaptionLine6.Text40 =
      local.CourtOrder.Item.LegalCaptionLine6.Text40;
    useImport.LegalCaptionLine3.Text40 =
      local.CourtOrder.Item.LegalCaptionLine3.Text40;
    useImport.LegalCaptionLine2.Text40 =
      local.CourtOrder.Item.LegalCaptionLine2.Text40;
    useImport.LegalCaptionLine1.Text40 =
      local.CourtOrder.Item.LegalCaptionLine1.Text40;
    useImport.ArrsByStandardNumber.Text12 =
      local.CourtOrder.Item.ArrearsByStandNumb.Text12;
    useImport.LocalPrepByAttyFirstName.Text12 =
      local.CourtOrder.Item.PrepAttyFirstName.Text12;
    useImport.PrepByAttyMiddleIniti.Text1 =
      local.CourtOrder.Item.PrepAttyMi.Text1;
    useImport.PrepByAttyLastName.Text17 =
      local.CourtOrder.Item.PrepAttyLastName.Text17;
    useImport.PrepByAttyCertificate.Text10 =
      local.CourtOrder.Item.PrepAttyCertificate.Text10;
    useImport.PrepByAttyAddress1.Text60 =
      local.CourtOrder.Item.PrepAttyAddr1.Text60;
    useImport.PrepByAttyAddress2.Text25 =
      local.CourtOrder.Item.PrepAttyAddr2.Text25;
    useImport.PrepByAttyCity.Text15 = local.CourtOrder.Item.PrepAttyCity.Text15;
    useImport.PrepByAttyState.Text2 = local.CourtOrder.Item.PrepAttyState.Text2;
    useImport.PrepByAttyPhone.Text10 =
      local.CourtOrder.Item.PrepAttyPhone.Text10;
    useImport.PrepByAttyFax.Text10 = local.CourtOrder.Item.PrepAttyFax.Text10;
    useImport.PrepByAttyEmail.Text50 =
      local.CourtOrder.Item.PrepAttyEmail.Text50;
    useImport.ArrsStdNmbrArrsAsOf.Date =
      local.CourtOrder.Item.ArrearsAsOfDate.Date;
    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    useImport.RecordType.Text2 = local.RecordType.Text2;
    useImport.ProcessDate.Date = local.ProcessingDate.Date;
    useImport.PrepByAttyZip.Text9 = local.CourtOrder.Item.PrepAttyZip.Text9;
    useExport.External.Assign(local.PassArea);

    Call(OeEabWriteCslnFile.Execute, useImport, useExport);

    local.PassArea.Assign(useExport.External);
  }

  private void UseOeEabWriteCslnFile6()
  {
    var useImport = new OeEabWriteCslnFile.Import();
    var useExport = new OeEabWriteCslnFile.Export();

    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    useImport.RecordType.Text2 = local.RecordType.Text2;
    useImport.CourtOrderCount.Count = local.CourtOrderCount.Count;
    useImport.ProcessDate.Date = local.ProcessingDate.Date;
    useExport.External.Assign(local.PassArea);

    Call(OeEabWriteCslnFile.Execute, useImport, useExport);

    local.PassArea.Assign(useExport.External);
  }

  private void UseOeEabWriteCslnFile7()
  {
    var useImport = new OeEabWriteCslnFile.Import();
    var useExport = new OeEabWriteCslnFile.Export();

    useImport.NcpAttyFirstName.Text12 =
      local.CourtOrder.Item.ChildByCourtOrder.Item.ChildNcpAttyFrstNm.Text12;
    useImport.NcpAttyLastName.Text17 =
      local.CourtOrder.Item.ChildByCourtOrder.Item.ChildNcpAttyLstNm.Text17;

    useImport.PrivateAttorneyAddress.Assign(
      local.CourtOrder.Item.ChildByCourtOrder.Item.ChildNcpAtty);
    useImport.ChildRecord.Assign(
      local.CourtOrder.Item.ChildByCourtOrder.Item.Child);
    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    useImport.RecordType.Text2 = local.RecordType.Text2;
    useImport.ProcessDate.Date = local.ProcessingDate.Date;
    useExport.External.Assign(local.PassArea);

    Call(OeEabWriteCslnFile.Execute, useImport, useExport);

    local.PassArea.Assign(useExport.External);
  }

  private void UseOeEabWriteCslnFile8()
  {
    var useImport = new OeEabWriteCslnFile.Import();
    var useExport = new OeEabWriteCslnFile.Export();

    useImport.CslnTrailerNcpCount.Text6 = local.FileTrailerNcpCount.Text6;
    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    useImport.RecordType.Text2 = local.RecordType.Text2;
    useExport.External.Assign(local.PassArea);

    Call(OeEabWriteCslnFile.Execute, useImport, useExport);

    local.PassArea.Assign(useExport.External);
  }

  private void UseOeEabWriteCslnFile9()
  {
    var useImport = new OeEabWriteCslnFile.Import();
    var useExport = new OeEabWriteCslnFile.Export();

    useImport.CslnHdrFileName.Text30 = local.FileHeaderFileName.Text30;
    useImport.CslnHdrCreatedTmstmp.Timestamp =
      local.FileHeaderTimestamp.Timestamp;
    useImport.CslnHdrRecordType.Text2 = local.FileHeaderRecordType.Text2;
    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    useImport.RecordType.Text2 = local.RecordType.Text2;
    useExport.External.Assign(local.PassArea);

    Call(OeEabWriteCslnFile.Execute, useImport, useExport);

    local.PassArea.Assign(useExport.External);
  }

  private void UseOeEabWriteCslnFile10()
  {
    var useImport = new OeEabWriteCslnFile.Import();
    var useExport = new OeEabWriteCslnFile.Export();

    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    useExport.External.Assign(local.PassArea);

    Call(OeEabWriteCslnFile.Execute, useImport, useExport);

    local.PassArea.Assign(useExport.External);
  }

  private void UseReadPgmChkpntRestart()
  {
    var useImport = new ReadPgmChkpntRestart.Import();
    var useExport = new ReadPgmChkpntRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmChkpntRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseSiGetCsePersonMailingAddr()
  {
    var useImport = new SiGetCsePersonMailingAddr.Import();
    var useExport = new SiGetCsePersonMailingAddr.Export();

    useImport.CsePerson.Number = local.NcpCsePerson.Number;

    Call(SiGetCsePersonMailingAddr.Execute, useImport, useExport);

    local.NcpCsePersonAddress.Assign(useExport.CsePersonAddress);
  }

  private void UseSiReadCsePersonBatch1()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.NcpRecordName.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    local.NcpCsePerson.Number = useExport.CsePerson.Number;
    local.AbendData.Assign(useExport.AbendData);
    local.NcpRecordName.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSiReadCsePersonBatch2()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.Cp.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    local.Cp.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSiReadCsePersonBatch3()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number =
      local.CourtOrder.Item.ChildByCourtOrder.Item.Child.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    local.CourtOrder.Update.ChildByCourtOrder.Update.Child.Assign(
      useExport.CsePersonsWorkSet);
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    MoveInfrastructure(useExport.Infrastructure, local.Infrastructure);
  }

  private IEnumerable<bool> ReadAdministrativeActCertification()
  {
    entities.AdministrativeActCertification.Populated = false;

    return ReadEach("ReadAdministrativeActCertification",
      (db, command) =>
      {
        db.SetString(command, "caseNumber", local.Restart.CaseNumber);
      },
      (db, reader) =>
      {
        entities.AdministrativeActCertification.CpaType =
          db.GetString(reader, 0);
        entities.AdministrativeActCertification.CspNumber =
          db.GetString(reader, 1);
        entities.AdministrativeActCertification.Type1 = db.GetString(reader, 2);
        entities.AdministrativeActCertification.TakenDate =
          db.GetDate(reader, 3);
        entities.AdministrativeActCertification.OriginalAmount =
          db.GetNullableDecimal(reader, 4);
        entities.AdministrativeActCertification.CurrentAmount =
          db.GetNullableDecimal(reader, 5);
        entities.AdministrativeActCertification.CurrentAmountDate =
          db.GetNullableDate(reader, 6);
        entities.AdministrativeActCertification.DecertifiedDate =
          db.GetNullableDate(reader, 7);
        entities.AdministrativeActCertification.NotificationDate =
          db.GetNullableDate(reader, 8);
        entities.AdministrativeActCertification.CreatedBy =
          db.GetString(reader, 9);
        entities.AdministrativeActCertification.CreatedTstamp =
          db.GetDateTime(reader, 10);
        entities.AdministrativeActCertification.LastUpdatedBy =
          db.GetNullableString(reader, 11);
        entities.AdministrativeActCertification.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 12);
        entities.AdministrativeActCertification.AdcAmount =
          db.GetNullableDecimal(reader, 13);
        entities.AdministrativeActCertification.NonAdcAmount =
          db.GetNullableDecimal(reader, 14);
        entities.AdministrativeActCertification.InjuredSpouseDate =
          db.GetNullableDate(reader, 15);
        entities.AdministrativeActCertification.NotifiedBy =
          db.GetNullableString(reader, 16);
        entities.AdministrativeActCertification.DateSent =
          db.GetNullableDate(reader, 17);
        entities.AdministrativeActCertification.EtypeAdministrativeOffset =
          db.GetNullableString(reader, 18);
        entities.AdministrativeActCertification.LocalCode =
          db.GetNullableString(reader, 19);
        entities.AdministrativeActCertification.Ssn = db.GetInt32(reader, 20);
        entities.AdministrativeActCertification.CaseNumber =
          db.GetString(reader, 21);
        entities.AdministrativeActCertification.LastName =
          db.GetString(reader, 22);
        entities.AdministrativeActCertification.FirstName =
          db.GetString(reader, 23);
        entities.AdministrativeActCertification.AmountOwed =
          db.GetInt32(reader, 24);
        entities.AdministrativeActCertification.TtypeAAddNewCase =
          db.GetNullableString(reader, 25);
        entities.AdministrativeActCertification.CaseType =
          db.GetString(reader, 26);
        entities.AdministrativeActCertification.TransferState =
          db.GetNullableString(reader, 27);
        entities.AdministrativeActCertification.LocalForTransfer =
          db.GetNullableInt32(reader, 28);
        entities.AdministrativeActCertification.ProcessYear =
          db.GetNullableInt32(reader, 29);
        entities.AdministrativeActCertification.TanfCode =
          db.GetString(reader, 30);
        entities.AdministrativeActCertification.TtypeDDeleteCertification =
          db.GetNullableString(reader, 31);
        entities.AdministrativeActCertification.TtypeLChangeSubmittingState =
          db.GetNullableString(reader, 32);
        entities.AdministrativeActCertification.TtypeMModifyAmount =
          db.GetNullableString(reader, 33);
        entities.AdministrativeActCertification.TtypeRModifyExclusion =
          db.GetNullableString(reader, 34);
        entities.AdministrativeActCertification.TtypeSStatePayment =
          db.GetNullableString(reader, 35);
        entities.AdministrativeActCertification.TtypeTTransferAdminReview =
          db.GetNullableString(reader, 36);
        entities.AdministrativeActCertification.EtypeFederalRetirement =
          db.GetNullableString(reader, 37);
        entities.AdministrativeActCertification.EtypeFederalSalary =
          db.GetNullableString(reader, 38);
        entities.AdministrativeActCertification.EtypeTaxRefund =
          db.GetNullableString(reader, 39);
        entities.AdministrativeActCertification.EtypeVendorPaymentOrMisc =
          db.GetNullableString(reader, 40);
        entities.AdministrativeActCertification.EtypePassportDenial =
          db.GetNullableString(reader, 41);
        entities.AdministrativeActCertification.EtypeFinancialInstitution =
          db.GetNullableString(reader, 42);
        entities.AdministrativeActCertification.ReturnStatus =
          db.GetNullableString(reader, 43);
        entities.AdministrativeActCertification.ReturnStatusDate =
          db.GetNullableDate(reader, 44);
        entities.AdministrativeActCertification.DecertificationReason =
          db.GetNullableString(reader, 45);
        entities.AdministrativeActCertification.Populated = true;
        CheckValid<AdministrativeActCertification>("CpaType",
          entities.AdministrativeActCertification.CpaType);
        CheckValid<AdministrativeActCertification>("Type1",
          entities.AdministrativeActCertification.Type1);

        return true;
      });
  }

  private bool ReadCaseCsePerson1()
  {
    entities.Case1.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadCaseCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", local.NcpCsePerson.Number);
        db.SetString(command, "cspNumber2", local.ChildPrevious.Number);
        db.SetNullableDate(
          command, "endDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CsePerson.Type1 = db.GetString(reader, 2);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 3);
        entities.Case1.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCaseCsePerson2()
  {
    entities.Case1.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadCaseCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", local.NcpCsePerson.Number);
        db.SetString(command, "cspNumber2", local.ChildPrevious.Number);
        db.SetNullableDate(
          command, "endDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CsePerson.Type1 = db.GetString(reader, 2);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 3);
        entities.Case1.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadCourtCaption()
  {
    entities.CourtCaption.Populated = false;

    return ReadEach("ReadCourtCaption",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", local.Previous.Identifier);
      },
      (db, reader) =>
      {
        entities.CourtCaption.LgaIdentifier = db.GetInt32(reader, 0);
        entities.CourtCaption.Number = db.GetInt32(reader, 1);
        entities.CourtCaption.Line = db.GetNullableString(reader, 2);
        entities.CourtCaption.Populated = true;

        return true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.ChCsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDt",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetNullableString(
          command, "standardNo", local.Previous.StandardNumber ?? "");
        db.SetNullableString(command, "cspNumber", local.NcpRecordName.Number);
      },
      (db, reader) =>
      {
        entities.ChCsePerson.Number = db.GetString(reader, 0);
        entities.ChCsePerson.Type1 = db.GetString(reader, 1);
        entities.ChCsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.ChCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ChCsePerson.Type1);
      });
  }

  private bool ReadCsePerson2()
  {
    entities.ChCsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDt",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetNullableString(
          command, "standardNo", local.Previous.StandardNumber ?? "");
        db.SetNullableString(command, "cspNumber", local.NcpRecordName.Number);
      },
      (db, reader) =>
      {
        entities.ChCsePerson.Number = db.GetString(reader, 0);
        entities.ChCsePerson.Type1 = db.GetString(reader, 1);
        entities.ChCsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.ChCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ChCsePerson.Type1);
      });
  }

  private bool ReadCsePerson3()
  {
    entities.NcpCsePerson.Populated = false;

    return Read("ReadCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "numb", local.NcpRecordName.Number);
      },
      (db, reader) =>
      {
        entities.NcpCsePerson.Number = db.GetString(reader, 0);
        entities.NcpCsePerson.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCsePerson4()
  {
    entities.ChCsePerson.Populated = false;

    return ReadEach("ReadCsePerson4",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDt",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetNullableString(
          command, "standardNo", local.Previous.StandardNumber ?? "");
        db.SetNullableString(command, "cspNumber", local.NcpRecordName.Number);
      },
      (db, reader) =>
      {
        entities.ChCsePerson.Number = db.GetString(reader, 0);
        entities.ChCsePerson.Type1 = db.GetString(reader, 1);
        entities.ChCsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.ChCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ChCsePerson.Type1);

        return true;
      });
  }

  private bool ReadCsePersonCase1()
  {
    entities.Case1.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadCsePersonCase1",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDt",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetNullableString(
          command, "standardNo", local.Previous.StandardNumber ?? "");
        db.SetNullableString(command, "cspNumber1", local.ChildPrevious.Number);
        db.SetString(command, "cspNumber2", local.NcpCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.Case1.Number = db.GetString(reader, 3);
        entities.Case1.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePersonCase2()
  {
    entities.Case1.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadCsePersonCase2",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDt",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTstamp",
          local.Iwo.CreatedTstamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.Case1.Number = db.GetString(reader, 3);
        entities.Case1.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadDebtDetailObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationType.Populated = false;
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadDebtDetailObligationType",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "retiredDt", local.NullDate.Date.GetValueOrDefault());
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 8);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 9);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 10);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 11);
        entities.ObligationType.Classification = db.GetString(reader, 12);
        entities.ObligationType.Populated = true;
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", local.Previous.StandardNumber ?? "");
        db.SetNullableString(command, "cspNumber", local.NcpCsePerson.Number);
        db.SetDate(
          command, "effectiveDt",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.InitiatingState = db.GetNullableString(reader, 3);
        entities.LegalAction.InitiatingCounty = db.GetNullableString(reader, 4);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 5);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 6);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 7);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 8);
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadObligationLegalActionObligationType()
  {
    entities.ObligationType.Populated = false;
    entities.Obligation.Populated = false;
    entities.LegalAction.Populated = false;

    return ReadEach("ReadObligationLegalActionObligationType",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", local.NcpRecordName.Number);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.LegalAction.Identifier = db.GetInt32(reader, 4);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 5);
        entities.Obligation.AsOfDtNadArrBal = db.GetNullableDecimal(reader, 6);
        entities.Obligation.AsOfDtAdcArrBal = db.GetNullableDecimal(reader, 7);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 8);
        entities.LegalAction.Classification = db.GetString(reader, 9);
        entities.LegalAction.ActionTaken = db.GetString(reader, 10);
        entities.LegalAction.InitiatingState = db.GetNullableString(reader, 11);
        entities.LegalAction.InitiatingCounty =
          db.GetNullableString(reader, 12);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 13);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 14);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 15);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 16);
        entities.ObligationType.Classification = db.GetString(reader, 17);
        entities.ObligationType.Populated = true;
        entities.Obligation.Populated = true;
        entities.LegalAction.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);

        return true;
      });
  }

  private bool ReadPersonPrivateAttorney1()
  {
    entities.PersonPrivateAttorney.Populated = false;

    return Read("ReadPersonPrivateAttorney1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", local.NcpRecordName.Number);
        db.SetString(command, "casNumber", local.Read.Number);
        db.
          SetDate(command, "dateDismissed", local.Max.Date.GetValueOrDefault());
          
        db.SetNullableString(
          command, "courtCaseNumber", local.LegalAction.CourtCaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.PersonPrivateAttorney.CspNumber = db.GetString(reader, 0);
        entities.PersonPrivateAttorney.Identifier = db.GetInt32(reader, 1);
        entities.PersonPrivateAttorney.CasNumber =
          db.GetNullableString(reader, 2);
        entities.PersonPrivateAttorney.DateDismissed = db.GetDate(reader, 3);
        entities.PersonPrivateAttorney.LastName =
          db.GetNullableString(reader, 4);
        entities.PersonPrivateAttorney.FirstName =
          db.GetNullableString(reader, 5);
        entities.PersonPrivateAttorney.MiddleInitial =
          db.GetNullableString(reader, 6);
        entities.PersonPrivateAttorney.CourtCaseNumber =
          db.GetNullableString(reader, 7);
        entities.PersonPrivateAttorney.Populated = true;
      });
  }

  private bool ReadPersonPrivateAttorney2()
  {
    entities.PersonPrivateAttorney.Populated = false;

    return Read("ReadPersonPrivateAttorney2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", local.NcpRecordName.Number);
        db.SetString(command, "casNumber", local.Read.Number);
        db.
          SetDate(command, "dateDismissed", local.Max.Date.GetValueOrDefault());
          
        db.SetNullableString(
          command, "courtCaseNumber", local.Previous.CourtCaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.PersonPrivateAttorney.CspNumber = db.GetString(reader, 0);
        entities.PersonPrivateAttorney.Identifier = db.GetInt32(reader, 1);
        entities.PersonPrivateAttorney.CasNumber =
          db.GetNullableString(reader, 2);
        entities.PersonPrivateAttorney.DateDismissed = db.GetDate(reader, 3);
        entities.PersonPrivateAttorney.LastName =
          db.GetNullableString(reader, 4);
        entities.PersonPrivateAttorney.FirstName =
          db.GetNullableString(reader, 5);
        entities.PersonPrivateAttorney.MiddleInitial =
          db.GetNullableString(reader, 6);
        entities.PersonPrivateAttorney.CourtCaseNumber =
          db.GetNullableString(reader, 7);
        entities.PersonPrivateAttorney.Populated = true;
      });
  }

  private bool ReadPrivateAttorneyAddress()
  {
    System.Diagnostics.Debug.Assert(entities.PersonPrivateAttorney.Populated);
    entities.PrivateAttorneyAddress.Populated = false;

    return Read("ReadPrivateAttorneyAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "ppaIdentifier", entities.PersonPrivateAttorney.Identifier);
        db.SetString(
          command, "cspNumber", entities.PersonPrivateAttorney.CspNumber);
      },
      (db, reader) =>
      {
        entities.PrivateAttorneyAddress.PpaIdentifier = db.GetInt32(reader, 0);
        entities.PrivateAttorneyAddress.CspNumber = db.GetString(reader, 1);
        entities.PrivateAttorneyAddress.EffectiveDate = db.GetDate(reader, 2);
        entities.PrivateAttorneyAddress.Street1 =
          db.GetNullableString(reader, 3);
        entities.PrivateAttorneyAddress.Street2 =
          db.GetNullableString(reader, 4);
        entities.PrivateAttorneyAddress.City = db.GetNullableString(reader, 5);
        entities.PrivateAttorneyAddress.State = db.GetNullableString(reader, 6);
        entities.PrivateAttorneyAddress.ZipCode5 =
          db.GetNullableString(reader, 7);
        entities.PrivateAttorneyAddress.ZipCode4 =
          db.GetNullableString(reader, 8);
        entities.PrivateAttorneyAddress.Populated = true;
      });
  }

  private bool ReadTribunalFips()
  {
    entities.Tribunal.Populated = false;
    entities.Fips.Populated = false;

    return Read("ReadTribunalFips",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", local.Previous.Identifier);
      },
      (db, reader) =>
      {
        entities.Tribunal.Name = db.GetString(reader, 0);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 1);
        entities.Tribunal.Identifier = db.GetInt32(reader, 2);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 3);
        entities.Fips.County = db.GetInt32(reader, 3);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 4);
        entities.Fips.State = db.GetInt32(reader, 4);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 5);
        entities.Fips.StateAbbreviation = db.GetString(reader, 6);
        entities.Tribunal.Populated = true;
        entities.Fips.Populated = true;
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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of NeededToOpen.
    /// </summary>
    [JsonPropertyName("neededToOpen")]
    public EabReportSend NeededToOpen
    {
      get => neededToOpen ??= new();
      set => neededToOpen = value;
    }

    /// <summary>
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
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
    /// A value of ExternalFidmTrailer.
    /// </summary>
    [JsonPropertyName("externalFidmTrailer")]
    public ExternalFidmTrailer ExternalFidmTrailer
    {
      get => externalFidmTrailer ??= new();
      set => externalFidmTrailer = value;
    }

    /// <summary>
    /// A value of ExternalFidmHeader.
    /// </summary>
    [JsonPropertyName("externalFidmHeader")]
    public ExternalFidmHeader ExternalFidmHeader
    {
      get => externalFidmHeader ??= new();
      set => externalFidmHeader = value;
    }

    /// <summary>
    /// A value of ExternalFidmDetail.
    /// </summary>
    [JsonPropertyName("externalFidmDetail")]
    public ExternalFidmDetail ExternalFidmDetail
    {
      get => externalFidmDetail ??= new();
      set => externalFidmDetail = value;
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
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    private EabReportSend neededToOpen;
    private EabReportSend neededToWrite;
    private EabFileHandling eabFileHandling;
    private ExternalFidmTrailer externalFidmTrailer;
    private ExternalFidmHeader externalFidmHeader;
    private ExternalFidmDetail externalFidmDetail;
    private ProgramProcessingInfo programProcessingInfo;
    private External external;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A IwoCabErrorMessagesGroup group.</summary>
    [Serializable]
    public class IwoCabErrorMessagesGroup
    {
      /// <summary>
      /// A value of ErrorCourtCase.
      /// </summary>
      [JsonPropertyName("errorCourtCase")]
      public LegalAction ErrorCourtCase
      {
        get => errorCourtCase ??= new();
        set => errorCourtCase = value;
      }

      /// <summary>
      /// A value of ErrorType.
      /// </summary>
      [JsonPropertyName("errorType")]
      public WorkArea ErrorType
      {
        get => errorType ??= new();
        set => errorType = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private LegalAction errorCourtCase;
      private WorkArea errorType;
    }

    /// <summary>A IwoGroupExtractGroup group.</summary>
    [Serializable]
    public class IwoGroupExtractGroup
    {
      /// <summary>
      /// A value of MaxIwoPct.
      /// </summary>
      [JsonPropertyName("maxIwoPct")]
      public DolUiWithholding MaxIwoPct
      {
        get => maxIwoPct ??= new();
        set => maxIwoPct = value;
      }

      /// <summary>
      /// A value of CourtCase.
      /// </summary>
      [JsonPropertyName("courtCase")]
      public LegalAction CourtCase
      {
        get => courtCase ??= new();
        set => courtCase = value;
      }

      /// <summary>
      /// A value of IwoAmounts.
      /// </summary>
      [JsonPropertyName("iwoAmounts")]
      public ScreenOwedAmounts IwoAmounts
      {
        get => iwoAmounts ??= new();
        set => iwoAmounts = value;
      }

      /// <summary>
      /// A value of Client.
      /// </summary>
      [JsonPropertyName("client")]
      public CsePersonsWorkSet Client
      {
        get => client ??= new();
        set => client = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 150;

      private DolUiWithholding maxIwoPct;
      private LegalAction courtCase;
      private ScreenOwedAmounts iwoAmounts;
      private CsePersonsWorkSet client;
    }

    /// <summary>A CourtOrderGroup group.</summary>
    [Serializable]
    public class CourtOrderGroup
    {
      /// <summary>
      /// Gets a value of ChildByCourtOrder.
      /// </summary>
      [JsonIgnore]
      public Array<ChildByCourtOrderGroup> ChildByCourtOrder =>
        childByCourtOrder ??= new(ChildByCourtOrderGroup.Capacity, 0);

      /// <summary>
      /// Gets a value of ChildByCourtOrder for json serialization.
      /// </summary>
      [JsonPropertyName("childByCourtOrder")]
      [Computed]
      public IList<ChildByCourtOrderGroup> ChildByCourtOrder_Json
      {
        get => childByCourtOrder;
        set => ChildByCourtOrder.Assign(value);
      }

      /// <summary>
      /// A value of ArrearsAsOfDate.
      /// </summary>
      [JsonPropertyName("arrearsAsOfDate")]
      public DateWorkArea ArrearsAsOfDate
      {
        get => arrearsAsOfDate ??= new();
        set => arrearsAsOfDate = value;
      }

      /// <summary>
      /// A value of ArrearsByStandNumb.
      /// </summary>
      [JsonPropertyName("arrearsByStandNumb")]
      public TextWorkArea ArrearsByStandNumb
      {
        get => arrearsByStandNumb ??= new();
        set => arrearsByStandNumb = value;
      }

      /// <summary>
      /// A value of LegalCaptionLine13.
      /// </summary>
      [JsonPropertyName("legalCaptionLine13")]
      public WorkArea LegalCaptionLine13
      {
        get => legalCaptionLine13 ??= new();
        set => legalCaptionLine13 = value;
      }

      /// <summary>
      /// A value of LegalCaptionLine12.
      /// </summary>
      [JsonPropertyName("legalCaptionLine12")]
      public WorkArea LegalCaptionLine12
      {
        get => legalCaptionLine12 ??= new();
        set => legalCaptionLine12 = value;
      }

      /// <summary>
      /// A value of LegalCaptionLine11.
      /// </summary>
      [JsonPropertyName("legalCaptionLine11")]
      public WorkArea LegalCaptionLine11
      {
        get => legalCaptionLine11 ??= new();
        set => legalCaptionLine11 = value;
      }

      /// <summary>
      /// A value of LegalCaptionLine10.
      /// </summary>
      [JsonPropertyName("legalCaptionLine10")]
      public WorkArea LegalCaptionLine10
      {
        get => legalCaptionLine10 ??= new();
        set => legalCaptionLine10 = value;
      }

      /// <summary>
      /// A value of LegalCaptionLine9.
      /// </summary>
      [JsonPropertyName("legalCaptionLine9")]
      public WorkArea LegalCaptionLine9
      {
        get => legalCaptionLine9 ??= new();
        set => legalCaptionLine9 = value;
      }

      /// <summary>
      /// A value of LegalCaptionLine8.
      /// </summary>
      [JsonPropertyName("legalCaptionLine8")]
      public WorkArea LegalCaptionLine8
      {
        get => legalCaptionLine8 ??= new();
        set => legalCaptionLine8 = value;
      }

      /// <summary>
      /// A value of LegalCaptionLine7.
      /// </summary>
      [JsonPropertyName("legalCaptionLine7")]
      public WorkArea LegalCaptionLine7
      {
        get => legalCaptionLine7 ??= new();
        set => legalCaptionLine7 = value;
      }

      /// <summary>
      /// A value of LegalCaptionLine6.
      /// </summary>
      [JsonPropertyName("legalCaptionLine6")]
      public WorkArea LegalCaptionLine6
      {
        get => legalCaptionLine6 ??= new();
        set => legalCaptionLine6 = value;
      }

      /// <summary>
      /// A value of LegalCaptionLine5.
      /// </summary>
      [JsonPropertyName("legalCaptionLine5")]
      public WorkArea LegalCaptionLine5
      {
        get => legalCaptionLine5 ??= new();
        set => legalCaptionLine5 = value;
      }

      /// <summary>
      /// A value of LegalCaptionLine4.
      /// </summary>
      [JsonPropertyName("legalCaptionLine4")]
      public WorkArea LegalCaptionLine4
      {
        get => legalCaptionLine4 ??= new();
        set => legalCaptionLine4 = value;
      }

      /// <summary>
      /// A value of LegalCaptionLine3.
      /// </summary>
      [JsonPropertyName("legalCaptionLine3")]
      public WorkArea LegalCaptionLine3
      {
        get => legalCaptionLine3 ??= new();
        set => legalCaptionLine3 = value;
      }

      /// <summary>
      /// A value of LegalCaptionLine2.
      /// </summary>
      [JsonPropertyName("legalCaptionLine2")]
      public WorkArea LegalCaptionLine2
      {
        get => legalCaptionLine2 ??= new();
        set => legalCaptionLine2 = value;
      }

      /// <summary>
      /// A value of LegalCaptionLine1.
      /// </summary>
      [JsonPropertyName("legalCaptionLine1")]
      public WorkArea LegalCaptionLine1
      {
        get => legalCaptionLine1 ??= new();
        set => legalCaptionLine1 = value;
      }

      /// <summary>
      /// A value of PrepAttyEmail.
      /// </summary>
      [JsonPropertyName("prepAttyEmail")]
      public WorkArea PrepAttyEmail
      {
        get => prepAttyEmail ??= new();
        set => prepAttyEmail = value;
      }

      /// <summary>
      /// A value of PrepAttyFax.
      /// </summary>
      [JsonPropertyName("prepAttyFax")]
      public WorkArea PrepAttyFax
      {
        get => prepAttyFax ??= new();
        set => prepAttyFax = value;
      }

      /// <summary>
      /// A value of PrepAttyPhone.
      /// </summary>
      [JsonPropertyName("prepAttyPhone")]
      public WorkArea PrepAttyPhone
      {
        get => prepAttyPhone ??= new();
        set => prepAttyPhone = value;
      }

      /// <summary>
      /// A value of PrepAttyState.
      /// </summary>
      [JsonPropertyName("prepAttyState")]
      public WorkArea PrepAttyState
      {
        get => prepAttyState ??= new();
        set => prepAttyState = value;
      }

      /// <summary>
      /// A value of PrepAttyCity.
      /// </summary>
      [JsonPropertyName("prepAttyCity")]
      public WorkArea PrepAttyCity
      {
        get => prepAttyCity ??= new();
        set => prepAttyCity = value;
      }

      /// <summary>
      /// A value of PrepAttyAddr2.
      /// </summary>
      [JsonPropertyName("prepAttyAddr2")]
      public WorkArea PrepAttyAddr2
      {
        get => prepAttyAddr2 ??= new();
        set => prepAttyAddr2 = value;
      }

      /// <summary>
      /// A value of PrepAttyAddr1.
      /// </summary>
      [JsonPropertyName("prepAttyAddr1")]
      public WorkArea PrepAttyAddr1
      {
        get => prepAttyAddr1 ??= new();
        set => prepAttyAddr1 = value;
      }

      /// <summary>
      /// A value of PrepAttyCertificate.
      /// </summary>
      [JsonPropertyName("prepAttyCertificate")]
      public WorkArea PrepAttyCertificate
      {
        get => prepAttyCertificate ??= new();
        set => prepAttyCertificate = value;
      }

      /// <summary>
      /// A value of PrepAttyLastName.
      /// </summary>
      [JsonPropertyName("prepAttyLastName")]
      public WorkArea PrepAttyLastName
      {
        get => prepAttyLastName ??= new();
        set => prepAttyLastName = value;
      }

      /// <summary>
      /// A value of PrepAttyMi.
      /// </summary>
      [JsonPropertyName("prepAttyMi")]
      public WorkArea PrepAttyMi
      {
        get => prepAttyMi ??= new();
        set => prepAttyMi = value;
      }

      /// <summary>
      /// A value of PrepAttyFirstName.
      /// </summary>
      [JsonPropertyName("prepAttyFirstName")]
      public WorkArea PrepAttyFirstName
      {
        get => prepAttyFirstName ??= new();
        set => prepAttyFirstName = value;
      }

      /// <summary>
      /// A value of ArrearsSupWithhdAmt.
      /// </summary>
      [JsonPropertyName("arrearsSupWithhdAmt")]
      public WorkArea ArrearsSupWithhdAmt
      {
        get => arrearsSupWithhdAmt ??= new();
        set => arrearsSupWithhdAmt = value;
      }

      /// <summary>
      /// A value of CurrSupWithholdAmt.
      /// </summary>
      [JsonPropertyName("currSupWithholdAmt")]
      public WorkArea CurrSupWithholdAmt
      {
        get => currSupWithholdAmt ??= new();
        set => currSupWithholdAmt = value;
      }

      /// <summary>
      /// A value of CpOrganization.
      /// </summary>
      [JsonPropertyName("cpOrganization")]
      public WorkArea CpOrganization
      {
        get => cpOrganization ??= new();
        set => cpOrganization = value;
      }

      /// <summary>
      /// A value of Cp.
      /// </summary>
      [JsonPropertyName("cp")]
      public CsePersonsWorkSet Cp
      {
        get => cp ??= new();
        set => cp = value;
      }

      /// <summary>
      /// A value of CourtOrderCaseNum.
      /// </summary>
      [JsonPropertyName("courtOrderCaseNum")]
      public WorkArea CourtOrderCaseNum
      {
        get => courtOrderCaseNum ??= new();
        set => courtOrderCaseNum = value;
      }

      /// <summary>
      /// A value of CourtOrderCountyNme.
      /// </summary>
      [JsonPropertyName("courtOrderCountyNme")]
      public WorkArea CourtOrderCountyNme
      {
        get => courtOrderCountyNme ??= new();
        set => courtOrderCountyNme = value;
      }

      /// <summary>
      /// A value of CourtOrderCity.
      /// </summary>
      [JsonPropertyName("courtOrderCity")]
      public WorkArea CourtOrderCity
      {
        get => courtOrderCity ??= new();
        set => courtOrderCity = value;
      }

      /// <summary>
      /// A value of CourtOrderState.
      /// </summary>
      [JsonPropertyName("courtOrderState")]
      public WorkArea CourtOrderState
      {
        get => courtOrderState ??= new();
        set => courtOrderState = value;
      }

      /// <summary>
      /// A value of StandardCourtOrder.
      /// </summary>
      [JsonPropertyName("standardCourtOrder")]
      public WorkArea StandardCourtOrder
      {
        get => standardCourtOrder ??= new();
        set => standardCourtOrder = value;
      }

      /// <summary>
      /// A value of PrepAttyZip.
      /// </summary>
      [JsonPropertyName("prepAttyZip")]
      public WorkArea PrepAttyZip
      {
        get => prepAttyZip ??= new();
        set => prepAttyZip = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 200;

      private Array<ChildByCourtOrderGroup> childByCourtOrder;
      private DateWorkArea arrearsAsOfDate;
      private TextWorkArea arrearsByStandNumb;
      private WorkArea legalCaptionLine13;
      private WorkArea legalCaptionLine12;
      private WorkArea legalCaptionLine11;
      private WorkArea legalCaptionLine10;
      private WorkArea legalCaptionLine9;
      private WorkArea legalCaptionLine8;
      private WorkArea legalCaptionLine7;
      private WorkArea legalCaptionLine6;
      private WorkArea legalCaptionLine5;
      private WorkArea legalCaptionLine4;
      private WorkArea legalCaptionLine3;
      private WorkArea legalCaptionLine2;
      private WorkArea legalCaptionLine1;
      private WorkArea prepAttyEmail;
      private WorkArea prepAttyFax;
      private WorkArea prepAttyPhone;
      private WorkArea prepAttyState;
      private WorkArea prepAttyCity;
      private WorkArea prepAttyAddr2;
      private WorkArea prepAttyAddr1;
      private WorkArea prepAttyCertificate;
      private WorkArea prepAttyLastName;
      private WorkArea prepAttyMi;
      private WorkArea prepAttyFirstName;
      private WorkArea arrearsSupWithhdAmt;
      private WorkArea currSupWithholdAmt;
      private WorkArea cpOrganization;
      private CsePersonsWorkSet cp;
      private WorkArea courtOrderCaseNum;
      private WorkArea courtOrderCountyNme;
      private WorkArea courtOrderCity;
      private WorkArea courtOrderState;
      private WorkArea standardCourtOrder;
      private WorkArea prepAttyZip;
    }

    /// <summary>A ChildByCourtOrderGroup group.</summary>
    [Serializable]
    public class ChildByCourtOrderGroup
    {
      /// <summary>
      /// A value of ChildNcpAttyFrstNm.
      /// </summary>
      [JsonPropertyName("childNcpAttyFrstNm")]
      public WorkArea ChildNcpAttyFrstNm
      {
        get => childNcpAttyFrstNm ??= new();
        set => childNcpAttyFrstNm = value;
      }

      /// <summary>
      /// A value of ChildNcpAttyMi.
      /// </summary>
      [JsonPropertyName("childNcpAttyMi")]
      public WorkArea ChildNcpAttyMi
      {
        get => childNcpAttyMi ??= new();
        set => childNcpAttyMi = value;
      }

      /// <summary>
      /// A value of ChildNcpAttyLstNm.
      /// </summary>
      [JsonPropertyName("childNcpAttyLstNm")]
      public WorkArea ChildNcpAttyLstNm
      {
        get => childNcpAttyLstNm ??= new();
        set => childNcpAttyLstNm = value;
      }

      /// <summary>
      /// A value of ChildNcpAtty.
      /// </summary>
      [JsonPropertyName("childNcpAtty")]
      public PrivateAttorneyAddress ChildNcpAtty
      {
        get => childNcpAtty ??= new();
        set => childNcpAtty = value;
      }

      /// <summary>
      /// A value of ChildStandardNum.
      /// </summary>
      [JsonPropertyName("childStandardNum")]
      public WorkArea ChildStandardNum
      {
        get => childStandardNum ??= new();
        set => childStandardNum = value;
      }

      /// <summary>
      /// A value of Child.
      /// </summary>
      [JsonPropertyName("child")]
      public CsePersonsWorkSet Child
      {
        get => child ??= new();
        set => child = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private WorkArea childNcpAttyFrstNm;
      private WorkArea childNcpAttyMi;
      private WorkArea childNcpAttyLstNm;
      private PrivateAttorneyAddress childNcpAtty;
      private WorkArea childStandardNum;
      private CsePersonsWorkSet child;
    }

    /// <summary>
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public AdministrativeActCertification Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// Gets a value of IwoCabErrorMessages.
    /// </summary>
    [JsonIgnore]
    public Array<IwoCabErrorMessagesGroup> IwoCabErrorMessages =>
      iwoCabErrorMessages ??= new(IwoCabErrorMessagesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of IwoCabErrorMessages for json serialization.
    /// </summary>
    [JsonPropertyName("iwoCabErrorMessages")]
    [Computed]
    public IList<IwoCabErrorMessagesGroup> IwoCabErrorMessages_Json
    {
      get => iwoCabErrorMessages;
      set => IwoCabErrorMessages.Assign(value);
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
    /// A value of NcpAttorney.
    /// </summary>
    [JsonPropertyName("ncpAttorney")]
    public PersonPrivateAttorney NcpAttorney
    {
      get => ncpAttorney ??= new();
      set => ncpAttorney = value;
    }

    /// <summary>
    /// A value of PrivateAttorneyAddress.
    /// </summary>
    [JsonPropertyName("privateAttorneyAddress")]
    public PrivateAttorneyAddress PrivateAttorneyAddress
    {
      get => privateAttorneyAddress ??= new();
      set => privateAttorneyAddress = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of Iwo.
    /// </summary>
    [JsonPropertyName("iwo")]
    public LegalAction Iwo
    {
      get => iwo ??= new();
      set => iwo = value;
    }

    /// <summary>
    /// A value of IwoFound.
    /// </summary>
    [JsonPropertyName("iwoFound")]
    public Common IwoFound
    {
      get => iwoFound ??= new();
      set => iwoFound = value;
    }

    /// <summary>
    /// A value of CourtOrder1.
    /// </summary>
    [JsonPropertyName("courtOrder1")]
    public ScreenOwedAmounts CourtOrder1
    {
      get => courtOrder1 ??= new();
      set => courtOrder1 = value;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public ObligationType Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of NcpCsePerson.
    /// </summary>
    [JsonPropertyName("ncpCsePerson")]
    public CsePerson NcpCsePerson
    {
      get => ncpCsePerson ??= new();
      set => ncpCsePerson = value;
    }

    /// <summary>
    /// A value of ChildPrevious.
    /// </summary>
    [JsonPropertyName("childPrevious")]
    public CsePerson ChildPrevious
    {
      get => childPrevious ??= new();
      set => childPrevious = value;
    }

    /// <summary>
    /// A value of NcpTotal.
    /// </summary>
    [JsonPropertyName("ncpTotal")]
    public ScreenOwedAmounts NcpTotal
    {
      get => ncpTotal ??= new();
      set => ncpTotal = value;
    }

    /// <summary>
    /// A value of FinanceWorkAttributes.
    /// </summary>
    [JsonPropertyName("financeWorkAttributes")]
    public FinanceWorkAttributes FinanceWorkAttributes
    {
      get => financeWorkAttributes ??= new();
      set => financeWorkAttributes = value;
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
    /// Gets a value of IwoGroupExtract.
    /// </summary>
    [JsonIgnore]
    public Array<IwoGroupExtractGroup> IwoGroupExtract =>
      iwoGroupExtract ??= new(IwoGroupExtractGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of IwoGroupExtract for json serialization.
    /// </summary>
    [JsonPropertyName("iwoGroupExtract")]
    [Computed]
    public IList<IwoGroupExtractGroup> IwoGroupExtract_Json
    {
      get => iwoGroupExtract;
      set => IwoGroupExtract.Assign(value);
    }

    /// <summary>
    /// A value of Read.
    /// </summary>
    [JsonPropertyName("read")]
    public Case1 Read
    {
      get => read ??= new();
      set => read = value;
    }

    /// <summary>
    /// A value of ScreenOwedAmounts.
    /// </summary>
    [JsonPropertyName("screenOwedAmounts")]
    public ScreenOwedAmounts ScreenOwedAmounts
    {
      get => screenOwedAmounts ??= new();
      set => screenOwedAmounts = value;
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
    /// A value of NcpAddressVerified.
    /// </summary>
    [JsonPropertyName("ncpAddressVerified")]
    public Common NcpAddressVerified
    {
      get => ncpAddressVerified ??= new();
      set => ncpAddressVerified = value;
    }

    /// <summary>
    /// Gets a value of CourtOrder.
    /// </summary>
    [JsonIgnore]
    public Array<CourtOrderGroup> CourtOrder => courtOrder ??= new(
      CourtOrderGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of CourtOrder for json serialization.
    /// </summary>
    [JsonPropertyName("courtOrder")]
    [Computed]
    public IList<CourtOrderGroup> CourtOrder_Json
    {
      get => courtOrder;
      set => CourtOrder.Assign(value);
    }

    /// <summary>
    /// A value of ChildDelete.
    /// </summary>
    [JsonPropertyName("childDelete")]
    public CsePersonsWorkSet ChildDelete
    {
      get => childDelete ??= new();
      set => childDelete = value;
    }

    /// <summary>
    /// A value of ArrsByStandardNumber.
    /// </summary>
    [JsonPropertyName("arrsByStandardNumber")]
    public TextWorkArea ArrsByStandardNumber
    {
      get => arrsByStandardNumber ??= new();
      set => arrsByStandardNumber = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public LegalAction Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    /// <summary>
    /// A value of RecordType.
    /// </summary>
    [JsonPropertyName("recordType")]
    public WorkArea RecordType
    {
      get => recordType ??= new();
      set => recordType = value;
    }

    /// <summary>
    /// A value of PreviousNcp.
    /// </summary>
    [JsonPropertyName("previousNcp")]
    public CsePersonsWorkSet PreviousNcp
    {
      get => previousNcp ??= new();
      set => previousNcp = value;
    }

    /// <summary>
    /// A value of LastOfTheMonth.
    /// </summary>
    [JsonPropertyName("lastOfTheMonth")]
    public DateWorkArea LastOfTheMonth
    {
      get => lastOfTheMonth ??= new();
      set => lastOfTheMonth = value;
    }

    /// <summary>
    /// A value of FirstOfTheMonth.
    /// </summary>
    [JsonPropertyName("firstOfTheMonth")]
    public DateWorkArea FirstOfTheMonth
    {
      get => firstOfTheMonth ??= new();
      set => firstOfTheMonth = value;
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
    /// A value of NcpCsePersonAddress.
    /// </summary>
    [JsonPropertyName("ncpCsePersonAddress")]
    public CsePersonAddress NcpCsePersonAddress
    {
      get => ncpCsePersonAddress ??= new();
      set => ncpCsePersonAddress = value;
    }

    /// <summary>
    /// A value of Cp.
    /// </summary>
    [JsonPropertyName("cp")]
    public CsePersonsWorkSet Cp
    {
      get => cp ??= new();
      set => cp = value;
    }

    /// <summary>
    /// A value of FileTrailerNcpCount.
    /// </summary>
    [JsonPropertyName("fileTrailerNcpCount")]
    public WorkArea FileTrailerNcpCount
    {
      get => fileTrailerNcpCount ??= new();
      set => fileTrailerNcpCount = value;
    }

    /// <summary>
    /// A value of FileTrailerRecordType.
    /// </summary>
    [JsonPropertyName("fileTrailerRecordType")]
    public WorkArea FileTrailerRecordType
    {
      get => fileTrailerRecordType ??= new();
      set => fileTrailerRecordType = value;
    }

    /// <summary>
    /// A value of FileHeaderRecordType.
    /// </summary>
    [JsonPropertyName("fileHeaderRecordType")]
    public WorkArea FileHeaderRecordType
    {
      get => fileHeaderRecordType ??= new();
      set => fileHeaderRecordType = value;
    }

    /// <summary>
    /// A value of ChildSuffix.
    /// </summary>
    [JsonPropertyName("childSuffix")]
    public WorkArea ChildSuffix
    {
      get => childSuffix ??= new();
      set => childSuffix = value;
    }

    /// <summary>
    /// A value of NcpAttyMiddleInitial.
    /// </summary>
    [JsonPropertyName("ncpAttyMiddleInitial")]
    public WorkArea NcpAttyMiddleInitial
    {
      get => ncpAttyMiddleInitial ??= new();
      set => ncpAttyMiddleInitial = value;
    }

    /// <summary>
    /// A value of NcpAttyFirstName.
    /// </summary>
    [JsonPropertyName("ncpAttyFirstName")]
    public WorkArea NcpAttyFirstName
    {
      get => ncpAttyFirstName ??= new();
      set => ncpAttyFirstName = value;
    }

    /// <summary>
    /// A value of NcpAttyLastName.
    /// </summary>
    [JsonPropertyName("ncpAttyLastName")]
    public WorkArea NcpAttyLastName
    {
      get => ncpAttyLastName ??= new();
      set => ncpAttyLastName = value;
    }

    /// <summary>
    /// A value of ArrsStdNumberArrsAsOf.
    /// </summary>
    [JsonPropertyName("arrsStdNumberArrsAsOf")]
    public DateWorkArea ArrsStdNumberArrsAsOf
    {
      get => arrsStdNumberArrsAsOf ??= new();
      set => arrsStdNumberArrsAsOf = value;
    }

    /// <summary>
    /// A value of PrepByAttyEmail.
    /// </summary>
    [JsonPropertyName("prepByAttyEmail")]
    public WorkArea PrepByAttyEmail
    {
      get => prepByAttyEmail ??= new();
      set => prepByAttyEmail = value;
    }

    /// <summary>
    /// A value of PrepByAttyFax.
    /// </summary>
    [JsonPropertyName("prepByAttyFax")]
    public WorkArea PrepByAttyFax
    {
      get => prepByAttyFax ??= new();
      set => prepByAttyFax = value;
    }

    /// <summary>
    /// A value of PrepByAttyPhone.
    /// </summary>
    [JsonPropertyName("prepByAttyPhone")]
    public WorkArea PrepByAttyPhone
    {
      get => prepByAttyPhone ??= new();
      set => prepByAttyPhone = value;
    }

    /// <summary>
    /// A value of PrepByAttyZip.
    /// </summary>
    [JsonPropertyName("prepByAttyZip")]
    public WorkArea PrepByAttyZip
    {
      get => prepByAttyZip ??= new();
      set => prepByAttyZip = value;
    }

    /// <summary>
    /// A value of PrepByAttyState.
    /// </summary>
    [JsonPropertyName("prepByAttyState")]
    public WorkArea PrepByAttyState
    {
      get => prepByAttyState ??= new();
      set => prepByAttyState = value;
    }

    /// <summary>
    /// A value of PrepByAttyCity.
    /// </summary>
    [JsonPropertyName("prepByAttyCity")]
    public WorkArea PrepByAttyCity
    {
      get => prepByAttyCity ??= new();
      set => prepByAttyCity = value;
    }

    /// <summary>
    /// A value of PrepByAttyAddress2.
    /// </summary>
    [JsonPropertyName("prepByAttyAddress2")]
    public WorkArea PrepByAttyAddress2
    {
      get => prepByAttyAddress2 ??= new();
      set => prepByAttyAddress2 = value;
    }

    /// <summary>
    /// A value of PrepByAttyAddress1.
    /// </summary>
    [JsonPropertyName("prepByAttyAddress1")]
    public WorkArea PrepByAttyAddress1
    {
      get => prepByAttyAddress1 ??= new();
      set => prepByAttyAddress1 = value;
    }

    /// <summary>
    /// A value of PrepByAttyCertificate.
    /// </summary>
    [JsonPropertyName("prepByAttyCertificate")]
    public WorkArea PrepByAttyCertificate
    {
      get => prepByAttyCertificate ??= new();
      set => prepByAttyCertificate = value;
    }

    /// <summary>
    /// A value of PrepByAttyLastName.
    /// </summary>
    [JsonPropertyName("prepByAttyLastName")]
    public WorkArea PrepByAttyLastName
    {
      get => prepByAttyLastName ??= new();
      set => prepByAttyLastName = value;
    }

    /// <summary>
    /// A value of PrepByAttyMiddleInitia.
    /// </summary>
    [JsonPropertyName("prepByAttyMiddleInitia")]
    public WorkArea PrepByAttyMiddleInitia
    {
      get => prepByAttyMiddleInitia ??= new();
      set => prepByAttyMiddleInitia = value;
    }

    /// <summary>
    /// A value of PrepByAttyFirstName.
    /// </summary>
    [JsonPropertyName("prepByAttyFirstName")]
    public WorkArea PrepByAttyFirstName
    {
      get => prepByAttyFirstName ??= new();
      set => prepByAttyFirstName = value;
    }

    /// <summary>
    /// A value of ArrearsByStandardNumber.
    /// </summary>
    [JsonPropertyName("arrearsByStandardNumber")]
    public WorkArea ArrearsByStandardNumber
    {
      get => arrearsByStandardNumber ??= new();
      set => arrearsByStandardNumber = value;
    }

    /// <summary>
    /// A value of LegalCaptionLine1.
    /// </summary>
    [JsonPropertyName("legalCaptionLine1")]
    public WorkArea LegalCaptionLine1
    {
      get => legalCaptionLine1 ??= new();
      set => legalCaptionLine1 = value;
    }

    /// <summary>
    /// A value of LegalCaptionLine2.
    /// </summary>
    [JsonPropertyName("legalCaptionLine2")]
    public WorkArea LegalCaptionLine2
    {
      get => legalCaptionLine2 ??= new();
      set => legalCaptionLine2 = value;
    }

    /// <summary>
    /// A value of LegalCaptionLine3.
    /// </summary>
    [JsonPropertyName("legalCaptionLine3")]
    public WorkArea LegalCaptionLine3
    {
      get => legalCaptionLine3 ??= new();
      set => legalCaptionLine3 = value;
    }

    /// <summary>
    /// A value of LegalCaptionLine4.
    /// </summary>
    [JsonPropertyName("legalCaptionLine4")]
    public WorkArea LegalCaptionLine4
    {
      get => legalCaptionLine4 ??= new();
      set => legalCaptionLine4 = value;
    }

    /// <summary>
    /// A value of LegalCaptionLine5.
    /// </summary>
    [JsonPropertyName("legalCaptionLine5")]
    public WorkArea LegalCaptionLine5
    {
      get => legalCaptionLine5 ??= new();
      set => legalCaptionLine5 = value;
    }

    /// <summary>
    /// A value of LegalCaptionLine6.
    /// </summary>
    [JsonPropertyName("legalCaptionLine6")]
    public WorkArea LegalCaptionLine6
    {
      get => legalCaptionLine6 ??= new();
      set => legalCaptionLine6 = value;
    }

    /// <summary>
    /// A value of LegalCaptionLine7.
    /// </summary>
    [JsonPropertyName("legalCaptionLine7")]
    public WorkArea LegalCaptionLine7
    {
      get => legalCaptionLine7 ??= new();
      set => legalCaptionLine7 = value;
    }

    /// <summary>
    /// A value of LegalCaptionLine8.
    /// </summary>
    [JsonPropertyName("legalCaptionLine8")]
    public WorkArea LegalCaptionLine8
    {
      get => legalCaptionLine8 ??= new();
      set => legalCaptionLine8 = value;
    }

    /// <summary>
    /// A value of LegalCaptionLine9.
    /// </summary>
    [JsonPropertyName("legalCaptionLine9")]
    public WorkArea LegalCaptionLine9
    {
      get => legalCaptionLine9 ??= new();
      set => legalCaptionLine9 = value;
    }

    /// <summary>
    /// A value of LegalCaptionLine10.
    /// </summary>
    [JsonPropertyName("legalCaptionLine10")]
    public WorkArea LegalCaptionLine10
    {
      get => legalCaptionLine10 ??= new();
      set => legalCaptionLine10 = value;
    }

    /// <summary>
    /// A value of LegalCaptionLine11.
    /// </summary>
    [JsonPropertyName("legalCaptionLine11")]
    public WorkArea LegalCaptionLine11
    {
      get => legalCaptionLine11 ??= new();
      set => legalCaptionLine11 = value;
    }

    /// <summary>
    /// A value of LegalCaptionLine12.
    /// </summary>
    [JsonPropertyName("legalCaptionLine12")]
    public WorkArea LegalCaptionLine12
    {
      get => legalCaptionLine12 ??= new();
      set => legalCaptionLine12 = value;
    }

    /// <summary>
    /// A value of LegalCaptionLine13.
    /// </summary>
    [JsonPropertyName("legalCaptionLine13")]
    public WorkArea LegalCaptionLine13
    {
      get => legalCaptionLine13 ??= new();
      set => legalCaptionLine13 = value;
    }

    /// <summary>
    /// A value of ArrsSupportWithAmt.
    /// </summary>
    [JsonPropertyName("arrsSupportWithAmt")]
    public WorkArea ArrsSupportWithAmt
    {
      get => arrsSupportWithAmt ??= new();
      set => arrsSupportWithAmt = value;
    }

    /// <summary>
    /// A value of CurrSuppWithholdingAmt.
    /// </summary>
    [JsonPropertyName("currSuppWithholdingAmt")]
    public WorkArea CurrSuppWithholdingAmt
    {
      get => currSuppWithholdingAmt ??= new();
      set => currSuppWithholdingAmt = value;
    }

    /// <summary>
    /// A value of CountyFipsName.
    /// </summary>
    [JsonPropertyName("countyFipsName")]
    public WorkArea CountyFipsName
    {
      get => countyFipsName ??= new();
      set => countyFipsName = value;
    }

    /// <summary>
    /// A value of CaseNumber.
    /// </summary>
    [JsonPropertyName("caseNumber")]
    public WorkArea CaseNumber
    {
      get => caseNumber ??= new();
      set => caseNumber = value;
    }

    /// <summary>
    /// A value of NcpRecordArrsBalance.
    /// </summary>
    [JsonPropertyName("ncpRecordArrsBalance")]
    public WorkArea NcpRecordArrsBalance
    {
      get => ncpRecordArrsBalance ??= new();
      set => ncpRecordArrsBalance = value;
    }

    /// <summary>
    /// A value of NcpModifier.
    /// </summary>
    [JsonPropertyName("ncpModifier")]
    public WorkArea NcpModifier
    {
      get => ncpModifier ??= new();
      set => ncpModifier = value;
    }

    /// <summary>
    /// A value of FileHeaderFileName.
    /// </summary>
    [JsonPropertyName("fileHeaderFileName")]
    public TextWorkArea FileHeaderFileName
    {
      get => fileHeaderFileName ??= new();
      set => fileHeaderFileName = value;
    }

    /// <summary>
    /// A value of CourtOrderCity.
    /// </summary>
    [JsonPropertyName("courtOrderCity")]
    public WorkArea CourtOrderCity
    {
      get => courtOrderCity ??= new();
      set => courtOrderCity = value;
    }

    /// <summary>
    /// A value of CourtOrderState.
    /// </summary>
    [JsonPropertyName("courtOrderState")]
    public TextWorkArea CourtOrderState
    {
      get => courtOrderState ??= new();
      set => courtOrderState = value;
    }

    /// <summary>
    /// A value of StandardCourtOrderNumbe.
    /// </summary>
    [JsonPropertyName("standardCourtOrderNumbe")]
    public WorkArea StandardCourtOrderNumbe
    {
      get => standardCourtOrderNumbe ??= new();
      set => standardCourtOrderNumbe = value;
    }

    /// <summary>
    /// A value of CourtOrderCount.
    /// </summary>
    [JsonPropertyName("courtOrderCount")]
    public Common CourtOrderCount
    {
      get => courtOrderCount ??= new();
      set => courtOrderCount = value;
    }

    /// <summary>
    /// A value of FileHeaderTimestamp.
    /// </summary>
    [JsonPropertyName("fileHeaderTimestamp")]
    public DateWorkArea FileHeaderTimestamp
    {
      get => fileHeaderTimestamp ??= new();
      set => fileHeaderTimestamp = value;
    }

    /// <summary>
    /// A value of NcpRecordName.
    /// </summary>
    [JsonPropertyName("ncpRecordName")]
    public CsePersonsWorkSet NcpRecordName
    {
      get => ncpRecordName ??= new();
      set => ncpRecordName = value;
    }

    /// <summary>
    /// A value of NcpCount.
    /// </summary>
    [JsonPropertyName("ncpCount")]
    public Common NcpCount
    {
      get => ncpCount ??= new();
      set => ncpCount = value;
    }

    /// <summary>
    /// A value of AdministrativeActCertification.
    /// </summary>
    [JsonPropertyName("administrativeActCertification")]
    public AdministrativeActCertification AdministrativeActCertification
    {
      get => administrativeActCertification ??= new();
      set => administrativeActCertification = value;
    }

    /// <summary>
    /// A value of TotalDollarAmount.
    /// </summary>
    [JsonPropertyName("totalDollarAmount")]
    public Common TotalDollarAmount
    {
      get => totalDollarAmount ??= new();
      set => totalDollarAmount = value;
    }

    /// <summary>
    /// A value of HeaderRecord.
    /// </summary>
    [JsonPropertyName("headerRecord")]
    public Common HeaderRecord
    {
      get => headerRecord ??= new();
      set => headerRecord = value;
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
    /// A value of FileOpened.
    /// </summary>
    [JsonPropertyName("fileOpened")]
    public Common FileOpened
    {
      get => fileOpened ??= new();
      set => fileOpened = value;
    }

    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public Common Start
    {
      get => start ??= new();
      set => start = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public Common Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of CurrentPosition.
    /// </summary>
    [JsonPropertyName("currentPosition")]
    public Common CurrentPosition
    {
      get => currentPosition ??= new();
      set => currentPosition = value;
    }

    /// <summary>
    /// A value of FieldNumber.
    /// </summary>
    [JsonPropertyName("fieldNumber")]
    public Common FieldNumber
    {
      get => fieldNumber ??= new();
      set => fieldNumber = value;
    }

    /// <summary>
    /// A value of IncludeArrearsOnly.
    /// </summary>
    [JsonPropertyName("includeArrearsOnly")]
    public Common IncludeArrearsOnly
    {
      get => includeArrearsOnly ??= new();
      set => includeArrearsOnly = value;
    }

    /// <summary>
    /// A value of Postion.
    /// </summary>
    [JsonPropertyName("postion")]
    public TextWorkArea Postion
    {
      get => postion ??= new();
      set => postion = value;
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
    /// A value of MinimumAmountOwed.
    /// </summary>
    [JsonPropertyName("minimumAmountOwed")]
    public Common MinimumAmountOwed
    {
      get => minimumAmountOwed ??= new();
      set => minimumAmountOwed = value;
    }

    /// <summary>
    /// A value of NumberOfDays.
    /// </summary>
    [JsonPropertyName("numberOfDays")]
    public Common NumberOfDays
    {
      get => numberOfDays ??= new();
      set => numberOfDays = value;
    }

    /// <summary>
    /// A value of ArrearsOnly.
    /// </summary>
    [JsonPropertyName("arrearsOnly")]
    public Common ArrearsOnly
    {
      get => arrearsOnly ??= new();
      set => arrearsOnly = value;
    }

    private AdministrativeActCertification restart;
    private Array<IwoCabErrorMessagesGroup> iwoCabErrorMessages;
    private LegalAction legalAction;
    private PersonPrivateAttorney ncpAttorney;
    private PrivateAttorneyAddress privateAttorneyAddress;
    private DateWorkArea max;
    private DateWorkArea null1;
    private LegalAction iwo;
    private Common iwoFound;
    private ScreenOwedAmounts courtOrder1;
    private ObligationType prev;
    private CsePerson ncpCsePerson;
    private CsePerson childPrevious;
    private ScreenOwedAmounts ncpTotal;
    private FinanceWorkAttributes financeWorkAttributes;
    private Infrastructure infrastructure;
    private Array<IwoGroupExtractGroup> iwoGroupExtract;
    private Case1 read;
    private ScreenOwedAmounts screenOwedAmounts;
    private DateWorkArea nullDate;
    private Common ncpAddressVerified;
    private Array<CourtOrderGroup> courtOrder;
    private CsePersonsWorkSet childDelete;
    private TextWorkArea arrsByStandardNumber;
    private LegalAction previous;
    private AbendData abendData;
    private WorkArea recordType;
    private CsePersonsWorkSet previousNcp;
    private DateWorkArea lastOfTheMonth;
    private DateWorkArea firstOfTheMonth;
    private DateWorkArea processingDate;
    private CsePersonAddress ncpCsePersonAddress;
    private CsePersonsWorkSet cp;
    private WorkArea fileTrailerNcpCount;
    private WorkArea fileTrailerRecordType;
    private WorkArea fileHeaderRecordType;
    private WorkArea childSuffix;
    private WorkArea ncpAttyMiddleInitial;
    private WorkArea ncpAttyFirstName;
    private WorkArea ncpAttyLastName;
    private DateWorkArea arrsStdNumberArrsAsOf;
    private WorkArea prepByAttyEmail;
    private WorkArea prepByAttyFax;
    private WorkArea prepByAttyPhone;
    private WorkArea prepByAttyZip;
    private WorkArea prepByAttyState;
    private WorkArea prepByAttyCity;
    private WorkArea prepByAttyAddress2;
    private WorkArea prepByAttyAddress1;
    private WorkArea prepByAttyCertificate;
    private WorkArea prepByAttyLastName;
    private WorkArea prepByAttyMiddleInitia;
    private WorkArea prepByAttyFirstName;
    private WorkArea arrearsByStandardNumber;
    private WorkArea legalCaptionLine1;
    private WorkArea legalCaptionLine2;
    private WorkArea legalCaptionLine3;
    private WorkArea legalCaptionLine4;
    private WorkArea legalCaptionLine5;
    private WorkArea legalCaptionLine6;
    private WorkArea legalCaptionLine7;
    private WorkArea legalCaptionLine8;
    private WorkArea legalCaptionLine9;
    private WorkArea legalCaptionLine10;
    private WorkArea legalCaptionLine11;
    private WorkArea legalCaptionLine12;
    private WorkArea legalCaptionLine13;
    private WorkArea arrsSupportWithAmt;
    private WorkArea currSuppWithholdingAmt;
    private WorkArea countyFipsName;
    private WorkArea caseNumber;
    private WorkArea ncpRecordArrsBalance;
    private WorkArea ncpModifier;
    private TextWorkArea fileHeaderFileName;
    private WorkArea courtOrderCity;
    private TextWorkArea courtOrderState;
    private WorkArea standardCourtOrderNumbe;
    private Common courtOrderCount;
    private DateWorkArea fileHeaderTimestamp;
    private CsePersonsWorkSet ncpRecordName;
    private Common ncpCount;
    private AdministrativeActCertification administrativeActCertification;
    private Common totalDollarAmount;
    private Common headerRecord;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private External passArea;
    private Common fileOpened;
    private Common start;
    private Common current;
    private Common currentPosition;
    private Common fieldNumber;
    private Common includeArrearsOnly;
    private TextWorkArea postion;
    private WorkArea workArea;
    private Common minimumAmountOwed;
    private Common numberOfDays;
    private Common arrearsOnly;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LaPersonLaCaseRole.
    /// </summary>
    [JsonPropertyName("laPersonLaCaseRole")]
    public LaPersonLaCaseRole LaPersonLaCaseRole
    {
      get => laPersonLaCaseRole ??= new();
      set => laPersonLaCaseRole = value;
    }

    /// <summary>
    /// A value of PrivateAttorneyAddress.
    /// </summary>
    [JsonPropertyName("privateAttorneyAddress")]
    public PrivateAttorneyAddress PrivateAttorneyAddress
    {
      get => privateAttorneyAddress ??= new();
      set => privateAttorneyAddress = value;
    }

    /// <summary>
    /// A value of NcpCaseRole.
    /// </summary>
    [JsonPropertyName("ncpCaseRole")]
    public CaseRole NcpCaseRole
    {
      get => ncpCaseRole ??= new();
      set => ncpCaseRole = value;
    }

    /// <summary>
    /// A value of ArCsePerson.
    /// </summary>
    [JsonPropertyName("arCsePerson")]
    public CsePerson ArCsePerson
    {
      get => arCsePerson ??= new();
      set => arCsePerson = value;
    }

    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of ArLegalActionPerson.
    /// </summary>
    [JsonPropertyName("arLegalActionPerson")]
    public LegalActionPerson ArLegalActionPerson
    {
      get => arLegalActionPerson ??= new();
      set => arLegalActionPerson = value;
    }

    /// <summary>
    /// A value of Parent.
    /// </summary>
    [JsonPropertyName("parent")]
    public LegalActionCaseRole Parent
    {
      get => parent ??= new();
      set => parent = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public LegalActionPerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of ChLegalActionPerson.
    /// </summary>
    [JsonPropertyName("chLegalActionPerson")]
    public LegalActionPerson ChLegalActionPerson
    {
      get => chLegalActionPerson ??= new();
      set => chLegalActionPerson = value;
    }

    /// <summary>
    /// A value of ChCaseRole.
    /// </summary>
    [JsonPropertyName("chCaseRole")]
    public CaseRole ChCaseRole
    {
      get => chCaseRole ??= new();
      set => chCaseRole = value;
    }

    /// <summary>
    /// A value of ChCsePerson.
    /// </summary>
    [JsonPropertyName("chCsePerson")]
    public CsePerson ChCsePerson
    {
      get => chCsePerson ??= new();
      set => chCsePerson = value;
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
    /// A value of NcpCsePerson.
    /// </summary>
    [JsonPropertyName("ncpCsePerson")]
    public CsePerson NcpCsePerson
    {
      get => ncpCsePerson ??= new();
      set => ncpCsePerson = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of CourtCaption.
    /// </summary>
    [JsonPropertyName("courtCaption")]
    public CourtCaption CourtCaption
    {
      get => courtCaption ??= new();
      set => courtCaption = value;
    }

    /// <summary>
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
    }

    /// <summary>
    /// A value of PersonPrivateAttorney.
    /// </summary>
    [JsonPropertyName("personPrivateAttorney")]
    public PersonPrivateAttorney PersonPrivateAttorney
    {
      get => personPrivateAttorney ??= new();
      set => personPrivateAttorney = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    /// <summary>
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public AccrualInstructions AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of AdministrativeActCertification.
    /// </summary>
    [JsonPropertyName("administrativeActCertification")]
    public AdministrativeActCertification AdministrativeActCertification
    {
      get => administrativeActCertification ??= new();
      set => administrativeActCertification = value;
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

    private LaPersonLaCaseRole laPersonLaCaseRole;
    private PrivateAttorneyAddress privateAttorneyAddress;
    private CaseRole ncpCaseRole;
    private CsePerson arCsePerson;
    private LegalActionDetail legalActionDetail;
    private LegalActionPerson arLegalActionPerson;
    private LegalActionCaseRole parent;
    private LegalActionPerson ap;
    private LegalActionPerson chLegalActionPerson;
    private CaseRole chCaseRole;
    private CsePerson chCsePerson;
    private Infrastructure infrastructure;
    private CsePerson ncpCsePerson;
    private Tribunal tribunal;
    private Fips fips;
    private LegalActionCaseRole legalActionCaseRole;
    private Case1 case1;
    private CaseRole caseRole;
    private CourtCaption courtCaption;
    private ObligationTransaction debt;
    private PersonPrivateAttorney personPrivateAttorney;
    private ObligationType obligationType;
    private Obligation obligation;
    private DebtDetail debtDetail;
    private LegalAction legalAction;
    private CsePersonAccount csePersonAccount;
    private AccrualInstructions accrualInstructions;
    private CsePersonAccount obligor;
    private CsePerson csePerson;
    private AdministrativeActCertification administrativeActCertification;
    private ProgramProcessingInfo programProcessingInfo;
  }
#endregion
}
