// Program: FN_OCSE157_LINE_19, ID: 371092718, model: 746.
// Short name: SWE02923
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_OCSE157_LINE_19.
/// </summary>
[Serializable]
public partial class FnOcse157Line19: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_OCSE157_LINE_19 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnOcse157Line19(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnOcse157Line19.
  /// </summary>
  public FnOcse157Line19(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------------------------------------------------------------
    //                                     
    // C H A N G E    L O G
    // ---------------------------------------------------------------------------------------------------
    // Date      Developer     Request #	Description
    // --------  ----------    ----------	
    // -----------------------------------------------------------
    // ??/??/??  KDoshi			Initial Development
    // 07/23/01  ??????			Skip foreign cases.
    // 08/09/01  ??????			Include Interstate Rqst Hist records where
    // 					1. Action=R and functional_code = EST, ENF or PAT      OR
    // 					2. Action=SPACES, function_code=SPACES and
    // 					   Reason_code=OICNV.
    // 07/11/06  GVandy	WR00230751	Federally mandated changes.
    // 					1) Include foreign and tribal interstate cases in line 19.
    // 02/04/20  GVandy	CQ66220		Beginning in FY 2022, exclude cases to tribal 
    // and
    // 					international child support agencies from Line 19.
    // 					Also, count the CSE case each reporting period
    // 					in which a new outgoing case is established.
    // 					
    // ---------------------------------------------------------------------------------------------------
    local.ForCreateOcse157Verification.FiscalYear =
      import.Ocse157Verification.FiscalYear.GetValueOrDefault();
    local.ForCreateOcse157Verification.RunNumber =
      import.Ocse157Verification.RunNumber.GetValueOrDefault();
    local.ForCreateOcse157Data.FiscalYear =
      import.Ocse157Verification.FiscalYear.GetValueOrDefault();
    local.ForCreateOcse157Data.RunNumber =
      import.Ocse157Verification.RunNumber.GetValueOrDefault();
    local.ProgramCheckpointRestart.ProgramName =
      import.ProgramCheckpointRestart.ProgramName;

    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
      (import.ProgramCheckpointRestart.RestartInfo, 1, 3, "19 "))
    {
      local.Restart.Number =
        Substring(import.ProgramCheckpointRestart.RestartInfo, 4, 10);
      local.Line19Curr.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 14, 10));
      local.Line19Former.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 24, 10));
      local.Line19Never.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 34, 10));
    }

    // ------------------------------------------
    // Read all Cases tied to an Outgoing Interstate Request.
    // ------------------------------------------
    foreach(var item in ReadCase())
    {
      MoveOcse157Verification2(local.Null1, local.ForCreateOcse157Verification);

      if (import.Ocse157Verification.FiscalYear.GetValueOrDefault() >= import
        .Cq66220EffectiveFy.FiscalYear.GetValueOrDefault())
      {
        // -------------------------------------------------------------------------------------
        // For FY 2022 and later:
        //   1) Excluded outgoing cases to tribal and foreign country child 
        // support agencies.
        //   2) Count case if an outgoing case was established during the 
        // reporting period.
        //      Previously it only counted the first time an outgoing case was 
        // established.
        //      So if the NCP moves to another state during the reporting period
        // and we have
        //      to request enforcement with the new state then we weren't 
        // previously counting
        //      the case.  Now we will.
        // -------------------------------------------------------------------------------------
        local.InterstateRequestFound.Flag = "N";
        local.CreatedInReportPeriod.Flag = "";
        local.CaseType.Flag = "";

        // ------------------------------------------------------------------------
        // 8/9/2001- Include Interstate Rqst Hist records where
        // 1. Action=R and functional_code = EST, ENF or PAT      OR
        // 2. Action=SPACES, function_code=SPACES and Reason_code=OICNV.
        // -------------------------------------------------------------------------
        foreach(var item1 in ReadInterstateRequest())
        {
          local.InterstateRequestFound.Flag = "Y";

          // --------------------------------------------------
          // Read earliest IRH on current Interstate Request.
          // --------------------------------------------------
          ReadInterstateRequestHistory();

          if (!entities.InterstateRequestHistory.Populated)
          {
            local.CreatedInReportPeriod.Flag = "N";

            continue;
          }

          // -------------------------------------------------
          // If earliest IRH is not created within FY, then skip Case.
          // -------------------------------------------------
          if (Lt(entities.InterstateRequestHistory.CreatedTimestamp,
            import.ReportStartDate.Timestamp) || Lt
            (import.ReportEndDate.Timestamp,
            entities.InterstateRequestHistory.CreatedTimestamp))
          {
            local.CreatedInReportPeriod.Flag = "N";

            continue;
          }

          local.CreatedInReportPeriod.Flag = "Y";

          if (!IsEmpty(entities.InterstateRequest.Country) || !
            IsEmpty(entities.InterstateRequest.TribalAgency))
          {
            // 02/04/20 GVandy  CQ66220  Beginning in FY 2022, exclude cases to 
            // tribal and
            // international child support agencies from Line 19.
            if (!IsEmpty(entities.InterstateRequest.Country))
            {
              local.CaseType.Flag = "F";
            }
            else
            {
              local.CaseType.Flag = "T";
            }

            continue;
          }

          // --If we get to here then the case is an outgoing interstate case 
          // that was
          //   established during the reporting period.  Count the cse case in 
          // Line 19.
          goto Test;
        }

        // Interstate Request Found?
        //    +-Yes
        //    |   Outgoing case created during reporting period?
        //    |       +-Yes
        //    |       |    Case is Outgoing to:
        //    |       |        +-Another State
        //    |       |        |   Count the case
        //    |       |        +-Tribal Agency
        //    |       |        |   Message "Skipped-Tribal Case"
        //    |       |        +-Foreign Country
        //    |       |        |   Message "Skipped-Foreign Case"
        //    |       |        +-
        //    |       +-No
        //    |       |    Message "Skipped-Outgoing case not established during
        // report period"
        //    |       +-
        //    +-No
        //    |   Message "Skipped-Case has never been outgoing interstate"
        //    +-
        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.ForCreateOcse157Verification.LineNumber = "19";
          local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;
          local.ForCreateOcse157Verification.IntRequestIdent =
            entities.InterstateRequest.IntHGeneratedId;

          switch(AsChar(local.InterstateRequestFound.Flag))
          {
            case 'Y':
              switch(AsChar(local.CreatedInReportPeriod.Flag))
              {
                case 'Y':
                  switch(AsChar(local.CaseType.Flag))
                  {
                    case 'T':
                      local.ForCreateOcse157Verification.Comment =
                        "Skipped-Tribal Case.";

                      break;
                    case 'F':
                      local.ForCreateOcse157Verification.Comment =
                        "Skipped-Foreign Case.";

                      break;
                    default:
                      break;
                  }

                  break;
                case 'N':
                  local.ForCreateOcse157Verification.Comment =
                    "Skipped-Not established in report period";

                  break;
                default:
                  break;
              }

              break;
            case 'N':
              local.ForCreateOcse157Verification.Comment =
                "Skipped-Case has never been outgoing";

              break;
            default:
              break;
          }

          UseFnCreateOcse157Verification();
        }

        // --Skip this CSE case.
        continue;
      }
      else
      {
        // ------------------------------------------
        // Read earliest IRH on current Case..
        // ------------------------------------------
        // ------------------------------------------------------------------------
        // 8/9/2001- Include Interstate Rqst Hist records where
        // 1. Action=R and functional_code = EST, ENF or PAT      OR
        // 2. Action=SPACES, function_code=SPACES and Reason_code=OICNV.
        // -------------------------------------------------------------------------
        ReadInterstateRequestHistoryInterstateRequest();

        if (!entities.InterstateRequestHistory.Populated)
        {
          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.ForCreateOcse157Verification.LineNumber = "19";
            local.ForCreateOcse157Verification.CaseNumber =
              entities.Case1.Number;
            local.ForCreateOcse157Verification.Comment =
              "Skipped-Int Rqst Hist nf.";
            UseFnCreateOcse157Verification();
          }

          continue;
        }

        // -------------------------------------------------
        // If earliest IRH is not created within FY, then skip Case.
        // -------------------------------------------------
        if (Lt(entities.InterstateRequestHistory.CreatedTimestamp,
          import.ReportStartDate.Timestamp) || Lt
          (import.ReportEndDate.Timestamp,
          entities.InterstateRequestHistory.CreatedTimestamp))
        {
          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.ForCreateOcse157Verification.LineNumber = "19";
            local.ForCreateOcse157Verification.CaseNumber =
              entities.Case1.Number;
            local.ForCreateOcse157Verification.IntRequestIdent =
              entities.InterstateRequest.IntHGeneratedId;
            local.ForCreateOcse157Verification.Comment =
              "Skipped-IRH create_dte not within FY.";
            UseFnCreateOcse157Verification();
          }

          continue;
        }
      }

