// Program: SP_B322_CO_PAT_EXTRACT, ID: 371207746, model: 746.
// Short name: SWEP322B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B322_CO_PAT_EXTRACT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SpB322CoPatExtract: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B322_CO_PAT_EXTRACT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB322CoPatExtract(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB322CoPatExtract.
  /// </summary>
  public SpB322CoPatExtract(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***************************************************************************************
    // 
    // Date        Developer                      Request #            Desc
    // 
    // __________  __________________________    ___________
    // ___________________________________
    // 09/16/2003  JeHoward                       WR#030256    Paternity Report 
    // CO Extract
    // 06/03/2004  M J Quinn                      PR#206847    Exclude deceased 
    // children
    // 08/26/2011  GVandy                         CQ29124      Add reason_code =
    // 'RC' when reading
    //                                                         
    // for regional office.
    // ***************************************************************************************
    // ****************************************************************
    // 
    // Check if ADABAS is available.
    // 
    // ****************************************************************
    UseCabReadAdabasPersonBatch1();

    if (Equal(local.AbendData.AdabasResponseCd, "0148"))
    {
      ExitState = "ADABAS_UNAVAILABLE_RB";

      return;
    }
    else
    {
      ExitState = "ACO_NN0000_ALL_OK";
    }

    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = Now().Date;
    local.EabReportSend.ProgramName = "SWEPB322";
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

    // ********************************************************************
    // 
    // Open the extract file.
    // ********************************************************************
    local.EabFileHandling.Action = "OPEN";
    UseEabWritePatCoExtract1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error in opening extract file for paternity report.";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ERROR_WRITING_TO_FILE_AB";

        return;
      }

      ExitState = "FILE_OPEN_ERROR";

      return;
    }

    local.MaxDate.Date = new DateTime(2099, 12, 31);
    local.ZeroDate.Date = new DateTime(1, 1, 1);

    foreach(var item in ReadCsePerson2())
    {
      foreach(var item1 in ReadCaseRole2())
      {
        if (ReadCase())
        {
          if (AsChar(entities.Case1.Status) == 'O')
          {
            local.Area.Name = "";
            local.Name.Name = "";
            local.Supervisor.FirstName = "";
            local.Supervisor.LastName = "";
            local.CoName.FirstName = "";
            local.CoName.LastName = "";
            local.Ap.Number = "";
            local.Ap.Number = "";
            local.ApName.FirstName = "";
            local.ApName.LastName = "";
            local.ApName.MiddleInitial = "";
            local.ApName.Number = "";
            local.ApName.Sex = "";
            local.ChCsePersonsWorkSet.FirstName = "";
            local.ChCsePersonsWorkSet.LastName = "";
            local.ChCsePersonsWorkSet.MiddleInitial = "";
            local.Type1.SelectChar = "";
            local.Locate.VerifiedDate = local.ZeroDate.Date;
            local.Case1.Number = entities.Case1.Number;
            local.ChCsePerson.BirthPlaceState =
              entities.CsePerson.BirthPlaceState;
            local.ChCsePerson.BirthCertificateSignature =
              entities.CsePerson.BirthCertificateSignature;
            local.ChCsePerson.Number = entities.CsePerson.Number;
            local.ChCsePersonsWorkSet.Number = entities.CsePerson.Number;

            // ***********************************************************************
            // 
            // Case does not currently have a legal request
            // 
            // with a status of 'O' or 'S' for referral reasons
            // 
            // of 'ADM', 'EST', or 'PAT'.
            // 
            // ***********************************************************************
            if (ReadLegalReferral())
            {
              goto ReadEach;
            }

            // ***********************************************************************
            // Get Child name
            // from Adabas
            // 
            // ******************************
            UseCabReadAdabasPersonBatch3();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
            }
            else if (AsChar(local.AbendData.Type1) == 'A' && Equal
              (local.AbendData.AdabasResponseCd, "0113"))
            {
              // ***********************************************************************
              // Write
              // to the error file and skip this child.
              // 
              // ***********************************************************************
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "No Adabas record found for this child:  " + entities
                .CsePerson.Number;
              UseCabErrorReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "ERROR_WRITING_TO_FILE_AB";

                return;
              }

              ++local.TotalErrors.Count;

              goto ReadEach;
            }
            else
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Fatal error in Adabas for person number:  " + entities
                .CsePerson.Number;
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + ", Abend Type Code =";
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + local.AbendData.Type1;
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + ", Response Code";
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + local
                .AbendData.AdabasResponseCd;
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + ", File Number";
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + local
                .AbendData.AdabasFileNumber;
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + ", File Action";
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + local
                .AbendData.AdabasFileAction;
              UseCabErrorReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "ERROR_WRITING_TO_FILE_AB";

                return;
              }

              ++local.TotalErrors.Count;
            }

            // ************************************************************************
            // 
            // Find related, active, male AP.
            // ***********************************************************************
            foreach(var item2 in ReadCaseRole1())
            {
              if (ReadCsePerson1())
              {
                local.ApName.Number = entities.CsePerson.Number;
                local.Ap.Number = entities.CsePerson.Number;
                UseCabReadAdabasPersonBatch2();

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  if (AsChar(local.ApName.Sex) == 'M')
                  {
                    // ***********************************************************************
                    // 
                    // For a priority referral, the AP must have at either a
                    // verified and not ended
                    // 
                    // address or a verified income source.
                    // 
                    // For a priority locate, the AP must have NO verified
                    // address or income source.
                    // 
                    // ***********************************************************************
                    UseCabGetNotEndedAddress();

                    if (IsEmpty(local.Locate.LocationType))
                    {
                      if (ReadIncomeSource())
                      {
                        local.Type1.SelectChar = "1";
                      }
                      else if (ReadIncarceration())
                      {
                        goto ReadEach;
                      }
                      else
                      {
                        local.Type1.SelectChar = "2";
                      }
                    }
                    else
                    {
                      local.Type1.SelectChar = "1";
                    }

                    // ***********************************************************************
                    // 
                    // Get Collection Officer assigned to this case and the
                    // office name.
                    // ******************************************************************
                    if (ReadOfficeOfficeServiceProviderServiceProvider())
                    {
                      local.CoName.FirstName =
                        entities.CoServiceProvider.FirstName;
                      local.CoName.LastName =
                        entities.CoServiceProvider.LastName;
                      local.Name.Name = entities.CoOffice.Name;
                    }
                    else
                    {
                      // ************************************************************************
                      // 
                      // No CO assignment found - write error and skip child.
                      // 
                      // **********************************************************************
                      local.EabFileHandling.Action = "WRITE";
                      local.EabReportSend.RptDetail =
                        "No CO found for Case:  " + local.Case1.Number;
                      UseCabErrorReport2();

                      if (!Equal(local.EabFileHandling.Status, "OK"))
                      {
                        ExitState = "ERROR_WRITING_TO_FILE_AB";

                        return;
                      }

                      ++local.TotalErrors.Count;

                      goto ReadEach;
                    }

                    // ************************************************************************
                    // 
                    // Get the Supervisor of the CO.
                    // 
                    // ***********************************************************************
                    if (ReadServiceProviderOfficeServiceProviderOfficeServiceProvRelationship())
                      
                    {
                      local.Supervisor.FirstName =
                        entities.SupervisorServiceProvider.FirstName;
                      local.Supervisor.LastName =
                        entities.SupervisorServiceProvider.LastName;
                    }
                    else
                    {
                      // ***********************************************************************
                      // 
                      // If can't find supervisor, set to blank.
                      // 
                      // ***********************************************************************
                      local.Supervisor.FirstName = "";
                      local.Supervisor.LastName = "";
                    }

                    // ************************************************************************
                    // 
                    // Get the Area Office which oversees the CO office.
                    // 
                    // ***********************************************************************
                    // 08/26/11  GVandy CQ29124  Add reason_code = 'RC' when 
                    // reading for regional office.
                    if (ReadCseOrganization())
                    {
                      local.Area.Name = entities.Area.Name;
                    }
                    else
                    {
                      // ************************************************************************
                      // 
                      // No Area Office found - write error and skip child.
                      // 
                      // **********************************************************************
                      local.EabFileHandling.Action = "WRITE";
                      local.EabReportSend.RptDetail =
                        "No Area Office found for Office:  " + entities
                        .CoOffice.Name;
                      UseCabErrorReport2();

                      if (!Equal(local.EabFileHandling.Status, "OK"))
                      {
                        ExitState = "ERROR_WRITING_TO_FILE_AB";

                        return;
                      }

                      ++local.TotalErrors.Count;

                      goto ReadEach;
                    }

                    local.EabFileHandling.Action = "WRITE";
                    UseEabWritePatCoExtract2();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "ERROR_WRITING_TO_FILE_AB";

                      return;
                    }
                    else
                    {
                      goto ReadEach;
                    }
                  }
                  else
                  {
                    continue;
                  }
                }
                else if (AsChar(local.AbendData.Type1) == 'A' && Equal
                  (local.AbendData.AdabasResponseCd, "0113"))
                {
                  // ***********************************************************************
                  // 
                  // Write to the error file and skip this AP.
                  // 
                  // ***********************************************************************
                  local.EabFileHandling.Action = "WRITE";
                  local.EabReportSend.RptDetail =
                    "No Adabas record found for this AP:  " + entities
                    .CsePerson.Number;
                  UseCabErrorReport2();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    ExitState = "ERROR_WRITING_TO_FILE_AB";

                    return;
                  }

                  ++local.TotalErrors.Count;

                  continue;
                }
                else
                {
                  local.EabFileHandling.Action = "WRITE";
                  local.EabReportSend.RptDetail =
                    "Fatal error in Adabas for person number:  " + entities
                    .CsePerson.Number;
                  local.EabReportSend.RptDetail =
                    TrimEnd(local.EabReportSend.RptDetail) + ", Abend Type Code =";
                    
                  local.EabReportSend.RptDetail =
                    TrimEnd(local.EabReportSend.RptDetail) + local
                    .AbendData.Type1;
                  local.EabReportSend.RptDetail =
                    TrimEnd(local.EabReportSend.RptDetail) + ", Response Code";
                  local.EabReportSend.RptDetail =
                    TrimEnd(local.EabReportSend.RptDetail) + local
                    .AbendData.AdabasResponseCd;
                  local.EabReportSend.RptDetail =
                    TrimEnd(local.EabReportSend.RptDetail) + ", File Number";
                  local.EabReportSend.RptDetail =
                    TrimEnd(local.EabReportSend.RptDetail) + local
                    .AbendData.AdabasFileNumber;
                  local.EabReportSend.RptDetail =
                    TrimEnd(local.EabReportSend.RptDetail) + ", File Action";
                  local.EabReportSend.RptDetail =
                    TrimEnd(local.EabReportSend.RptDetail) + local
                    .AbendData.AdabasFileAction;
                  UseCabErrorReport2();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    ExitState = "ERROR_WRITING_TO_FILE_AB";

                    return;
                  }

                  ++local.TotalErrors.Count;
                  ExitState = "ACO_NN0000_ALL_OK";
                }
              }
              else
              {
                // ***********************************************************************
                // Write
                // to the error file and skip this AP.
                // 
                // ***********************************************************************
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "No CSE Person found for Case Role \"" + NumberToString
                  (entities.CaseRole.Identifier, 15);
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "ERROR_WRITING_TO_FILE_AB";

                  return;
                }

                ++local.TotalErrors.Count;

                continue;
              }

              goto ReadEach;
            }
          }
          else
          {
            goto ReadEach;
          }
        }
        else
        {
          // ***********************************************************************
          // Write to
          // the error file and skip this child.
          // 
          // ***********************************************************************
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "No Case found for Person Number" + entities
            .CsePerson.Number + "Case Role Number " + NumberToString
            (entities.CaseRole.Identifier, 15);
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ERROR_WRITING_TO_FILE_AB";

            return;
          }

          ++local.TotalErrors.Count;

          goto ReadEach;
        }

        goto ReadEach;
      }

