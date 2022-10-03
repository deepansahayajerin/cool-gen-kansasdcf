// Program: SP_B324_ATTY_PAT_EXTRACT, ID: 371209026, model: 746.
// Short name: SWEP324B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B324_ATTY_PAT_EXTRACT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SpB324AttyPatExtract: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B324_ATTY_PAT_EXTRACT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB324AttyPatExtract(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB324AttyPatExtract.
  /// </summary>
  public SpB324AttyPatExtract(IContext context, Import import, Export export):
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
    // 10/13/2003  JeHoward                       WR#030256    Paternity Report 
    // Atty Extract
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
    local.EabReportSend.ProgramName = "SWEPB324";
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
    UseEabWritePatAttyExtract1();

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
      foreach(var item1 in ReadCaseRole1())
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
            // Case currently has a legal request
            // 
            // with a status of 'O' or 'S' for referral reasons
            // 
            // of 'ADM', 'EST', or 'PAT'.
            // 
            // ***********************************************************************
            if (ReadLegalReferral())
            {
              local.LegalReferral.ReferralDate =
                entities.LegalReferral.ReferralDate;
            }
            else
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
            foreach(var item2 in ReadCaseRole2())
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
                    // Get Locate Verified Date for AP
                    // 
                    // ***********************************************************************
                    UseSiGetCsePersonMailingAddr();

                    // ***********************************************************************
                    // 
                    // Get attorney assigned to this case and the office name.
                    // 
                    // ******************************************************************
                    if (ReadOfficeOfficeServiceProviderServiceProvider())
                    {
                      local.Atty.FirstName =
                        entities.AttyServiceProvider.FirstName;
                      local.Atty.LastName =
                        entities.AttyServiceProvider.LastName;
                      local.Name.Name = entities.AttyOffice.Name;
                    }
                    else
                    {
                      // ************************************************************************
                      // 
                      // No attorney assignment found - write error and skip
                      // child.
                      // **********************************************************************
                      local.EabFileHandling.Action = "WRITE";
                      local.EabReportSend.RptDetail =
                        "No attorney assigned for case:  " + local
                        .Case1.Number;
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
                    // Get the Area Office which oversees the attorney's office.
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
                        .AttyOffice.Name;
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
                    UseEabWritePatAttyExtract2();

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
    UseEabWritePatAttyExtract1();

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

  private void UseEabWritePatAttyExtract1()
  {
    var useImport = new EabWritePatAttyExtract.Import();
    var useExport = new EabWritePatAttyExtract.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(EabWritePatAttyExtract.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabWritePatAttyExtract2()
  {
    var useImport = new EabWritePatAttyExtract.Import();
    var useExport = new EabWritePatAttyExtract.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NameArea.Name = local.Area.Name;
    useImport.Name.Name = local.Name.Name;
    MoveServiceProvider(local.Atty, useImport.Atty);
    useImport.Case1.Number = local.Case1.Number;
    useImport.Ap.Number = local.Ap.Number;
    useImport.Ch.Assign(local.ChCsePerson);
    useImport.ApName.Assign(local.ApName);
    useImport.ChName.Assign(local.ChCsePersonsWorkSet);
    useImport.Locate.VerifiedDate = local.Locate.VerifiedDate;
    useImport.LegalReferral.ReferralDate = local.LegalReferral.ReferralDate;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(EabWritePatAttyExtract.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSiGetCsePersonMailingAddr()
  {
    var useImport = new SiGetCsePersonMailingAddr.Import();
    var useExport = new SiGetCsePersonMailingAddr.Export();

    useImport.CsePerson.Number = local.Ap.Number;

    Call(SiGetCsePersonMailingAddr.Execute, useImport, useExport);

    MoveCsePersonAddress(useExport.CsePersonAddress, local.Locate);
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

  private IEnumerable<bool> ReadCaseRole2()
  {
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRole2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
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
    System.Diagnostics.Debug.Assert(entities.AttyOffice.Populated);
    entities.Area.Populated = false;

    return Read("ReadCseOrganization",
      (db, command) =>
      {
        db.SetString(
          command, "cogParentType", entities.AttyOffice.CogTypeCode ?? "");
        db.
          SetString(command, "cogParentCode", entities.AttyOffice.CogCode ?? "");
          
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
    System.Diagnostics.Debug.Assert(entities.LegalReferral.Populated);
    entities.AttyOffice.Populated = false;
    entities.AttyOfficeServiceProvider.Populated = false;
    entities.AttyServiceProvider.Populated = false;

    return Read("ReadOfficeOfficeServiceProviderServiceProvider",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetInt32(command, "lgrId", entities.LegalReferral.Identifier);
        db.SetString(command, "casNo", entities.LegalReferral.CasNumber);
        db.SetDate(command, "effectiveDate", date);
      },
      (db, reader) =>
      {
        entities.AttyOffice.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.AttyOffice.Name = db.GetString(reader, 1);
        entities.AttyOffice.CogTypeCode = db.GetNullableString(reader, 2);
        entities.AttyOffice.CogCode = db.GetNullableString(reader, 3);
        entities.AttyOffice.OffOffice = db.GetNullableInt32(reader, 4);
        entities.AttyOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 5);
        entities.AttyOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 6);
        entities.AttyOfficeServiceProvider.RoleCode = db.GetString(reader, 7);
        entities.AttyOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 8);
        entities.AttyServiceProvider.SystemGeneratedId = db.GetInt32(reader, 9);
        entities.AttyServiceProvider.LastName = db.GetString(reader, 10);
        entities.AttyServiceProvider.FirstName = db.GetString(reader, 11);
        entities.AttyOffice.Populated = true;
        entities.AttyOfficeServiceProvider.Populated = true;
        entities.AttyServiceProvider.Populated = true;
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
    /// A value of Atty.
    /// </summary>
    [JsonPropertyName("atty")]
    public ServiceProvider Atty
    {
      get => atty ??= new();
      set => atty = value;
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
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
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
    private ServiceProvider atty;
    private ServiceProvider supervisor;
    private ServiceProvider coName;
    private Common type1;
    private Case1 case1;
    private CsePerson ap;
    private CsePerson chCsePerson;
    private CsePersonsWorkSet apName;
    private CsePersonsWorkSet chCsePersonsWorkSet;
    private CsePersonAddress locate;
    private LegalReferral legalReferral;
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
    /// A value of LegalReferralAssignment.
    /// </summary>
    [JsonPropertyName("legalReferralAssignment")]
    public LegalReferralAssignment LegalReferralAssignment
    {
      get => legalReferralAssignment ??= new();
      set => legalReferralAssignment = value;
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
    /// A value of AttyOffice.
    /// </summary>
    [JsonPropertyName("attyOffice")]
    public Office AttyOffice
    {
      get => attyOffice ??= new();
      set => attyOffice = value;
    }

    /// <summary>
    /// A value of AttyOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("attyOfficeServiceProvider")]
    public OfficeServiceProvider AttyOfficeServiceProvider
    {
      get => attyOfficeServiceProvider ??= new();
      set => attyOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of AttyServiceProvider.
    /// </summary>
    [JsonPropertyName("attyServiceProvider")]
    public ServiceProvider AttyServiceProvider
    {
      get => attyServiceProvider ??= new();
      set => attyServiceProvider = value;
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
    /// A value of AttyCseOrganization.
    /// </summary>
    [JsonPropertyName("attyCseOrganization")]
    public CseOrganization AttyCseOrganization
    {
      get => attyCseOrganization ??= new();
      set => attyCseOrganization = value;
    }

    private LegalReferralAssignment legalReferralAssignment;
    private IncomeSource incomeSource;
    private LegalReferral legalReferral;
    private Case1 case1;
    private CaseRole caseRole;
    private CsePerson csePerson;
    private Office attyOffice;
    private OfficeServiceProvider attyOfficeServiceProvider;
    private ServiceProvider attyServiceProvider;
    private CaseAssignment caseAssignment;
    private ServiceProvider supervisorServiceProvider;
    private OfficeServiceProvider supervisorOfficeServiceProvider;
    private OfficeServiceProvRelationship officeServiceProvRelationship;
    private CseOrganization area;
    private CseOrganizationRelationship cseOrganizationRelationship;
    private CseOrganization attyCseOrganization;
  }
#endregion
}
