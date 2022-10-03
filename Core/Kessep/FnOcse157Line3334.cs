// Program: FN_OCSE157_LINE_33_34, ID: 371282033, model: 746.
// Short name: SWE02976
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_OCSE157_LINE_33_34.
/// </summary>
[Serializable]
public partial class FnOcse157Line3334: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_OCSE157_LINE_33_34 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnOcse157Line3334(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnOcse157Line3334.
  /// </summary>
  public FnOcse157Line3334(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------------------------------------------------
    //                                     
    // C H A N G E    L O G
    // -------------------------------------------------------------------------------------------------------------
    // Date      Developer     Request #	Description
    // --------  ----------    ----------	
    // ---------------------------------------------------------------------
    // 08/08/06  GVandy	WR00230751	Initial Development.
    // 02/04/20  GVandy	CQ66220		Beginning in FY 2022, Lines 33 and 34 are
    // 					removed from the OCSE157 report.
    // -------------------------------------------------------------------------------------------------------------
    if (import.Ocse157Verification.FiscalYear.GetValueOrDefault() >= import
      .Cq66220EffectiveFy.FiscalYear.GetValueOrDefault())
    {
      // 02/04/20 GVandy  CQ66220  Beginning in FY 2022, Lines 33 and 34 are 
      // removed from the OCSE157 report.
      return;
    }

    local.ProgramCheckpointRestart.ProgramName =
      import.ProgramCheckpointRestart.ProgramName;
    local.ForCreateOcse157Verification.FiscalYear =
      import.Ocse157Verification.FiscalYear.GetValueOrDefault();
    local.ForCreateOcse157Verification.RunNumber =
      import.Ocse157Verification.RunNumber.GetValueOrDefault();
    local.ForCreateOcse157Data.FiscalYear =
      import.Ocse157Verification.FiscalYear.GetValueOrDefault();
    local.ForCreateOcse157Data.RunNumber =
      import.Ocse157Verification.RunNumber.GetValueOrDefault();
    local.ReportEnd.Month = Month(import.ReportEndDate.Date);
    local.ReportEnd.Year = Year(import.ReportEndDate.Date);

    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
      (import.ProgramCheckpointRestart.RestartInfo, 1, 3, "33 "))
    {
      local.Restart.Number =
        Substring(import.ProgramCheckpointRestart.RestartInfo, 4, 10);

      // -------------------------------------
      // Initialize counters for lines 33 and 34
      // -------------------------------------
      local.Line33.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 14, 10));
      local.Line34.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 24, 10));
    }

    ReadOcse157Data();

    // ------------------------------------------------------
    // Read all cse_persons that meet our criteria.
    // -------------------------------------------------------
    foreach(var item in ReadCsePerson2())
    {
      MoveOcse157Verification2(local.Null1, local.ForCreateOcse157Verification);
      local.ChildIsTitle19.Flag = "N";

      // ---------------------------------------------------------------------------------------------
      // Determine if the child is a current Title 19 recipient.
      // Title 19 recipients are identified by an active CI, MA, MK, MP, or MS 
      // program
      // (i.e. program system gen id = 6, 7, 8, 10, or 11).
      // ---------------------------------------------------------------------------------------------
      foreach(var item1 in ReadPersonProgramProgram())
      {
        if (entities.Program.SystemGeneratedIdentifier == 10)
        {
          // -- If the program is MP (sys gen id = 10) then we must look at the 
          // medical subtype to determine if it is Title 19.
          //    Any subtype that begins with the letter "T" is Title 21.  Any 
          // subtype beginning with any other letter is Title 19.
          // -- Get the medical subtype from Adabas.
          UseEabReadMedicalSubtype();

          if (IsEmpty(local.AbendData.Type1))
          {
            // -- Successful Adabas read occurred.
          }
          else
          {
            export.Abort.Flag = "Y";

            if (AsChar(local.AbendData.Type1) == 'A')
            {
              // -- Unsuccessful Adabas read occurred.
              if (Equal(local.AbendData.AdabasResponseCd, "0148"))
              {
                // -- Adabas not available.
                local.EabReportSend.RptDetail = "Child " + entities
                  .Child.Number + " - Adabas error, response code = 0148.  Adabas Unavailable.";
                  
              }
              else
              {
                if (Equal(local.AbendData.AdabasFileAction, " NF"))
                {
                  // -- Case Comp record not found for the child.
                  // -- Don't abend.  Just log the not found condition to the 
                  // error report.
                  export.Abort.Flag = "N";
                  local.EabReportSend.RptDetail = "Child " + entities
                    .Child.Number + " - CASE-COMPOSITION-DBF Not Found for BENEFIT-MONTH " +
                    NumberToString(Year(import.ReportEndDate.Date), 12, 4) + NumberToString
                    (Month(import.ReportEndDate.Date), 14, 2);
                }
                else
                {
                  // -- Other Adabas error occurred.
                  local.EabReportSend.RptDetail = "Child " + entities
                    .Child.Number + " - Adabas error, response code = " + local
                    .AbendData.AdabasResponseCd + ", file action = " + local
                    .AbendData.AdabasFileAction;
                }
              }
            }
            else
            {
              // -- Action failed.
              local.EabReportSend.RptDetail = "Child " + entities
                .Child.Number + " - Unknown Adabas error, response code = " + local
                .AbendData.CicsResponseCd + ", type = " + local
                .AbendData.Type1;
            }

            local.Write.Action = "WRITE";
            UseCabErrorReport();

            if (AsChar(export.Abort.Flag) == 'Y')
            {
              ExitState = "ACO_AE0000_BATCH_ABEND";
              UseCabErrorReport();

              return;
            }

            goto Test;
          }

          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            // -- Display medical subtype returned
            local.EabReportSend.RptDetail = "*** Child " + entities
              .Child.Number + " Medical Subtypes = ";
          }

          // -- Default to Title 19 in case no subtypes were returned.
          local.ChildIsTitle19.Flag = "Y";

          // -- Check for any returned medical subtype that does not begin with 
          // the letter "T".
          for(local.G.Index = 0; local.G.Index < local.G.Count; ++local.G.Index)
          {
            if (!local.G.CheckSize())
            {
              break;
            }

            if (AsChar(import.DisplayInd.Flag) == 'Y')
            {
              // -- Display medical subtype returned
              if (local.G.Index == 0)
              {
                local.EabReportSend.RptDetail =
                  TrimEnd(local.EabReportSend.RptDetail) + " " + (
                    local.G.Item.G1.MedType ?? "");
              }
              else
              {
                local.EabReportSend.RptDetail =
                  TrimEnd(local.EabReportSend.RptDetail) + "," + (
                    local.G.Item.G1.MedType ?? "");
              }
            }

            if (CharAt(local.G.Item.G1.MedType, 1) == 'T')
            {
              local.ChildIsTitle19.Flag = "N";
            }
            else
            {
              local.ChildIsTitle19.Flag = "Y";

              break;
            }
          }

          local.G.CheckIndex();

          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            // -- Display medical subtype returned
            local.Write.Action = "WRITE";
            UseCabErrorReport();
          }

          if (AsChar(local.ChildIsTitle19.Flag) == 'Y')
          {
            break;
          }
        }
        else
        {
          local.ChildIsTitle19.Flag = "Y";

          break;
        }