ReadEach:
      ;
    }

    // ********************************************************************
    // 
    // Close the extract file.
    // 
    // ********************************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseEabWritePatCoExtract1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error in opening extract file for paternity report.";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ERROR_WRITING_TO_FILE_AB";

        return;
      }

      ExitState = "FILE_OPEN_ERROR";

      return;
    }

    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writing to control report.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
    local.Close.Number = "CLOSE";
    UseEabReadCsePersonBatch();
  }

  private static void MoveCsePersonAddress(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.VerifiedDate = source.VerifiedDate;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveServiceProvider(ServiceProvider source,
    ServiceProvider target)
  {
    target.LastName = source.LastName;
    target.FirstName = source.FirstName;
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

  private void UseCabGetNotEndedAddress()
  {
    var useImport = new CabGetNotEndedAddress.Import();
    var useExport = new CabGetNotEndedAddress.Export();

    useImport.CsePerson.Number = local.Ap.Number;

    Call(CabGetNotEndedAddress.Execute, useImport, useExport);

    MoveCsePersonAddress(useExport.CsePersonAddress, local.Locate);
  }

  private void UseCabReadAdabasPersonBatch1()
  {
    var useImport = new CabReadAdabasPersonBatch.Import();
    var useExport = new CabReadAdabasPersonBatch.Export();

    Call(CabReadAdabasPersonBatch.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseCabReadAdabasPersonBatch2()
  {
    var useImport = new CabReadAdabasPersonBatch.Import();
    var useExport = new CabReadAdabasPersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.ApName.Number;
    useImport.ErrOnAdabasUnavailable.Flag = local.ErrOnAdabasUnavailable.Flag;

    Call(CabReadAdabasPersonBatch.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    local.ApName.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseCabReadAdabasPersonBatch3()
  {
    var useImport = new CabReadAdabasPersonBatch.Import();
    var useExport = new CabReadAdabasPersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.ChCsePersonsWorkSet.Number;
    useImport.ErrOnAdabasUnavailable.Flag = local.ErrOnAdabasUnavailable.Flag;

    Call(CabReadAdabasPersonBatch.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    local.ChCsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseEabReadCsePersonBatch()
  {
    var useImport = new EabReadCsePersonBatch.Import();
    var useExport = new EabReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.Close.Number;

    Call(EabReadCsePersonBatch.Execute, useImport, useExport);
  }

  private void UseEabWritePatCoExtract1()
  {
    var useImport = new EabWritePatCoExtract.Import();
    var useExport = new EabWritePatCoExtract.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(EabWritePatCoExtract.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabWritePatCoExtract2()
  {
    var useImport = new EabWritePatCoExtract.Import();
    var useExport = new EabWritePatCoExtract.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NameArea.Name = local.Area.Name;
    useImport.Name.Name = local.Name.Name;
    MoveServiceProvider(local.Supervisor, useImport.SupervisorName);
    MoveServiceProvider(local.CoName, useImport.Co);
    useImport.Type1.SelectChar = local.Type1.SelectChar;
    useImport.Case1.Number = local.Case1.Number;
    useImport.Ap.Number = local.Ap.Number;
    useImport.Ch.Assign(local.ChCsePerson);
    useImport.ApName.Assign(local.ApName);
    useImport.ChName.Assign(local.ChCsePersonsWorkSet);
    useImport.Locate.VerifiedDate = local.Locate.VerifiedDate;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(EabWritePatCoExtract.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private bool ReadCase()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.CaseRole.CasNumber);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 3);
        entities.Case1.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseRole1()
  {
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRole1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "endDate", local.MaxDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.DateOfEmancipation = db.GetNullableDate(reader, 5);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRole2()
  {
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRole2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "endDate",
          local.EabReportSend.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.DateOfEmancipation = db.GetNullableDate(reader, 5);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
      });
  }

  private bool ReadCseOrganization()
  {
    System.Diagnostics.Debug.Assert(entities.CoOffice.Populated);
    entities.Area.Populated = false;

    return Read("ReadCseOrganization",
      (db, command) =>
      {
        db.SetString(
          command, "cogParentType", entities.CoOffice.CogTypeCode ?? "");
        db.SetString(command, "cogParentCode", entities.CoOffice.CogCode ?? "");
      },
      (db, reader) =>
      {
        entities.Area.Code = db.GetString(reader, 0);
        entities.Area.Type1 = db.GetString(reader, 1);
        entities.Area.Name = db.GetString(reader, 2);
        entities.Area.Populated = true;
      });
  }

  private bool ReadCsePerson1()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.CaseRole.CspNumber);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.CsePerson.BirthPlaceState = db.GetNullableString(reader, 3);
        entities.CsePerson.BornOutOfWedlock = db.GetNullableString(reader, 4);
        entities.CsePerson.CseToEstblPaternity =
          db.GetNullableString(reader, 5);
        entities.CsePerson.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 6);
        entities.CsePerson.DatePaternEstab = db.GetDate(reader, 7);
        entities.CsePerson.BirthCertificateSignature =
          db.GetNullableString(reader, 8);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePerson2()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "dateOfDeath", local.ZeroDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.CsePerson.BirthPlaceState = db.GetNullableString(reader, 3);
        entities.CsePerson.BornOutOfWedlock = db.GetNullableString(reader, 4);
        entities.CsePerson.CseToEstblPaternity =
          db.GetNullableString(reader, 5);
        entities.CsePerson.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 6);
        entities.CsePerson.DatePaternEstab = db.GetDate(reader, 7);
        entities.CsePerson.BirthCertificateSignature =
          db.GetNullableString(reader, 8);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private bool ReadIncarceration()
  {
    entities.Incarceration.Populated = false;

    return Read("ReadIncarceration",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetDate(command, "date1", local.MaxDate.Date.GetValueOrDefault());
        db.SetDate(command, "date2", local.ZeroDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Incarceration.CspNumber = db.GetString(reader, 0);
        entities.Incarceration.Identifier = db.GetInt32(reader, 1);
        entities.Incarceration.VerifiedDate = db.GetNullableDate(reader, 2);
        entities.Incarceration.EndDate = db.GetNullableDate(reader, 3);
        entities.Incarceration.StartDate = db.GetNullableDate(reader, 4);
        entities.Incarceration.Incarcerated = db.GetNullableString(reader, 5);
        entities.Incarceration.Populated = true;
      });
  }

  private bool ReadIncomeSource()
  {
    entities.IncomeSource.Populated = false;

    return Read("ReadIncomeSource",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "returnDt", local.ZeroDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDt", local.MaxDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.Type1 = db.GetString(reader, 1);
        entities.IncomeSource.ReturnDt = db.GetNullableDate(reader, 2);
        entities.IncomeSource.ReturnCd = db.GetNullableString(reader, 3);
        entities.IncomeSource.CspINumber = db.GetString(reader, 4);
        entities.IncomeSource.EndDt = db.GetNullableDate(reader, 5);
        entities.IncomeSource.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.IncomeSource.Type1);
      });
  }

  private bool ReadLegalReferral()
  {
    entities.LegalReferral.Populated = false;

    return Read("ReadLegalReferral",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferral.Status = db.GetNullableString(reader, 2);
        entities.LegalReferral.ReferralDate = db.GetDate(reader, 3);
        entities.LegalReferral.ReferralReason1 = db.GetString(reader, 4);
        entities.LegalReferral.ReferralReason2 = db.GetString(reader, 5);
        entities.LegalReferral.ReferralReason3 = db.GetString(reader, 6);
        entities.LegalReferral.ReferralReason4 = db.GetString(reader, 7);
        entities.LegalReferral.ReferralReason5 = db.GetString(reader, 8);
        entities.LegalReferral.Populated = true;
      });
  }

  private bool ReadOfficeOfficeServiceProviderServiceProvider()
  {
    entities.CoOffice.Populated = false;
    entities.CoOfficeServiceProvider.Populated = false;
    entities.CoServiceProvider.Populated = false;

    return Read("ReadOfficeOfficeServiceProviderServiceProvider",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetDate(command, "effectiveDate", date);
        db.SetString(command, "casNo", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CoOffice.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.CoOffice.Name = db.GetString(reader, 1);
        entities.CoOffice.CogTypeCode = db.GetNullableString(reader, 2);
        entities.CoOffice.CogCode = db.GetNullableString(reader, 3);
        entities.CoOffice.OffOffice = db.GetNullableInt32(reader, 4);
        entities.CoOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 5);
        entities.CoOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 6);
        entities.CoOfficeServiceProvider.RoleCode = db.GetString(reader, 7);
        entities.CoOfficeServiceProvider.EffectiveDate = db.GetDate(reader, 8);
        entities.CoServiceProvider.SystemGeneratedId = db.GetInt32(reader, 9);
        entities.CoServiceProvider.LastName = db.GetString(reader, 10);
        entities.CoServiceProvider.FirstName = db.GetString(reader, 11);
        entities.CoOffice.Populated = true;
        entities.CoOfficeServiceProvider.Populated = true;
        entities.CoServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProviderOfficeServiceProviderOfficeServiceProvRelationship()
    
  {
    System.Diagnostics.Debug.Assert(entities.CoOfficeServiceProvider.Populated);
    entities.SupervisorServiceProvider.Populated = false;
    entities.SupervisorOfficeServiceProvider.Populated = false;
    entities.OfficeServiceProvRelationship.Populated = false;

    return Read(
      "ReadServiceProviderOfficeServiceProviderOfficeServiceProvRelationship",
      (db, command) =>
      {
        db.SetString(
          command, "ospRoleCode", entities.CoOfficeServiceProvider.RoleCode);
        db.SetDate(
          command, "ospEffectiveDate",
          entities.CoOfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.SetInt32(
          command, "offGeneratedId",
          entities.CoOfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdGeneratedId",
          entities.CoOfficeServiceProvider.SpdGeneratedId);
      },
      (db, reader) =>
      {
        entities.SupervisorServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.SupervisorServiceProvider.LastName = db.GetString(reader, 1);
        entities.SupervisorServiceProvider.FirstName = db.GetString(reader, 2);
        entities.SupervisorOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 3);
        entities.OfficeServiceProvRelationship.SpdRGeneratedId =
          db.GetInt32(reader, 3);
        entities.SupervisorOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 4);
        entities.OfficeServiceProvRelationship.OffRGeneratedId =
          db.GetInt32(reader, 4);
        entities.SupervisorOfficeServiceProvider.RoleCode =
          db.GetString(reader, 5);
        entities.OfficeServiceProvRelationship.OspRRoleCode =
          db.GetString(reader, 5);
        entities.SupervisorOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 6);
        entities.OfficeServiceProvRelationship.OspREffectiveDt =
          db.GetDate(reader, 6);
        entities.OfficeServiceProvRelationship.OspEffectiveDate =
          db.GetDate(reader, 7);
        entities.OfficeServiceProvRelationship.OspRoleCode =
          db.GetString(reader, 8);
        entities.OfficeServiceProvRelationship.OffGeneratedId =
          db.GetInt32(reader, 9);
        entities.OfficeServiceProvRelationship.SpdGeneratedId =
          db.GetInt32(reader, 10);
        entities.OfficeServiceProvRelationship.ReasonCode =
          db.GetString(reader, 11);
        entities.SupervisorServiceProvider.Populated = true;
        entities.SupervisorOfficeServiceProvider.Populated = true;
        entities.OfficeServiceProvRelationship.Populated = true;
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
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A RolesForPersonGroup group.</summary>
    [Serializable]
    public class RolesForPersonGroup
    {
      /// <summary>
      /// A value of Role.
      /// </summary>
      [JsonPropertyName("role")]
      public CaseRole Role
      {
        get => role ??= new();
        set => role = value;
      }

      private CaseRole role;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    /// <summary>
    /// A value of ZeroDate.
    /// </summary>
    [JsonPropertyName("zeroDate")]
    public DateWorkArea ZeroDate
    {
      get => zeroDate ??= new();
      set => zeroDate = value;
    }

    /// <summary>
    /// A value of ApMale.
    /// </summary>
    [JsonPropertyName("apMale")]
    public Common ApMale
    {
      get => apMale ??= new();
      set => apMale = value;
    }

    /// <summary>
    /// Gets a value of RolesForPerson.
    /// </summary>
    [JsonPropertyName("rolesForPerson")]
    public RolesForPersonGroup RolesForPerson
    {
      get => rolesForPerson ?? (rolesForPerson = new());
      set => rolesForPerson = value;
    }

    /// <summary>
    /// A value of HoldCsePerson.
    /// </summary>
    [JsonPropertyName("holdCsePerson")]
    public CsePerson HoldCsePerson
    {
      get => holdCsePerson ??= new();
      set => holdCsePerson = value;
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
    /// A value of HoldEabReportSend.
    /// </summary>
    [JsonPropertyName("holdEabReportSend")]
    public EabReportSend HoldEabReportSend
    {
      get => holdEabReportSend ??= new();
      set => holdEabReportSend = value;
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
    /// A value of TotalErrors.
    /// </summary>
    [JsonPropertyName("totalErrors")]
    public Common TotalErrors
    {
      get => totalErrors ??= new();
      set => totalErrors = value;
    }

    /// <summary>
    /// A value of Area.
    /// </summary>
    [JsonPropertyName("area")]
    public Office Area
    {
      get => area ??= new();
      set => area = value;
    }

    /// <summary>
    /// A value of Name.
    /// </summary>
    [JsonPropertyName("name")]
    public Office Name
    {
      get => name ??= new();
      set => name = value;
    }

    /// <summary>
    /// A value of Supervisor.
    /// </summary>
    [JsonPropertyName("supervisor")]
    public ServiceProvider Supervisor
    {
      get => supervisor ??= new();
      set => supervisor = value;
    }

    /// <summary>
    /// A value of CoName.
    /// </summary>
    [JsonPropertyName("coName")]
    public ServiceProvider CoName
    {
      get => coName ??= new();
      set => coName = value;
    }

    /// <summary>
    /// A value of Type1.
    /// </summary>
    [JsonPropertyName("type1")]
    public Common Type1
    {
      get => type1 ??= new();
      set => type1 = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
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
    /// A value of ApName.
    /// </summary>
    [JsonPropertyName("apName")]
    public CsePersonsWorkSet ApName
    {
      get => apName ??= new();
      set => apName = value;
    }

    /// <summary>
    /// A value of ChCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("chCsePersonsWorkSet")]
    public CsePersonsWorkSet ChCsePersonsWorkSet
    {
      get => chCsePersonsWorkSet ??= new();
      set => chCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Locate.
    /// </summary>
    [JsonPropertyName("locate")]
    public CsePersonAddress Locate
    {
      get => locate ??= new();
      set => locate = value;
    }

    /// <summary>
    /// A value of ErrOnAdabasUnavailable.
    /// </summary>
    [JsonPropertyName("errOnAdabasUnavailable")]
    public Common ErrOnAdabasUnavailable
    {
      get => errOnAdabasUnavailable ??= new();
      set => errOnAdabasUnavailable = value;
    }

    /// <summary>
    /// A value of Close.
    /// </summary>
    [JsonPropertyName("close")]
    public CsePersonsWorkSet Close
    {
      get => close ??= new();
      set => close = value;
    }

    private DateWorkArea maxDate;
    private DateWorkArea zeroDate;
    private Common apMale;
    private RolesForPersonGroup rolesForPerson;
    private CsePerson holdCsePerson;
    private CaseRole caseRole;
    private EabReportSend holdEabReportSend;
    private AbendData abendData;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Common totalErrors;
    private Office area;
    private Office name;
    private ServiceProvider supervisor;
    private ServiceProvider coName;
    private Common type1;
    private Case1 case1;
    private CsePerson ap;
    private CsePerson chCsePerson;
    private CsePersonsWorkSet apName;
    private CsePersonsWorkSet chCsePersonsWorkSet;
    private CsePersonAddress locate;
    private Common errOnAdabasUnavailable;
    private CsePersonsWorkSet close;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Incarceration.
    /// </summary>
    [JsonPropertyName("incarceration")]
    public Incarceration Incarceration
    {
      get => incarceration ??= new();
      set => incarceration = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of CoOffice.
    /// </summary>
    [JsonPropertyName("coOffice")]
    public Office CoOffice
    {
      get => coOffice ??= new();
      set => coOffice = value;
    }

    /// <summary>
    /// A value of CoOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("coOfficeServiceProvider")]
    public OfficeServiceProvider CoOfficeServiceProvider
    {
      get => coOfficeServiceProvider ??= new();
      set => coOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of CoServiceProvider.
    /// </summary>
    [JsonPropertyName("coServiceProvider")]
    public ServiceProvider CoServiceProvider
    {
      get => coServiceProvider ??= new();
      set => coServiceProvider = value;
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
    /// A value of SupervisorServiceProvider.
    /// </summary>
    [JsonPropertyName("supervisorServiceProvider")]
    public ServiceProvider SupervisorServiceProvider
    {
      get => supervisorServiceProvider ??= new();
      set => supervisorServiceProvider = value;
    }

    /// <summary>
    /// A value of SupervisorOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("supervisorOfficeServiceProvider")]
    public OfficeServiceProvider SupervisorOfficeServiceProvider
    {
      get => supervisorOfficeServiceProvider ??= new();
      set => supervisorOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvRelationship.
    /// </summary>
    [JsonPropertyName("officeServiceProvRelationship")]
    public OfficeServiceProvRelationship OfficeServiceProvRelationship
    {
      get => officeServiceProvRelationship ??= new();
      set => officeServiceProvRelationship = value;
    }

    /// <summary>
    /// A value of Area.
    /// </summary>
    [JsonPropertyName("area")]
    public CseOrganization Area
    {
      get => area ??= new();
      set => area = value;
    }

    /// <summary>
    /// A value of CseOrganizationRelationship.
    /// </summary>
    [JsonPropertyName("cseOrganizationRelationship")]
    public CseOrganizationRelationship CseOrganizationRelationship
    {
      get => cseOrganizationRelationship ??= new();
      set => cseOrganizationRelationship = value;
    }

    /// <summary>
    /// A value of CoCseOrganization.
    /// </summary>
    [JsonPropertyName("coCseOrganization")]
    public CseOrganization CoCseOrganization
    {
      get => coCseOrganization ??= new();
      set => coCseOrganization = value;
    }

    private Incarceration incarceration;
    private IncomeSource incomeSource;
    private LegalReferral legalReferral;
    private Case1 case1;
    private CaseRole caseRole;
    private CsePerson csePerson;
    private Office coOffice;
    private OfficeServiceProvider coOfficeServiceProvider;
    private ServiceProvider coServiceProvider;
    private CaseAssignment caseAssignment;
    private ServiceProvider supervisorServiceProvider;
    private OfficeServiceProvider supervisorOfficeServiceProvider;
    private OfficeServiceProvRelationship officeServiceProvRelationship;
    private CseOrganization area;
    private CseOrganizationRelationship cseOrganizationRelationship;
    private CseOrganization coCseOrganization;
  }
#endregion
}
