// Program: OE_DELETE_LOCATE_REQUEST, ID: 374424035, model: 746.
// Short name: SWE00902
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
/// A program: OE_DELETE_LOCATE_REQUEST.
/// </para>
/// <para>
/// Deletes applicable LOCATE_REQUESTS.  These will fall into one of two 
/// catagories:
/// ONLINE:  USER with appropriate authority.  This method is performed where 
/// the trancode is equal to SRPO
/// OR
/// BATCH:  Two possible routes:
/// 1)	The latest of the Request_Date or
/// Response_Date is at least 180
/// 
/// days old
/// OR
/// 2)	For a multiple source agency resend, all but the first sequence
/// number for each agency record.
/// </para>
/// </summary>
[Serializable]
public partial class OeDeleteLocateRequest: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_DELETE_LOCATE_REQUEST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeDeleteLocateRequest(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeDeleteLocateRequest.
  /// </summary>
  public OeDeleteLocateRequest(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------------------------
    // CHANGE LOG:
    // 06/30/2000	PMcElderry
    // Original coding
    // 07/10/2000	PMcElderry
    // Added logic for batch program
    // --------------------------------------------------------------
    if (Equal(global.TranCode, "SRDO"))
    {
      if (ReadLocateRequest1())
      {
        DeleteLocateRequest();
        ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
      }
      else
      {
        ExitState = "OE0000_LOCATE_REQUEST_NF";
      }
    }
    else
    {
      local.ProgramProcessingInfo.ProcessDate =
        import.ProgramProcessingInfo.ProcessDate;
      MoveProgramCheckpointRestart2(import.ProgramCheckpointRestart,
        local.ProgramCheckpointRestart);
      local.Null1.Date = null;
      local.ProgramCheckpointRestart.ProgramName = global.UserId;
      local.Current.Date = Now().Date;

      foreach(var item in ReadCaseCaseRole())
      {
        if (ReadCsePerson())
        {
          foreach(var item1 in ReadLocateRequest2())
          {
            if (local.DeleteCount.Count == local
              .ProgramCheckpointRestart.UpdateFrequencyCount.
                GetValueOrDefault())
            {
              local.ProgramCheckpointRestart.RestartInfo =
                entities.Case1.Number;
              UseUpdatePgmCheckpointRestart();
              UseExtToDoACommit();

              if (local.PassArea.NumericReturnCode == 0)
              {
                local.DeleteCount.Count = 0;
              }
              else
              {
                ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

                return;
              }

              export.TotalDeleteCount.Count = local.DeleteCount.Count + export
                .TotalDeleteCount.Count;
            }
            else
            {
              local.ProgramCheckpointRestart.RestartInfo =
                entities.Case1.Number;
            }

            // ------------------------------------------------------
            // First type of delete - last date of action + 18 months
            // ------------------------------------------------------
            if (Equal(entities.LocateRequest.ResponseDate, local.Null1.Date))
            {
              if (Lt(AddMonths(entities.LocateRequest.RequestDate, 18),
                local.ProgramProcessingInfo.ProcessDate))
              {
                DeleteLocateRequest();
                ++local.DeleteCount.Count;

                continue;
              }
              else
              {
                // -------------------
                // Continue processing
                // -------------------
              }
            }
            else if (Lt(AddMonths(entities.LocateRequest.ResponseDate, 18),
              local.ProgramProcessingInfo.ProcessDate))
            {
              DeleteLocateRequest();
              ++local.DeleteCount.Count;

              continue;
            }
            else
            {
              // -------------------
              // Continue processing
              // -------------------
            }

            // -------------------------------------------------------
            // Second type of deletes - multiple sources for an agency
            // -------------------------------------------------------
            if (entities.LocateRequest.SequenceNumber > 1)
            {
              if (ReadIncarceration())
              {
                ExitState = "INCARCERATION_AE";
              }
              else
              {
                foreach(var item2 in ReadObligation())
                {
                  foreach(var item3 in ReadDebtDebtDetail())
                  {
                    local.Local2MonthsAgo.ProcessDate =
                      AddMonths(import.ProgramProcessingInfo.ProcessDate, -2);

                    if (!Lt(local.Local2MonthsAgo.ProcessDate,
                      entities.DebtDetail.DueDt))
                    {
                      local.LocateRequestAgency.Text4 =
                        Substring(entities.LocateRequest.AgencyNumber, 2, 4);

                      if (ReadCodeCodeValue())
                      {
                        if (Equal(entities.LocateRequest.ResponseDate,
                          local.Null1.Date))
                        {
                          // ---------------------------------------------------
                          // Determine if a delete based upon LOCATE_REQUEST
                          // 'without a response' should occur
                          // ---------------------------------------------------
                          if (!Lt(local.ProgramProcessingInfo.ProcessDate,
                            AddDays(entities.LocateRequest.RequestDate,
                            (int)StringToNumber(Substring(
                              entities.CodeValue.Cdvalue,
                            CodeValue.Cdvalue_MaxLength, 1, 3)))))
                          {
                            DeleteLocateRequest();
                            ++local.DeleteCount.Count;

                            goto ReadEach;
                          }
                          else
                          {
                            // -------------------
                            // Continue processing
                            // -------------------
                          }
                        }
                        else
                        {
                          // ----------------------------------------------------
                          // Determine if a delete based upon LOCATE_REQUEST 
                          // with
                          // a response should occur
                          // ----------------------------------------------------
                          if (!Lt(local.ProgramProcessingInfo.ProcessDate,
                            AddDays(entities.LocateRequest.ResponseDate,
                            (int)StringToNumber(Substring(
                              entities.CodeValue.Cdvalue,
                            CodeValue.Cdvalue_MaxLength, 4, 3)))))
                          {
                            DeleteLocateRequest();
                            ++local.DeleteCount.Count;

                            goto ReadEach;
                          }
                          else
                          {
                            // -------------------
                            // Continue processing
                            // -------------------
                          }
                        }
                      }
                      else
                      {
                        ExitState = "CODE_VALUE_NF";

                        goto Read;
                      }
                    }
                    else
                    {
                      // -------------------------------------
                      // Get next occurrence of LOCATE REQUEST
                      // -------------------------------------
                    }
                  }
                }
              }
            }
            else
            {
              // -------------------------------------
              // Get next occurrence of LOCATE REQUEST
              // -------------------------------------
            }

ReadEach:
            ;
          }
        }
        else
        {
          ExitState = "CSE_PERSON_NF";
        }

Read:

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // -------------------
          // continue processing
          // -------------------
        }
        else
        {
          if (IsExitState("CSE_PERSON_NF"))
          {
            local.EabReportSend.RptDetail =
              "CSE Person number not found; Case number is " + entities
              .Case1.Number;
          }
          else if (IsExitState("CODE_VALUE_NF"))
          {
            local.EabReportSend.RptDetail =
              "Code value for agency not found; " + entities
              .LocateRequest.AgencyNumber + " CSE person number is " + entities
              .CsePerson.Number;
          }
          else if (IsExitState("INCARCERATION_AE"))
          {
            local.EabReportSend.RptDetail =
              "Cannot delete record, an incarceration record exists; CSE person number is " +
              entities.CsePerson.Number;
          }
          else
          {
            // ------------------
            // Add more as needed
            // ------------------
          }

          local.EabFileHandling.Action = "WRITE";
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
          else
          {
            ExitState = "ACO_NN0000_ALL_OK";
          }
        }
      }

      // -----------------------------------------------------------------
      // For termination logic and for the last record (add the total to
      // that point), fall through here.
      // -----------------------------------------------------------------
      export.TotalDeleteCount.Count = local.DeleteCount.Count + export
        .TotalDeleteCount.Count;
    }
  }

  private static void MoveProgramCheckpointRestart1(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.ProgramName = source.ProgramName;
    target.CheckpointCount = source.CheckpointCount;
    target.RestartInfo = source.RestartInfo;
  }

  private static void MoveProgramCheckpointRestart2(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.RestartInfo = source.RestartInfo;
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

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.Pass.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.Pass.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    MoveProgramCheckpointRestart1(local.ProgramCheckpointRestart,
      useImport.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private void DeleteLocateRequest()
  {
    Update("DeleteLocateRequest",
      (db, command) =>
      {
        db.SetString(
          command, "csePersonNumber", entities.LocateRequest.CsePersonNumber);
        db.SetString(
          command, "agencyNumber", entities.LocateRequest.AgencyNumber);
        db.SetInt32(
          command, "sequenceNumber", entities.LocateRequest.SequenceNumber);
      });
  }

  private IEnumerable<bool> ReadCaseCaseRole()
  {
    entities.Case1.Populated = false;
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseCaseRole",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetNullableString(
          command, "restartInfo",
          local.ProgramCheckpointRestart.RestartInfo ?? "");
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.Case1.Populated = true;
        entities.CaseRole.Populated = true;

        return true;
      });
  }

  private bool ReadCodeCodeValue()
  {
    entities.Code.Populated = false;
    entities.CodeValue.Populated = false;

    return Read("ReadCodeCodeValue",
      (db, command) =>
      {
        db.SetString(command, "text4", local.LocateRequestAgency.Text4);
      },
      (db, reader) =>
      {
        entities.Code.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 0);
        entities.Code.CodeName = db.GetString(reader, 1);
        entities.Code.EffectiveDate = db.GetDate(reader, 2);
        entities.Code.ExpirationDate = db.GetDate(reader, 3);
        entities.Code.DisplayTitle = db.GetString(reader, 4);
        entities.CodeValue.Id = db.GetInt32(reader, 5);
        entities.CodeValue.Cdvalue = db.GetString(reader, 6);
        entities.CodeValue.EffectiveDate = db.GetDate(reader, 7);
        entities.CodeValue.ExpirationDate = db.GetDate(reader, 8);
        entities.CodeValue.Description = db.GetString(reader, 9);
        entities.Code.Populated = true;
        entities.CodeValue.Populated = true;
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
        entities.CsePerson.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDebtDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.Debt.Populated = false;
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadDebtDebtDetail",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetNullableDate(
          command, "retiredDt", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 4);
        entities.Debt.CreatedBy = db.GetString(reader, 5);
        entities.Debt.DebtType = db.GetString(reader, 6);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 7);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 8);
        entities.Debt.OtyType = db.GetInt32(reader, 9);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 9);
        entities.DebtDetail.DueDt = db.GetDate(reader, 10);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 11);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 12);
        entities.DebtDetail.AdcDt = db.GetNullableDate(reader, 13);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 14);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 15);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 16);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 17);
        entities.Debt.Populated = true;
        entities.DebtDetail.Populated = true;

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
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Incarceration.CspNumber = db.GetString(reader, 0);
        entities.Incarceration.Identifier = db.GetInt32(reader, 1);
        entities.Incarceration.EndDate = db.GetNullableDate(reader, 2);
        entities.Incarceration.StartDate = db.GetNullableDate(reader, 3);
        entities.Incarceration.Populated = true;
      });
  }

  private bool ReadLocateRequest1()
  {
    entities.LocateRequest.Populated = false;

    return Read("ReadLocateRequest1",
      (db, command) =>
      {
        db.SetString(
          command, "csePersonNumber", import.LocateRequest.CsePersonNumber);
        db.
          SetString(command, "agencyNumber", import.LocateRequest.AgencyNumber);
          
        db.SetInt32(
          command, "sequenceNumber", import.LocateRequest.SequenceNumber);
      },
      (db, reader) =>
      {
        entities.LocateRequest.CsePersonNumber = db.GetString(reader, 0);
        entities.LocateRequest.RequestDate = db.GetNullableDate(reader, 1);
        entities.LocateRequest.ResponseDate = db.GetNullableDate(reader, 2);
        entities.LocateRequest.AgencyNumber = db.GetString(reader, 3);
        entities.LocateRequest.SequenceNumber = db.GetInt32(reader, 4);
        entities.LocateRequest.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLocateRequest2()
  {
    entities.LocateRequest.Populated = false;

    return ReadEach("ReadLocateRequest2",
      (db, command) =>
      {
        db.SetString(command, "csePersonNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.LocateRequest.CsePersonNumber = db.GetString(reader, 0);
        entities.LocateRequest.RequestDate = db.GetNullableDate(reader, 1);
        entities.LocateRequest.ResponseDate = db.GetNullableDate(reader, 2);
        entities.LocateRequest.AgencyNumber = db.GetString(reader, 3);
        entities.LocateRequest.SequenceNumber = db.GetInt32(reader, 4);
        entities.LocateRequest.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadObligation()
  {
    entities.Obligation.Populated = false;

    return ReadEach("ReadObligation",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 4);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 5);
        entities.Obligation.Populated = true;

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
    /// A value of LocateRequest.
    /// </summary>
    [JsonPropertyName("locateRequest")]
    public LocateRequest LocateRequest
    {
      get => locateRequest ??= new();
      set => locateRequest = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private LocateRequest locateRequest;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of TotalDeleteCount.
    /// </summary>
    [JsonPropertyName("totalDeleteCount")]
    public Common TotalDeleteCount
    {
      get => totalDeleteCount ??= new();
      set => totalDeleteCount = value;
    }

    private Common totalDeleteCount;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of LocateRequestAgency.
    /// </summary>
    [JsonPropertyName("locateRequestAgency")]
    public WorkArea LocateRequestAgency
    {
      get => locateRequestAgency ??= new();
      set => locateRequestAgency = value;
    }

    /// <summary>
    /// A value of LocateRequest.
    /// </summary>
    [JsonPropertyName("locateRequest")]
    public LocateRequest LocateRequest
    {
      get => locateRequest ??= new();
      set => locateRequest = value;
    }

    /// <summary>
    /// A value of Pass.
    /// </summary>
    [JsonPropertyName("pass")]
    public External Pass
    {
      get => pass ??= new();
      set => pass = value;
    }

    /// <summary>
    /// A value of DeleteCount.
    /// </summary>
    [JsonPropertyName("deleteCount")]
    public Common DeleteCount
    {
      get => deleteCount ??= new();
      set => deleteCount = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of ZdelLocalRestart.
    /// </summary>
    [JsonPropertyName("zdelLocalRestart")]
    public Case1 ZdelLocalRestart
    {
      get => zdelLocalRestart ??= new();
      set => zdelLocalRestart = value;
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
    /// A value of Local2MonthsAgo.
    /// </summary>
    [JsonPropertyName("local2MonthsAgo")]
    public ProgramProcessingInfo Local2MonthsAgo
    {
      get => local2MonthsAgo ??= new();
      set => local2MonthsAgo = value;
    }

    private DateWorkArea current;
    private WorkArea locateRequestAgency;
    private LocateRequest locateRequest;
    private External pass;
    private Common deleteCount;
    private DateWorkArea null1;
    private ProgramProcessingInfo programProcessingInfo;
    private Case1 zdelLocalRestart;
    private External passArea;
    private ProgramCheckpointRestart programCheckpointRestart;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ProgramProcessingInfo local2MonthsAgo;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of LocateRequest.
    /// </summary>
    [JsonPropertyName("locateRequest")]
    public LocateRequest LocateRequest
    {
      get => locateRequest ??= new();
      set => locateRequest = value;
    }

    /// <summary>
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
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
    /// A value of ZdelExisting.
    /// </summary>
    [JsonPropertyName("zdelExisting")]
    public CsePerson ZdelExisting
    {
      get => zdelExisting ??= new();
      set => zdelExisting = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    private Incarceration incarceration;
    private CsePerson csePerson;
    private LocateRequest locateRequest;
    private Code code;
    private CodeValue codeValue;
    private Case1 case1;
    private CaseRole caseRole;
    private CsePerson zdelExisting;
    private ObligationTransaction debt;
    private DebtDetail debtDetail;
    private Obligation obligation;
    private CsePersonAccount obligor;
  }
#endregion
}