Test:
        ;
      }

      if (AsChar(local.ChildIsTitle19.Flag) != 'Y')
      {
        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.ForCreateOcse157Verification.SuppPersonNumber =
            entities.Child.Number;
          local.ForCreateOcse157Verification.LineNumber = "33";
          local.ForCreateOcse157Verification.Comment =
            "Skipped-Child is not Title 19 recipient.";
          UseFnCreateOcse157Verification();
        }

        continue;
      }

      local.IncludeInLine33.Flag = "N";

      foreach(var item1 in ReadCase())
      {
        // ----------------------------------------------
        // Was this Case reported in line 1? If not, skip.
        // ----------------------------------------------
        ReadOcse157Verification();

        if (IsEmpty(local.Minimum.Number))
        {
          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.ForCreateOcse157Verification.SuppPersonNumber =
              entities.Child.Number;
            local.ForCreateOcse157Verification.CaseNumber =
              entities.Case1.Number;
            local.ForCreateOcse157Verification.LineNumber = "33";
            local.ForCreateOcse157Verification.Comment =
              "Skipped-Case not included in line 1.";
            UseFnCreateOcse157Verification();
          }

          continue;
        }

        local.IncludeInLine33.Flag = "Y";
        local.SaveForLine33.CaseNumber = entities.Case1.Number;

        // ------------------------------------------------------------------------------
        //  Determine in the child qualifies for Line 34 based on Health 
        // Insurance provided on this case.
        // ------------------------------------------------------------------------------
        // -- Find health insurance coverage for the child at any time during 
        // the fiscal year.
        foreach(var item2 in ReadPersonalHealthInsuranceHealthInsuranceCoverage())
          
        {
          // -- The health insurance must have been provided by someone on this 
          // case.
          if (!ReadCsePerson1())
          {
            continue;
          }

          ++local.Line34.Count;
          local.ForCreateOcse157Verification.LineNumber = "34";
          local.ForCreateOcse157Verification.Column = "a";
          local.ForCreateOcse157Verification.SuppPersonNumber =
            entities.Child.Number;
          local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;
          UseFnCreateOcse157Verification();

          goto ReadEach;
        }
      }

