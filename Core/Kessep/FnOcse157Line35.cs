// Program: FN_OCSE157_LINE_35, ID: 371283165, model: 746.
// Short name: SWE02977
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_OCSE157_LINE_35.
/// </summary>
[Serializable]
public partial class FnOcse157Line35: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_OCSE157_LINE_35 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnOcse157Line35(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnOcse157Line35.
  /// </summary>
  public FnOcse157Line35(IContext context, Import import, Export export):
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
    // 08/10/06  GVandy	WR00230751	Initial Development.
    // -------------------------------------------------------------------------------------------------------------
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

    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
      (import.ProgramCheckpointRestart.RestartInfo, 1, 3, "35 "))
    {
      local.Restart.Number =
        Substring(import.ProgramCheckpointRestart.RestartInfo, 4, 10);

      // -------------------------------------
      // Initialize counters for line 35
      // -------------------------------------
      local.Line35.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 14, 10));
    }

    ReadOcse157Data();

    foreach(var item in ReadCase())
    {
      MoveOcse157Verification2(local.Null1, local.ForCreateOcse157Verification);

      // ----------------------------------------------
      // Was this Case reported in line 1? If not, skip.
      // ----------------------------------------------
      ReadOcse157Verification();

      if (IsEmpty(local.Minimum.Number))
      {
        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.ForCreateOcse157Verification.SuppPersonNumber =
            entities.ChCsePerson.Number;
          local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;
          local.ForCreateOcse157Verification.LineNumber = "35";
          local.ForCreateOcse157Verification.Comment =
            "Skipped-Case not included in line 1.";
          UseFnCreateOcse157Verification();
        }

        continue;
      }

      local.IncludeInLine35.Flag = "N";

      // ---------------------------------------------------------------------------------------------
      // Include the case in Line 35 if any child on the case is a current Title
      // 19 or 21 recipient.
      // Title 19 recipients are identified by an active CI, MA, MK, MP, or MS 
      // program
      // (i.e. program system gen id = 6, 7, 8, 10, or 11).
      // Title 21 recipients are identified by an active MP program with a 
      // medical subtype beginning with letter "T".
      // ---------------------------------------------------------------------------------------------
      foreach(var item1 in ReadCsePerson2())
      {
        if (ReadPersonProgram())
        {
          local.IncludeInLine35.Flag = "Y";

          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.ForCreateOcse157Verification.Comment =
              "Publicly funded health insurance.";
            local.ForCreateOcse157Verification.SuppPersonNumber =
              entities.ChCsePerson.Number;
          }

          break;
        }
      }

      if (AsChar(local.IncludeInLine35.Flag) == 'N')
      {
        // ---------------------------------------------------------------------------------------------
        // Include the case in Line 35 if any child on the case had health 
        // insurance at any time during the FY.
        // ---------------------------------------------------------------------------------------------
        foreach(var item1 in ReadCsePerson2())
        {
          // -- Find health insurance coverage for the child at any time during 
          // the fiscal year.
          foreach(var item2 in ReadPersonalHealthInsuranceHealthInsuranceCoverage())
            
          {
            // -- The health insurance must have been provided by someone on 
            // this case.
            if (ReadCsePerson1())
            {
              local.IncludeInLine35.Flag = "Y";

              if (AsChar(import.DisplayInd.Flag) == 'Y')
              {
                local.ForCreateOcse157Verification.Comment =
                  "Privately funded health insurance.";
                local.ForCreateOcse157Verification.SuppPersonNumber =
                  entities.ChCsePerson.Number;
                local.ForCreateOcse157Verification.ObligorPersonNbr =
                  entities.CsePerson.Number;
              }

              goto ReadEach;
            }
            else
            {
              // -- Continue.
            }
          }
        }

ReadEach:
        ;
      }

      if (AsChar(local.IncludeInLine35.Flag) == 'N')
      {
        // ---------------------------------------------------------------------------------------------
        // Include the case in Line 35 if there was an MS or MC collection 
        // during the FY.
        // ---------------------------------------------------------------------------------------------
        foreach(var item1 in ReadCaseRoleCsePersonCaseRoleCsePerson())
        {
          // --  MS is sys gen id = 3 and MC is sys gen id = 19
          if (ReadCollection())
          {
            local.IncludeInLine35.Flag = "Y";

            if (AsChar(import.DisplayInd.Flag) == 'Y')
            {
              local.ForCreateOcse157Verification.Comment =
                "MS or MC Collection.";
              local.ForCreateOcse157Verification.SuppPersonNumber =
                entities.ChCsePerson.Number;
              local.ForCreateOcse157Verification.ObligorPersonNbr =
                entities.ApCsePerson.Number;
              local.ForCreateOcse157Verification.CollCreatedDte =
                Date(entities.Collection.CreatedTmst);
            }

            break;
          }
        }
      }

      if (AsChar(local.IncludeInLine35.Flag) != 'Y')
      {
        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;
          local.ForCreateOcse157Verification.LineNumber = "35";
          local.ForCreateOcse157Verification.Comment =
            "Skipped-No Medical coverage.";
          UseFnCreateOcse157Verification();
        }

        continue;
      }

      ++local.Line35.Count;
      local.ForCreateOcse157Verification.LineNumber = "35";
      local.ForCreateOcse157Verification.Column = "a";
      local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;
      UseFnCreateOcse157Verification();
      ++local.CommitCnt.Count;

      if (local.CommitCnt.Count >= import
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        local.CommitCnt.Count = 0;
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo = "35 " + entities
          .Case1.Number;
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line35.Count, 6, 10);
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.ForError.LineNumber = "33";
          local.ForError.SuppPersonNumber = entities.ChCsePerson.Number;
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
    local.ForCreateOcse157Data.LineNumber = "35";
    local.ForCreateOcse157Data.Column = "a";
    local.ForCreateOcse157Data.Number = local.Line35.Count;
    UseFnCreateOcse157Data();
    local.ProgramCheckpointRestart.RestartInd = "Y";
    local.ProgramCheckpointRestart.RestartInfo = "36 " + " ";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.ForError.LineNumber = "35";
      local.ForError.CaseNumber = "";
      local.ForError.SuppPersonNumber = "";
      UseOcse157WriteError();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.Abort.Flag = "Y";
      }
    }
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
    target.ObligorPersonNbr = source.ObligorPersonNbr;
    target.DtePaternityEst = source.DtePaternityEst;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.DateOfBirth = source.DateOfBirth;
    target.PlaceOfBirth = source.PlaceOfBirth;
    target.SocialSecurityNumber = source.SocialSecurityNumber;
    target.ObTypeSgi = source.ObTypeSgi;
    target.CollApplToCode = source.CollApplToCode;
    target.CollCreatedDte = source.CollCreatedDte;
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
    target.ObligorPersonNbr = source.ObligorPersonNbr;
    target.DtePaternityEst = source.DtePaternityEst;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.DateOfBirth = source.DateOfBirth;
    target.PlaceOfBirth = source.PlaceOfBirth;
    target.SocialSecurityNumber = source.SocialSecurityNumber;
    target.ObTypeSgi = source.ObTypeSgi;
    target.CollApplToCode = source.CollApplToCode;
    target.CollCreatedDte = source.CollCreatedDte;
    target.CaseAsinEffDte = source.CaseAsinEffDte;
    target.CaseAsinEndDte = source.CaseAsinEndDte;
    target.IntRequestIdent = source.IntRequestIdent;
    target.KansasCaseInd = source.KansasCaseInd;
    target.PersonProgCode = source.PersonProgCode;
    target.GoodCauseEffDte = source.GoodCauseEffDte;
    target.Comment = source.Comment;
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
        db.SetNullableString(
          command, "caseNumber1", import.From.CaseNumber ?? "");
        db.
          SetNullableString(command, "caseNumber2", import.To.CaseNumber ?? "");
          
        db.SetString(command, "numb", local.Restart.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRoleCsePersonCaseRoleCsePerson()
  {
    entities.ChCaseRole.Populated = false;
    entities.ApCaseRole.Populated = false;
    entities.ApCsePerson.Populated = false;
    entities.ChCsePerson.Populated = false;

    return ReadEach("ReadCaseRoleCsePersonCaseRoleCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ApCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ApCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ApCsePerson.Number = db.GetString(reader, 1);
        entities.ApCaseRole.Type1 = db.GetString(reader, 2);
        entities.ApCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ApCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ApCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ChCaseRole.CasNumber = db.GetString(reader, 6);
        entities.ChCaseRole.CspNumber = db.GetString(reader, 7);
        entities.ChCsePerson.Number = db.GetString(reader, 7);
        entities.ChCaseRole.Type1 = db.GetString(reader, 8);
        entities.ChCaseRole.Identifier = db.GetInt32(reader, 9);
        entities.ChCaseRole.StartDate = db.GetNullableDate(reader, 10);
        entities.ChCaseRole.EndDate = db.GetNullableDate(reader, 11);
        entities.ChCaseRole.Populated = true;
        entities.ApCaseRole.Populated = true;
        entities.ApCsePerson.Populated = true;
        entities.ChCsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ApCaseRole.Type1);
        CheckValid<CaseRole>("Type1", entities.ChCaseRole.Type1);

        return true;
      });
  }

  private bool ReadCollection()
  {
    entities.Collection.Populated = false;

    return Read("ReadCollection",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspSupNumber", entities.ChCsePerson.Number);
        db.SetDateTime(
          command, "timestamp1",
          import.ReportStartDate.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetDate(
          command, "collAdjDt", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Collection.ConcurrentInd = db.GetString(reader, 2);
        entities.Collection.CrtType = db.GetInt32(reader, 3);
        entities.Collection.CstId = db.GetInt32(reader, 4);
        entities.Collection.CrvId = db.GetInt32(reader, 5);
        entities.Collection.CrdId = db.GetInt32(reader, 6);
        entities.Collection.ObgId = db.GetInt32(reader, 7);
        entities.Collection.CspNumber = db.GetString(reader, 8);
        entities.Collection.CpaType = db.GetString(reader, 9);
        entities.Collection.OtrId = db.GetInt32(reader, 10);
        entities.Collection.OtrType = db.GetString(reader, 11);
        entities.Collection.OtyId = db.GetInt32(reader, 12);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 13);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 14);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
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
    entities.ChCsePerson.Populated = false;

    return ReadEach("ReadCsePerson2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate", import.ReportStartDate.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ChCsePerson.Number = db.GetString(reader, 0);
        entities.ChCsePerson.Populated = true;

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

  private bool ReadPersonProgram()
  {
    entities.PersonProgram.Populated = false;

    return Read("ReadPersonProgram",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ChCsePerson.Number);
        db.SetDate(
          command, "effectiveDate",
          import.ReportEndDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate",
          import.ReportStartDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.PersonProgram.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPersonalHealthInsuranceHealthInsuranceCoverage()
  {
    entities.HealthInsuranceCoverage.Populated = false;
    entities.PersonalHealthInsurance.Populated = false;

    return ReadEach("ReadPersonalHealthInsuranceHealthInsuranceCoverage",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ChCsePerson.Number);
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
        entities.HealthInsuranceCoverage.Populated = true;
        entities.PersonalHealthInsurance.Populated = true;

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
    /// A value of ReportEndDate.
    /// </summary>
    [JsonPropertyName("reportEndDate")]
    public DateWorkArea ReportEndDate
    {
      get => reportEndDate ??= new();
      set => reportEndDate = value;
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

    private ProgramCheckpointRestart programCheckpointRestart;
    private Ocse157Verification ocse157Verification;
    private DateWorkArea reportEndDate;
    private Common displayInd;
    private DateWorkArea reportStartDate;
    private Ocse157Verification from;
    private Ocse157Verification to;
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
    /// <summary>
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public Case1 Restart
    {
      get => restart ??= new();
      set => restart = value;
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
    /// A value of Line35.
    /// </summary>
    [JsonPropertyName("line35")]
    public Common Line35
    {
      get => line35 ??= new();
      set => line35 = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public Ocse157Verification Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of IncludeInLine35.
    /// </summary>
    [JsonPropertyName("includeInLine35")]
    public Common IncludeInLine35
    {
      get => includeInLine35 ??= new();
      set => includeInLine35 = value;
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
    /// A value of ForError.
    /// </summary>
    [JsonPropertyName("forError")]
    public Ocse157Verification ForError
    {
      get => forError ??= new();
      set => forError = value;
    }

    private Case1 restart;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Ocse157Verification forCreateOcse157Verification;
    private Ocse157Data forCreateOcse157Data;
    private Common line35;
    private Ocse157Data max;
    private Ocse157Verification null1;
    private Common includeInLine35;
    private Case1 minimum;
    private Common commitCnt;
    private Ocse157Verification forError;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePersonAccount Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
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
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
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
    /// A value of Ocse157Verification.
    /// </summary>
    [JsonPropertyName("ocse157Verification")]
    public Ocse157Verification Ocse157Verification
    {
      get => ocse157Verification ??= new();
      set => ocse157Verification = value;
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
    /// A value of HealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("healthInsuranceCoverage")]
    public HealthInsuranceCoverage HealthInsuranceCoverage
    {
      get => healthInsuranceCoverage ??= new();
      set => healthInsuranceCoverage = value;
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
    /// A value of PersonalHealthInsurance.
    /// </summary>
    [JsonPropertyName("personalHealthInsurance")]
    public PersonalHealthInsurance PersonalHealthInsurance
    {
      get => personalHealthInsurance ??= new();
      set => personalHealthInsurance = value;
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
    /// A value of ApCaseRole.
    /// </summary>
    [JsonPropertyName("apCaseRole")]
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
    }

    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
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

    private ObligationTransaction debt;
    private CsePersonAccount obligor;
    private ObligationType obligationType;
    private Obligation obligation;
    private CsePersonAccount supported;
    private Collection collection;
    private Ocse157Data ocse157Data;
    private Case1 case1;
    private Program program;
    private PersonProgram personProgram;
    private Ocse157Verification ocse157Verification;
    private CsePerson csePerson;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private CaseRole caseRole;
    private PersonalHealthInsurance personalHealthInsurance;
    private CaseRole chCaseRole;
    private CaseRole apCaseRole;
    private CsePerson apCsePerson;
    private CsePerson chCsePerson;
  }
#endregion
}