Test:

      // -----------------------------------------------------------
      // All checks passed. Count case.
      // -----------------------------------------------------------
      UseFn157GetAssistanceForCase();

      // ------------------------------------------
      // No exit state is set in this CAB.
      // ------------------------------------------
      switch(AsChar(local.AssistanceProgram.Flag))
      {
        case 'C':
          ++local.Line19Curr.Count;
          local.ForCreateOcse157Verification.Column = "b";

          break;
        case 'F':
          ++local.Line19Former.Count;
          local.ForCreateOcse157Verification.Column = "c";

          break;
        default:
          ++local.Line19Never.Count;
          local.ForCreateOcse157Verification.Column = "d";

          break;
      }

      if (AsChar(import.DisplayInd.Flag) == 'Y')
      {
        local.ForCreateOcse157Verification.LineNumber = "19";
        local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;
        local.ForCreateOcse157Verification.IntRequestIdent =
          entities.InterstateRequest.IntHGeneratedId;
        local.ForCreateOcse157Verification.IntRqstRqstDte =
          Date(entities.InterstateRequestHistory.CreatedTimestamp);
        local.ForCreateOcse157Verification.PersonProgCode =
          local.ForVerification.Code;
        local.ForCreateOcse157Verification.SuppPersonNumber =
          local.SuppPersForVerification.Number;
        UseFnCreateOcse157Verification();
      }

      ++local.CommitCnt.Count;

      if (local.CommitCnt.Count >= import
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        local.CommitCnt.Count = 0;
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo = "19 " + entities
          .Case1.Number;
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line19Curr.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line19Former.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line19Never.Count, 6, 10);
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.ForError.LineNumber = "19";
          local.ForError.CaseNumber = entities.Case1.Number;
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
    // Take checkpoint and create ocse157_data records.
    // --------------------------------------------------
    local.ForCreateOcse157Data.LineNumber = "19";
    local.ForCreateOcse157Data.Column = "b";
    local.ForCreateOcse157Data.Number = local.Line19Curr.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "c";
    local.ForCreateOcse157Data.Number = local.Line19Former.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "d";
    local.ForCreateOcse157Data.Number = local.Line19Never.Count;
    UseFnCreateOcse157Data();
    local.ProgramCheckpointRestart.RestartInd = "Y";
    local.ProgramCheckpointRestart.RestartInfo = "20 " + " ";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.ForError.LineNumber = "19";
      local.ForError.CaseNumber = "";
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
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ObTypeSgi = source.ObTypeSgi;
    target.CollectionSgi = source.CollectionSgi;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDte = source.CollectionDte;
    target.CaseAsinEffDte = source.CaseAsinEffDte;
    target.CaseAsinEndDte = source.CaseAsinEndDte;
    target.IntRequestIdent = source.IntRequestIdent;
    target.IntRqstRqstDte = source.IntRqstRqstDte;
    target.KansasCaseInd = source.KansasCaseInd;
    target.PersonProgCode = source.PersonProgCode;
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
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ObTypeSgi = source.ObTypeSgi;
    target.CollectionSgi = source.CollectionSgi;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDte = source.CollectionDte;
    target.CaseAsinEffDte = source.CaseAsinEffDte;
    target.CaseAsinEndDte = source.CaseAsinEndDte;
    target.IntRequestIdent = source.IntRequestIdent;
    target.IntRqstRqstDte = source.IntRqstRqstDte;
    target.KansasCaseInd = source.KansasCaseInd;
    target.PersonProgCode = source.PersonProgCode;
    target.Comment = source.Comment;
  }

  private void UseFn157GetAssistanceForCase()
  {
    var useImport = new Fn157GetAssistanceForCase.Import();
    var useExport = new Fn157GetAssistanceForCase.Export();

    useImport.ReportEndDate.Date = import.ReportEndDate.Date;
    useImport.Case1.Assign(entities.Case1);

    Call(Fn157GetAssistanceForCase.Execute, useImport, useExport);

    local.ForVerification.Code = useExport.Program.Code;
    local.SuppPersForVerification.Number = useExport.CsePerson.Number;
    local.AssistanceProgram.Flag = useExport.AssistanceProgram.Flag;
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
        db.SetString(command, "numb", local.Restart.Number);
        db.SetNullableString(
          command, "caseNumber1", import.From.CaseNumber ?? "");
        db.
          SetNullableString(command, "caseNumber2", import.To.CaseNumber ?? "");
          
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 1);
        entities.Case1.PaMedicalService = db.GetNullableString(reader, 2);
        entities.Case1.InterstateCaseId = db.GetNullableString(reader, 3);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return ReadEach("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 2);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 3);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 4);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 5);
        entities.InterstateRequest.Populated = true;

        return true;
      });
  }

  private bool ReadInterstateRequestHistory()
  {
    entities.InterstateRequestHistory.Populated = false;

    return Read("ReadInterstateRequestHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstateRequest.IntHGeneratedId);
      },
      (db, reader) =>
      {
        entities.InterstateRequestHistory.IntGeneratedId =
          db.GetInt32(reader, 0);
        entities.InterstateRequestHistory.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.InterstateRequestHistory.ActionCode = db.GetString(reader, 2);
        entities.InterstateRequestHistory.FunctionalTypeCode =
          db.GetString(reader, 3);
        entities.InterstateRequestHistory.TransactionDate =
          db.GetDate(reader, 4);
        entities.InterstateRequestHistory.ActionReasonCode =
          db.GetNullableString(reader, 5);
        entities.InterstateRequestHistory.Populated = true;
      });
  }

  private bool ReadInterstateRequestHistoryInterstateRequest()
  {
    entities.InterstateRequestHistory.Populated = false;
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequestHistoryInterstateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequestHistory.IntGeneratedId =
          db.GetInt32(reader, 0);
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequestHistory.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.InterstateRequestHistory.ActionCode = db.GetString(reader, 2);
        entities.InterstateRequestHistory.FunctionalTypeCode =
          db.GetString(reader, 3);
        entities.InterstateRequestHistory.TransactionDate =
          db.GetDate(reader, 4);
        entities.InterstateRequestHistory.ActionReasonCode =
          db.GetNullableString(reader, 5);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 7);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 8);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 9);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 10);
        entities.InterstateRequestHistory.Populated = true;
        entities.InterstateRequest.Populated = true;
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
    /// A value of Ocse157Verification.
    /// </summary>
    [JsonPropertyName("ocse157Verification")]
    public Ocse157Verification Ocse157Verification
    {
      get => ocse157Verification ??= new();
      set => ocse157Verification = value;
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
    /// A value of ReportEndDate.
    /// </summary>
    [JsonPropertyName("reportEndDate")]
    public DateWorkArea ReportEndDate
    {
      get => reportEndDate ??= new();
      set => reportEndDate = value;
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
    /// A value of Cq66220EffectiveFy.
    /// </summary>
    [JsonPropertyName("cq66220EffectiveFy")]
    public Ocse157Verification Cq66220EffectiveFy
    {
      get => cq66220EffectiveFy ??= new();
      set => cq66220EffectiveFy = value;
    }

    private Ocse157Verification ocse157Verification;
    private ProgramCheckpointRestart programCheckpointRestart;
    private DateWorkArea reportEndDate;
    private DateWorkArea reportStartDate;
    private Ocse157Verification from;
    private Ocse157Verification to;
    private Common displayInd;
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
    /// <summary>
    /// A value of CaseType.
    /// </summary>
    [JsonPropertyName("caseType")]
    public Common CaseType
    {
      get => caseType ??= new();
      set => caseType = value;
    }

    /// <summary>
    /// A value of CreatedInReportPeriod.
    /// </summary>
    [JsonPropertyName("createdInReportPeriod")]
    public Common CreatedInReportPeriod
    {
      get => createdInReportPeriod ??= new();
      set => createdInReportPeriod = value;
    }

    /// <summary>
    /// A value of InterstateRequestFound.
    /// </summary>
    [JsonPropertyName("interstateRequestFound")]
    public Common InterstateRequestFound
    {
      get => interstateRequestFound ??= new();
      set => interstateRequestFound = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

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
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public Case1 Prev
    {
      get => prev ??= new();
      set => prev = value;
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
    /// A value of Line19Curr.
    /// </summary>
    [JsonPropertyName("line19Curr")]
    public Common Line19Curr
    {
      get => line19Curr ??= new();
      set => line19Curr = value;
    }

    /// <summary>
    /// A value of Line19Former.
    /// </summary>
    [JsonPropertyName("line19Former")]
    public Common Line19Former
    {
      get => line19Former ??= new();
      set => line19Former = value;
    }

    /// <summary>
    /// A value of Line19Never.
    /// </summary>
    [JsonPropertyName("line19Never")]
    public Common Line19Never
    {
      get => line19Never ??= new();
      set => line19Never = value;
    }

    /// <summary>
    /// A value of AssistanceProgram.
    /// </summary>
    [JsonPropertyName("assistanceProgram")]
    public Common AssistanceProgram
    {
      get => assistanceProgram ??= new();
      set => assistanceProgram = value;
    }

    /// <summary>
    /// A value of SuppPersForVerification.
    /// </summary>
    [JsonPropertyName("suppPersForVerification")]
    public CsePerson SuppPersForVerification
    {
      get => suppPersForVerification ??= new();
      set => suppPersForVerification = value;
    }

    /// <summary>
    /// A value of ForVerification.
    /// </summary>
    [JsonPropertyName("forVerification")]
    public Program ForVerification
    {
      get => forVerification ??= new();
      set => forVerification = value;
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

    private Common caseType;
    private Common createdInReportPeriod;
    private Common interstateRequestFound;
    private Ocse157Verification null1;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Case1 restart;
    private Case1 prev;
    private Ocse157Verification forCreateOcse157Verification;
    private Ocse157Data forCreateOcse157Data;
    private Common line19Curr;
    private Common line19Former;
    private Common line19Never;
    private Common assistanceProgram;
    private CsePerson suppPersForVerification;
    private Program forVerification;
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
    /// A value of InterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("interstateRequestHistory")]
    public InterstateRequestHistory InterstateRequestHistory
    {
      get => interstateRequestHistory ??= new();
      set => interstateRequestHistory = value;
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

    private InterstateRequestHistory interstateRequestHistory;
    private Case1 case1;
    private InterstateRequest interstateRequest;
  }
#endregion
}
