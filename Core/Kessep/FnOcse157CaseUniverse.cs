// Program: FN_OCSE157_CASE_UNIVERSE, ID: 371122778, model: 746.
// Short name: SWE02971
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_OCSE157_CASE_UNIVERSE.
/// </summary>
[Serializable]
public partial class FnOcse157CaseUniverse: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_OCSE157_CASE_UNIVERSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnOcse157CaseUniverse(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnOcse157CaseUniverse.
  /// </summary>
  public FnOcse157CaseUniverse(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -----------------------------------------------------------
    // Initial Version - 9/2001.
    // -----------------------------------------------------------
    // ----------------------------------------------------------------------------------------------------------------------------
    // 04/14/08  GVandy	CQ#2461		Per federal data reliability audit, provide 
    // case status date instead of case worker
    // 					assignment date in audit data.
    // 10/14/08  GVandy	CQ7393		Include last 4 digits of child, AP, and AR SSN 
    // in federal audit data.
    // 10/05/12  GVandy	CQ35684		Report only one CP and one NCP per case.
    // 10/02/13  LSS	        CQ37588		Remove last 4 digits of child SSN from 
    // federal audit data.
    //                                         
    // SSN is not included in the federal
    // requirements for data submission.
    //                                         
    // Include AP Name and AR Name in audit
    // data per federal data submission
    // requirments
    //                                         
    // (FY2013)
    // ----------------------------------------------------------------------------------------------------------------------------
    local.ForCreateOcse157Verification.FiscalYear =
      import.Ocse157Verification.FiscalYear.GetValueOrDefault();
    local.ForCreateOcse157Verification.RunNumber =
      import.Ocse157Verification.RunNumber.GetValueOrDefault();
    local.ForCreateOcse157Verification.LineNumber = "888";
    local.ForCreateOcse157Verification.Column = "a";
    local.ForCreateOcse157Data.FiscalYear =
      import.Ocse157Verification.FiscalYear.GetValueOrDefault();
    local.ForCreateOcse157Data.RunNumber =
      import.Ocse157Verification.RunNumber.GetValueOrDefault();
    local.ProgramCheckpointRestart.ProgramName =
      import.ProgramCheckpointRestart.ProgramName;

    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
      (import.ProgramCheckpointRestart.RestartInfo, 1, 3, "888"))
    {
      local.Restart.Number =
        Substring(import.ProgramCheckpointRestart.RestartInfo, 4, 10);
      local.Line888.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 14, 10));
    }

    foreach(var item in ReadCase())
    {
      MoveOcse157Verification2(local.NullOcse157Verification,
        local.ForCreateOcse157Verification);
      local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;
      ReadCaseAssignmentOfficeServiceProvider();

      if (!entities.CaseAssignment.Populated)
      {
        // -----------------------------------------
        // Case created after FY end.
        // -----------------------------------------
        continue;
      }

      if (!Lt(entities.CaseAssignment.DiscontinueDate, import.ReportEndDate.Date))
        
      {
        local.ForCreateOcse157Verification.KansasCaseInd = "O";

        if (!Lt(import.ReportEndDate.Date, entities.Case1.StatusDate))
        {
          local.ForCreateOcse157Verification.CaseAsinEffDte =
            entities.Case1.StatusDate;
        }
        else
        {
          local.ForCreateOcse157Verification.CaseAsinEffDte =
            entities.Case1.CseOpenDate;
        }
      }
      else
      {
        local.ForCreateOcse157Verification.KansasCaseInd = "C";

        if (!Lt(import.ReportEndDate.Date, entities.Case1.StatusDate))
        {
          local.ForCreateOcse157Verification.CaseAsinEffDte =
            entities.Case1.StatusDate;
        }
        else
        {
          local.ForCreateOcse157Verification.CaseAsinEffDte =
            entities.CaseAssignment.DiscontinueDate;
        }
      }

      local.ForCreateOcse157Verification.CaseWorkerNumber =
        NumberToString(entities.Office.SystemGeneratedId, 12, 4);
      local.ForCreateOcse157Verification.CaseWorkerName =
        entities.ServiceProvider.FirstName + entities
        .ServiceProvider.LastName + entities.ServiceProvider.MiddleInitial;

      // -- 10/05/12  GVandy  CQ35684  Report only one CP and one NCP per case.
      // CQ37588  Do not include SSN in audit data per federal submission 
      // requirements.
      //          Include AP Name and AR Name in audit data.
      //          Use separate reads - one for AP and one for AR to account for 
      // when the AR is an organization.
      local.CsePersonsWorkSet.Assign(local.NullCsePersonsWorkSet);

      if (ReadCaseRoleCsePerson1())
      {
        if (AsChar(entities.CsePerson.Type1) == 'C')
        {
          local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
          UseCabReadAdabasPersonBatch();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.ForError.LineNumber = "888";
            local.ForError.CaseNumber = entities.Case1.Number;
            local.ForError.SuppPersonNumber = entities.CsePerson.Number;
            UseOcse157WriteError();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Abort.Flag = "Y";

              return;
            }
          }
        }
      }

      UseFnB716FormatName();
      local.ForCreateOcse157Verification.ApName =
        local.CsePersonsWorkSet.FormattedName;
      local.CsePersonsWorkSet.Assign(local.NullCsePersonsWorkSet);
      local.ForCreateOcse157Verification.ArName = "UNKNOWN";

      if (ReadCaseRoleCsePerson2())
      {
        if (AsChar(entities.CsePerson.Type1) == 'C')
        {
          local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
          UseCabReadAdabasPersonBatch();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.ForError.LineNumber = "888";
            local.ForError.CaseNumber = entities.Case1.Number;
            local.ForError.SuppPersonNumber = entities.CsePerson.Number;
            UseOcse157WriteError();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Abort.Flag = "Y";

              return;
            }
          }

          UseFnB716FormatName();
          local.ForCreateOcse157Verification.ArName =
            local.CsePersonsWorkSet.FormattedName;
        }
        else if (AsChar(entities.CsePerson.Type1) == 'O')
        {
          local.ForCreateOcse157Verification.ArName =
            entities.CsePerson.OrganizationName;
        }
      }

      UseFnCreateOcse157Verification();
      ++local.Line888.Count;

      // -- 10/05/12  GVandy  CQ35684  Continue reporting all children ever on 
      // the case.
      ++local.CommitCnt.Count;

      if (local.CommitCnt.Count >= import
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        local.CommitCnt.Count = 0;
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo = "888" + entities
          .Case1.Number;
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line888.Count, 6, 10);
        UseUpdateCheckpointRstAndCommit();

        if (local.ForCommit.NumericReturnCode != 0)
        {
          local.ForError.LineNumber = "888";
          local.ForError.CaseNumber = entities.Ocse157Verification.CaseNumber;
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
    local.ForCreateOcse157Data.LineNumber = "888";
    local.ForCreateOcse157Data.Column = "a";
    local.ForCreateOcse157Data.Number = local.Line888.Count;
    UseFnCreateOcse157Data();
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
    target.DebtDetailDueDt = source.DebtDetailDueDt;
    target.DebtDetailBalanceDue = source.DebtDetailBalanceDue;
    target.ObTypeSgi = source.ObTypeSgi;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDte = source.CollectionDte;
    target.CollCreatedDte = source.CollCreatedDte;
    target.CaseRoleType = source.CaseRoleType;
    target.CaseWorkerNumber = source.CaseWorkerNumber;
    target.CaseWorkerName = source.CaseWorkerName;
    target.CaseAsinEffDte = source.CaseAsinEffDte;
    target.CaseAsinEndDte = source.CaseAsinEndDte;
    target.IntRequestIdent = source.IntRequestIdent;
    target.KansasCaseInd = source.KansasCaseInd;
    target.PersonProgCode = source.PersonProgCode;
    target.Comment = source.Comment;
    target.Child4DigitSsn = source.Child4DigitSsn;
    target.Ap4DigitSsn = source.Ap4DigitSsn;
    target.Ar4DigitSsn = source.Ar4DigitSsn;
    target.ArName = source.ArName;
    target.ApName = source.ApName;
  }

  private static void MoveOcse157Verification2(Ocse157Verification source,
    Ocse157Verification target)
  {
    target.CaseNumber = source.CaseNumber;
    target.SuppPersonNumber = source.SuppPersonNumber;
    target.ObligorPersonNbr = source.ObligorPersonNbr;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.DebtDetailDueDt = source.DebtDetailDueDt;
    target.DebtDetailBalanceDue = source.DebtDetailBalanceDue;
    target.ObTypeSgi = source.ObTypeSgi;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDte = source.CollectionDte;
    target.CollCreatedDte = source.CollCreatedDte;
    target.CaseRoleType = source.CaseRoleType;
    target.CaseWorkerNumber = source.CaseWorkerNumber;
    target.CaseWorkerName = source.CaseWorkerName;
    target.CaseAsinEffDte = source.CaseAsinEffDte;
    target.CaseAsinEndDte = source.CaseAsinEndDte;
    target.IntRequestIdent = source.IntRequestIdent;
    target.KansasCaseInd = source.KansasCaseInd;
    target.PersonProgCode = source.PersonProgCode;
    target.Comment = source.Comment;
    target.Child4DigitSsn = source.Child4DigitSsn;
    target.Ap4DigitSsn = source.Ap4DigitSsn;
    target.Ar4DigitSsn = source.Ar4DigitSsn;
    target.ArName = source.ArName;
    target.ApName = source.ApName;
  }

  private void UseCabReadAdabasPersonBatch()
  {
    var useImport = new CabReadAdabasPersonBatch.Import();
    var useExport = new CabReadAdabasPersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(CabReadAdabasPersonBatch.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseFnB716FormatName()
  {
    var useImport = new FnB716FormatName.Import();
    var useExport = new FnB716FormatName.Export();

    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(FnB716FormatName.Execute, useImport, useExport);

    local.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
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
        entities.Case1.StatusDate = db.GetNullableDate(reader, 1);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 2);
        entities.Case1.InterstateCaseId = db.GetNullableString(reader, 3);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadCaseAssignmentOfficeServiceProvider()
  {
    entities.ServiceProvider.Populated = false;
    entities.Office.Populated = false;
    entities.CaseAssignment.Populated = false;

    return Read("ReadCaseAssignmentOfficeServiceProvider",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          import.ReportEndDate.Date.GetValueOrDefault());
        db.SetString(command, "casNo", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 4);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 5);
        entities.CaseAssignment.OspCode = db.GetString(reader, 6);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 7);
        entities.CaseAssignment.CasNo = db.GetString(reader, 8);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 9);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 10);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 11);
        entities.ServiceProvider.UserId = db.GetString(reader, 12);
        entities.ServiceProvider.LastName = db.GetString(reader, 13);
        entities.ServiceProvider.FirstName = db.GetString(reader, 14);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 15);
        entities.ServiceProvider.Populated = true;
        entities.Office.Populated = true;
        entities.CaseAssignment.Populated = true;
      });
  }

  private bool ReadCaseRoleCsePerson1()
  {
    entities.CsePerson.Populated = false;
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRoleCsePerson1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
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
        entities.CaseRole.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.CsePerson.Type1 = db.GetString(reader, 7);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 8);
        entities.CsePerson.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCaseRoleCsePerson2()
  {
    entities.CsePerson.Populated = false;
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRoleCsePerson2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
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
        entities.CaseRole.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.CsePerson.Type1 = db.GetString(reader, 7);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 8);
        entities.CsePerson.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
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

    private DateWorkArea reportEndDate;
    private Ocse157Verification from;
    private Ocse157Verification to;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Ocse157Verification ocse157Verification;
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
    /// A value of NullCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("nullCsePersonsWorkSet")]
    public CsePersonsWorkSet NullCsePersonsWorkSet
    {
      get => nullCsePersonsWorkSet ??= new();
      set => nullCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of NullOcse157Verification.
    /// </summary>
    [JsonPropertyName("nullOcse157Verification")]
    public Ocse157Verification NullOcse157Verification
    {
      get => nullOcse157Verification ??= new();
      set => nullOcse157Verification = value;
    }

    /// <summary>
    /// A value of PrevCaseRole.
    /// </summary>
    [JsonPropertyName("prevCaseRole")]
    public CaseRole PrevCaseRole
    {
      get => prevCaseRole ??= new();
      set => prevCaseRole = value;
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
    /// A value of PrevCsePerson.
    /// </summary>
    [JsonPropertyName("prevCsePerson")]
    public CsePerson PrevCsePerson
    {
      get => prevCsePerson ??= new();
      set => prevCsePerson = value;
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
    /// A value of ForCreateOcse157Verification.
    /// </summary>
    [JsonPropertyName("forCreateOcse157Verification")]
    public Ocse157Verification ForCreateOcse157Verification
    {
      get => forCreateOcse157Verification ??= new();
      set => forCreateOcse157Verification = value;
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
    /// A value of CommitCnt.
    /// </summary>
    [JsonPropertyName("commitCnt")]
    public Common CommitCnt
    {
      get => commitCnt ??= new();
      set => commitCnt = value;
    }

    /// <summary>
    /// A value of ForCommit.
    /// </summary>
    [JsonPropertyName("forCommit")]
    public External ForCommit
    {
      get => forCommit ??= new();
      set => forCommit = value;
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
    /// A value of Line888.
    /// </summary>
    [JsonPropertyName("line888")]
    public Common Line888
    {
      get => line888 ??= new();
      set => line888 = value;
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

    private CsePersonsWorkSet nullCsePersonsWorkSet;
    private Ocse157Verification nullOcse157Verification;
    private CaseRole prevCaseRole;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson prevCsePerson;
    private Case1 restart;
    private Ocse157Verification forCreateOcse157Verification;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common commitCnt;
    private External forCommit;
    private Ocse157Verification forError;
    private Common line888;
    private Ocse157Data forCreateOcse157Data;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
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
    /// A value of Ocse157Verification.
    /// </summary>
    [JsonPropertyName("ocse157Verification")]
    public Ocse157Verification Ocse157Verification
    {
      get => ocse157Verification ??= new();
      set => ocse157Verification = value;
    }

    private CsePerson csePerson;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private CaseAssignment caseAssignment;
    private CaseRole caseRole;
    private Case1 case1;
    private Ocse157Verification ocse157Verification;
  }
#endregion
}