ReadEach:

      if (AsChar(local.IncludeInLine33.Flag) != 'Y')
      {
        continue;
      }

      ++local.Line33.Count;
      local.ForCreateOcse157Verification.LineNumber = "33";
      local.ForCreateOcse157Verification.Column = "a";
      local.ForCreateOcse157Verification.SuppPersonNumber =
        entities.Child.Number;
      local.ForCreateOcse157Verification.CaseNumber =
        local.SaveForLine33.CaseNumber ?? "";
      UseFnCreateOcse157Verification();
      ++local.CommitCnt.Count;

      if (local.CommitCnt.Count >= import
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        local.CommitCnt.Count = 0;
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo = "33 " + entities
          .Child.Number;
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line33.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line34.Count, 6, 10);
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.ForError.LineNumber = "33";
          local.ForError.SuppPersonNumber = entities.Child.Number;
          UseOcse157WriteError();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.Abort.Flag = "Y";

            return;
          }
        }
      }
    }

    // --------------------------------------------------
    // Processing complete for this line.
    // Create ocse157_data records and take final checkpoint.
    // --------------------------------------------------
    local.ForCreateOcse157Data.LineNumber = "33";
    local.ForCreateOcse157Data.Column = "a";
    local.ForCreateOcse157Data.Number = local.Line33.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.LineNumber = "34";
    local.ForCreateOcse157Data.Column = "a";
    local.ForCreateOcse157Data.Number = local.Line34.Count;
    UseFnCreateOcse157Data();
    local.ProgramCheckpointRestart.RestartInd = "Y";
    local.ProgramCheckpointRestart.RestartInfo = "35 " + " ";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.ForError.LineNumber = "33";
      local.ForError.CaseNumber = "";
      local.ForError.SuppPersonNumber = "";
      UseOcse157WriteError();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.Abort.Flag = "Y";
      }
    }
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Month = source.Month;
    target.Year = source.Year;
  }

  private static void MoveG1(EabReadMedicalSubtype.Export.GGroup source,
    Local.GGroup target)
  {
    target.G1.MedType = source.G1.MedType;
  }

  private static void MoveG2(Local.GGroup source,
    EabReadMedicalSubtype.Export.GGroup target)
  {
    target.G1.MedType = source.G1.MedType;
  }

  private static void MoveOcse157Verification1(Ocse157Verification source,
    Ocse157Verification target)
  {
    target.FiscalYear = source.FiscalYear;
    target.RunNumber = source.RunNumber;
    target.LineNumber = source.LineNumber;
    target.Column = source.Column;
    target.CaseNumber = source.CaseNumber;
    target.SuppPersonNumber = source.SuppPersonNumber;
    target.DtePaternityEst = source.DtePaternityEst;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.DateOfBirth = source.DateOfBirth;
    target.PlaceOfBirth = source.PlaceOfBirth;
    target.SocialSecurityNumber = source.SocialSecurityNumber;
    target.ObTypeSgi = source.ObTypeSgi;
    target.CollectionDte = source.CollectionDte;
    target.CollApplToCode = source.CollApplToCode;
    target.CaseAsinEffDte = source.CaseAsinEffDte;
    target.CaseAsinEndDte = source.CaseAsinEndDte;
    target.IntRequestIdent = source.IntRequestIdent;
    target.KansasCaseInd = source.KansasCaseInd;
    target.PersonProgCode = source.PersonProgCode;
    target.GoodCauseEffDte = source.GoodCauseEffDte;
    target.Comment = source.Comment;
  }

  private static void MoveOcse157Verification2(Ocse157Verification source,
    Ocse157Verification target)
  {
    target.LineNumber = source.LineNumber;
    target.Column = source.Column;
    target.CaseNumber = source.CaseNumber;
    target.SuppPersonNumber = source.SuppPersonNumber;
    target.DtePaternityEst = source.DtePaternityEst;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.DateOfBirth = source.DateOfBirth;
    target.PlaceOfBirth = source.PlaceOfBirth;
    target.SocialSecurityNumber = source.SocialSecurityNumber;
    target.ObTypeSgi = source.ObTypeSgi;
    target.CollectionDte = source.CollectionDte;
    target.CollApplToCode = source.CollApplToCode;
    target.CaseAsinEffDte = source.CaseAsinEffDte;
    target.CaseAsinEndDte = source.CaseAsinEndDte;
    target.IntRequestIdent = source.IntRequestIdent;
    target.KansasCaseInd = source.KansasCaseInd;
    target.PersonProgCode = source.PersonProgCode;
    target.GoodCauseEffDte = source.GoodCauseEffDte;
    target.Comment = source.Comment;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.Write.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.Write.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabReadMedicalSubtype()
  {
    var useImport = new EabReadMedicalSubtype.Import();
    var useExport = new EabReadMedicalSubtype.Export();

    useImport.CsePerson.Number = entities.Child.Number;
    MoveDateWorkArea(local.ReportEnd, useImport.End);
    MoveDateWorkArea(local.ReportEnd, useImport.Start);
    useExport.AbendData.Assign(local.AbendData);
    local.G.CopyTo(useExport.G, MoveG2);

    Call(EabReadMedicalSubtype.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    useExport.G.CopyTo(local.G, MoveG1);
  }

  private void UseFnCreateOcse157Data()
  {
    var useImport = new FnCreateOcse157Data.Import();
    var useExport = new FnCreateOcse157Data.Export();

    useImport.Ocse157Data.Assign(local.ForCreateOcse157Data);

    Call(FnCreateOcse157Data.Execute, useImport, useExport);
  }

  private void UseFnCreateOcse157Verification()
  {
    var useImport = new FnCreateOcse157Verification.Import();
    var useExport = new FnCreateOcse157Verification.Export();

    MoveOcse157Verification1(local.ForCreateOcse157Verification,
      useImport.Ocse157Verification);

    Call(FnCreateOcse157Verification.Execute, useImport, useExport);
  }

  private void UseOcse157WriteError()
  {
    var useImport = new Ocse157WriteError.Import();
    var useExport = new Ocse157WriteError.Export();

    useImport.Ocse157Verification.Assign(local.ForError);

    Call(Ocse157WriteError.Execute, useImport, useExport);
  }

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.Child.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadCsePerson1()
  {
    System.Diagnostics.Debug.Assert(entities.HealthInsuranceCoverage.Populated);
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(
          command, "numb", entities.HealthInsuranceCoverage.CspNumber ?? "");
        db.SetNullableDate(
          command, "startDate", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate", import.ReportStartDate.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCsePerson2()
  {
    entities.Child.Populated = false;

    return ReadEach("ReadCsePerson2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "suppPersonNumber1", import.From.SuppPersonNumber ?? "");
        db.SetNullableString(
          command, "suppPersonNumber2", import.To.SuppPersonNumber ?? "");
        db.SetString(command, "numb", local.Restart.Number);
      },
      (db, reader) =>
      {
        entities.Child.Number = db.GetString(reader, 0);
        entities.Child.Populated = true;

        return true;
      });
  }

  private bool ReadOcse157Data()
  {
    local.Max.Populated = false;

    return Read("ReadOcse157Data",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "fiscalYear",
          import.Ocse157Verification.FiscalYear.GetValueOrDefault());
      },
      (db, reader) =>
      {
        local.Max.RunNumber = db.GetNullableInt32(reader, 0);
        local.Max.Populated = true;
      });
  }

  private bool ReadOcse157Verification()
  {
    local.Minimum.Populated = false;

    return Read("ReadOcse157Verification",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "fiscalYear",
          import.Ocse157Verification.FiscalYear.GetValueOrDefault());
        db.SetNullableInt32(
          command, "runNumber", local.Max.RunNumber.GetValueOrDefault());
        db.SetNullableString(command, "caseNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        local.Minimum.Number = db.GetString(reader, 0);
        local.Minimum.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPersonProgramProgram()
  {
    entities.PersonProgram.Populated = false;
    entities.Program.Populated = false;

    return ReadEach("ReadPersonProgramProgram",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Child.Number);
        db.SetDate(
          command, "effectiveDate",
          import.ReportEndDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 4);
        entities.PersonProgram.Populated = true;
        entities.Program.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonalHealthInsuranceHealthInsuranceCoverage()
  {
    entities.PersonalHealthInsurance.Populated = false;
    entities.HealthInsuranceCoverage.Populated = false;

    return ReadEach("ReadPersonalHealthInsuranceHealthInsuranceCoverage",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Child.Number);
        db.SetNullableDate(
          command, "coverBeginDate",
          import.ReportEndDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "coverEndDate",
          import.ReportStartDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonalHealthInsurance.HcvId = db.GetInt64(reader, 0);
        entities.HealthInsuranceCoverage.Identifier = db.GetInt64(reader, 0);
        entities.PersonalHealthInsurance.CspNumber = db.GetString(reader, 1);
        entities.PersonalHealthInsurance.CoverageBeginDate =
          db.GetNullableDate(reader, 2);
        entities.PersonalHealthInsurance.CoverageEndDate =
          db.GetNullableDate(reader, 3);
        entities.HealthInsuranceCoverage.PolicyExpirationDate =
          db.GetNullableDate(reader, 4);
        entities.HealthInsuranceCoverage.PolicyEffectiveDate =
          db.GetNullableDate(reader, 5);
        entities.HealthInsuranceCoverage.CspNumber =
          db.GetNullableString(reader, 6);
        entities.PersonalHealthInsurance.Populated = true;
        entities.HealthInsuranceCoverage.Populated = true;

        return true;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of Ocse157Verification.
    /// </summary>
    [JsonPropertyName("ocse157Verification")]
    public Ocse157Verification Ocse157Verification
    {
      get => ocse157Verification ??= new();
      set => ocse157Verification = value;
    }

    /// <summary>
    /// A value of DisplayInd.
    /// </summary>
    [JsonPropertyName("displayInd")]
    public Common DisplayInd
    {
      get => displayInd ??= new();
      set => displayInd = value;
    }

    /// <summary>
    /// A value of ReportStartDate.
    /// </summary>
    [JsonPropertyName("reportStartDate")]
    public DateWorkArea ReportStartDate
    {
      get => reportStartDate ??= new();
      set => reportStartDate = value;
    }

    /// <summary>
    /// A value of ReportEndDate.
    /// </summary>
    [JsonPropertyName("reportEndDate")]
    public DateWorkArea ReportEndDate
    {
      get => reportEndDate ??= new();
      set => reportEndDate = value;
    }

    /// <summary>
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    public Ocse157Verification From
    {
      get => from ??= new();
      set => from = value;
    }

    /// <summary>
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public Ocse157Verification To
    {
      get => to ??= new();
      set => to = value;
    }

    /// <summary>
    /// A value of Cq66220EffectiveFy.
    /// </summary>
    [JsonPropertyName("cq66220EffectiveFy")]
    public Ocse157Verification Cq66220EffectiveFy
    {
      get => cq66220EffectiveFy ??= new();
      set => cq66220EffectiveFy = value;
    }

    private ProgramCheckpointRestart programCheckpointRestart;
    private Ocse157Verification ocse157Verification;
    private Common displayInd;
    private DateWorkArea reportStartDate;
    private DateWorkArea reportEndDate;
    private Ocse157Verification from;
    private Ocse157Verification to;
    private Ocse157Verification cq66220EffectiveFy;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Abort.
    /// </summary>
    [JsonPropertyName("abort")]
    public Common Abort
    {
      get => abort ??= new();
      set => abort = value;
    }

    private Common abort;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A GGroup group.</summary>
    [Serializable]
    public class GGroup
    {
      /// <summary>
      /// A value of G1.
      /// </summary>
      [JsonPropertyName("g1")]
      public PersonProgram G1
      {
        get => g1 ??= new();
        set => g1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private PersonProgram g1;
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
    /// A value of Write.
    /// </summary>
    [JsonPropertyName("write")]
    public EabFileHandling Write
    {
      get => write ??= new();
      set => write = value;
    }

    /// <summary>
    /// A value of SaveForLine33.
    /// </summary>
    [JsonPropertyName("saveForLine33")]
    public Ocse157Verification SaveForLine33
    {
      get => saveForLine33 ??= new();
      set => saveForLine33 = value;
    }

    /// <summary>
    /// A value of IncludeInLine33.
    /// </summary>
    [JsonPropertyName("includeInLine33")]
    public Common IncludeInLine33
    {
      get => includeInLine33 ??= new();
      set => includeInLine33 = value;
    }

    /// <summary>
    /// A value of ChildIsTitle19.
    /// </summary>
    [JsonPropertyName("childIsTitle19")]
    public Common ChildIsTitle19
    {
      get => childIsTitle19 ??= new();
      set => childIsTitle19 = value;
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
    /// Gets a value of G.
    /// </summary>
    [JsonIgnore]
    public Array<GGroup> G => g ??= new(GGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of G for json serialization.
    /// </summary>
    [JsonPropertyName("g")]
    [Computed]
    public IList<GGroup> G_Json
    {
      get => g;
      set => G.Assign(value);
    }

    /// <summary>
    /// A value of ReportEnd.
    /// </summary>
    [JsonPropertyName("reportEnd")]
    public DateWorkArea ReportEnd
    {
      get => reportEnd ??= new();
      set => reportEnd = value;
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
    /// A value of ForCreateOcse157Verification.
    /// </summary>
    [JsonPropertyName("forCreateOcse157Verification")]
    public Ocse157Verification ForCreateOcse157Verification
    {
      get => forCreateOcse157Verification ??= new();
      set => forCreateOcse157Verification = value;
    }

    /// <summary>
    /// A value of ForCreateOcse157Data.
    /// </summary>
    [JsonPropertyName("forCreateOcse157Data")]
    public Ocse157Data ForCreateOcse157Data
    {
      get => forCreateOcse157Data ??= new();
      set => forCreateOcse157Data = value;
    }

    /// <summary>
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public CsePerson Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// A value of Line33.
    /// </summary>
    [JsonPropertyName("line33")]
    public Common Line33
    {
      get => line33 ??= new();
      set => line33 = value;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public CsePerson Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public Ocse157Verification Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of ForError.
    /// </summary>
    [JsonPropertyName("forError")]
    public Ocse157Verification ForError
    {
      get => forError ??= new();
      set => forError = value;
    }

    /// <summary>
    /// A value of CommitCnt.
    /// </summary>
    [JsonPropertyName("commitCnt")]
    public Common CommitCnt
    {
      get => commitCnt ??= new();
      set => commitCnt = value;
    }

    /// <summary>
    /// A value of Line34.
    /// </summary>
    [JsonPropertyName("line34")]
    public Common Line34
    {
      get => line34 ??= new();
      set => line34 = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public Ocse157Data Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of Minimum.
    /// </summary>
    [JsonPropertyName("minimum")]
    public Case1 Minimum
    {
      get => minimum ??= new();
      set => minimum = value;
    }

    private EabReportSend eabReportSend;
    private EabFileHandling write;
    private Ocse157Verification saveForLine33;
    private Common includeInLine33;
    private Common childIsTitle19;
    private AbendData abendData;
    private Array<GGroup> g;
    private DateWorkArea reportEnd;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Ocse157Verification forCreateOcse157Verification;
    private Ocse157Data forCreateOcse157Data;
    private CsePerson restart;
    private Common line33;
    private CsePerson prev;
    private Ocse157Verification null1;
    private Ocse157Verification forError;
    private Common commitCnt;
    private Common line34;
    private Ocse157Data max;
    private Case1 minimum;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of PersonalHealthInsurance.
    /// </summary>
    [JsonPropertyName("personalHealthInsurance")]
    public PersonalHealthInsurance PersonalHealthInsurance
    {
      get => personalHealthInsurance ??= new();
      set => personalHealthInsurance = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("healthInsuranceCoverage")]
    public HealthInsuranceCoverage HealthInsuranceCoverage
    {
      get => healthInsuranceCoverage ??= new();
      set => healthInsuranceCoverage = value;
    }

    /// <summary>
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CsePerson Child
    {
      get => child ??= new();
      set => child = value;
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
    /// A value of PersonProgram.
    /// </summary>
    [JsonPropertyName("personProgram")]
    public PersonProgram PersonProgram
    {
      get => personProgram ??= new();
      set => personProgram = value;
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of Ch.
    /// </summary>
    [JsonPropertyName("ch")]
    public CaseRole Ch
    {
      get => ch ??= new();
      set => ch = value;
    }

    /// <summary>
    /// A value of Ocse157Verification.
    /// </summary>
    [JsonPropertyName("ocse157Verification")]
    public Ocse157Verification Ocse157Verification
    {
      get => ocse157Verification ??= new();
      set => ocse157Verification = value;
    }

    /// <summary>
    /// A value of Ocse157Data.
    /// </summary>
    [JsonPropertyName("ocse157Data")]
    public Ocse157Data Ocse157Data
    {
      get => ocse157Data ??= new();
      set => ocse157Data = value;
    }

    private CaseRole caseRole;
    private CsePerson csePerson;
    private PersonalHealthInsurance personalHealthInsurance;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private CsePerson child;
    private Case1 case1;
    private PersonProgram personProgram;
    private Program program;
    private CaseRole ch;
    private Ocse157Verification ocse157Verification;
    private Ocse157Data ocse157Data;
  }
#endregion
}
