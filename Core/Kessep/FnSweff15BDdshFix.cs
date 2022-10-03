// Program: FN_SWEFF15B_DDSH_FIX, ID: 373404744, model: 746.
// Short name: SWEFF15B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_SWEFF15B_DDSH_FIX.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnSweff15BDdshFix: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_SWEFF15B_DDSH_FIX program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnSweff15BDdshFix(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnSweff15BDdshFix.
  /// </summary>
  public FnSweff15BDdshFix(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
    this.local = context.GetData<Local>();
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();
    local.ProvideFile.Flag =
      Substring(local.ProgramProcessingInfo.ParameterList, 1, 1);
    local.TestOnly.Flag =
      Substring(local.ProgramProcessingInfo.ParameterList, 2, 1);
    local.CommitPoint.Count =
      (int)StringToNumber(Substring(
        local.ProgramProcessingInfo.ParameterList, 3, 9));
    local.EabFileHandling.Action = "OPEN";
    local.NeededToOpenEabReportSend.ProgramName = global.UserId;
    local.NeededToOpenEabReportSend.ProcessDate = Now().Date;
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabFileHandling.Action = "WRITE";

    if (AsChar(local.ProvideFile.Flag) == 'Y')
    {
      local.File.Action = "OPEN";
      UseFnExtSweff15WriteFile3();

      if (!IsEmpty(local.File.Status))
      {
        local.NeededToWrite.RptDetail =
          "File write unsuccessful.  Status is:  " + local.File.Status;
        UseCabErrorReport3();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    local.File.Action = "WRITE";
    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    foreach(var item in ReadDebtObligationCsePerson())
    {
      ++local.NumberOfDebts.Count;
      local.DupDate.Count = 0;

      foreach(var item1 in ReadDebtDetailStatusHistory1())
      {
        ++local.DupDate.Count;
      }

      if (local.DupDate.Count > 1)
      {
        local.MostRecent.Flag = "Y";
        local.NumberOfDdsh.Count = 0;

        foreach(var item1 in ReadDebtDetailStatusHistory2())
        {
          if (AsChar(local.MostRecent.Flag) == 'Y')
          {
            local.MostRecent.Flag = "N";

            // : Set the discontinue date for the next row that will be 
            // retrieved.
            //   (this date is equal to the start date of the current row).
            local.DebtDetailStatusHistory.DiscontinueDt = local.Max.Date;
          }

          local.Old.DiscontinueDt =
            entities.DebtDetailStatusHistory.DiscontinueDt;

          if (AsChar(local.TestOnly.Flag) != 'Y')
          {
            try
            {
              UpdateDebtDetailStatusHistory();
              ++local.NumberOfDdsh.Count;
              ++local.TotalEojDdsh.Count;
              local.DebtDetailStatusHistory.DiscontinueDt =
                entities.DebtDetailStatusHistory.EffectiveDt;
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  local.NeededToWrite.RptDetail = "Not Unique:" + entities
                    .CsePerson.Number + "OB ID: " + NumberToString
                    (entities.Obligation.SystemGeneratedIdentifier, 13, 3) + "DEBT: " +
                    NumberToString
                    (entities.Debt.SystemGeneratedIdentifier, 7, 9) + " DDSH: " +
                    NumberToString
                    (entities.DebtDetailStatusHistory.SystemGeneratedIdentifier,
                    13, 3);
                  UseCabErrorReport4();
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                case ErrorCode.PermittedValueViolation:
                  local.NeededToWrite.RptDetail = "PV: " + entities
                    .CsePerson.Number + "OB ID: " + NumberToString
                    (entities.Obligation.SystemGeneratedIdentifier, 13, 3) + "DEBT: " +
                    NumberToString
                    (entities.Debt.SystemGeneratedIdentifier, 7, 9) + " DDSH: " +
                    NumberToString
                    (entities.DebtDetailStatusHistory.SystemGeneratedIdentifier,
                    13, 3);
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
          else
          {
            // Test only.
            ++local.NumberOfDdsh.Count;
            ++local.TotalEojDdsh.Count;
          }
        }

        // COMMIT
        ++local.ForCommit.Count;

        if (local.ForCommit.Count > local.CommitPoint.Count)
        {
          local.NeededToWrite.RptDetail = "Commit Will Be Taken: " + entities
            .CsePerson.Number + "OB ID: " + NumberToString
            (entities.Obligation.SystemGeneratedIdentifier, 13, 3) + "DEBT: " +
            NumberToString(entities.Debt.SystemGeneratedIdentifier, 7, 9) + " DDSH: " +
            NumberToString
            (entities.DebtDetailStatusHistory.SystemGeneratedIdentifier, 13, 3);
            
          UseCabErrorReport4();
          UseExtToDoACommit();
          local.ForCommit.Count = 0;
        }

        if (AsChar(local.ProvideFile.Flag) == 'Y')
        {
          UseFnExtSweff15WriteFile1();

          if (!IsEmpty(local.File.Status))
          {
            local.NeededToWrite.RptDetail =
              "File write unsuccessful.  Status is:  " + local.File.Status;
            UseCabErrorReport4();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }
        }

        ++local.DebtDetailsUpdated.Count;
      }
    }

    // : Control report/
    local.NeededToWrite.RptDetail = "Number of Debts Read:            " + NumberToString
      (local.NumberOfDebts.Count, 15);
    UseCabControlReport3();
    local.NeededToWrite.RptDetail = "Number of Debts With DDSH fixed: " + NumberToString
      (local.DebtDetailsUpdated.Count, 15);
    UseCabControlReport3();
    local.NeededToWrite.RptDetail = "Number of DDSH fixed:            " + NumberToString
      (local.TotalEojDdsh.Count, 15);
    UseCabControlReport3();

    // Close Files
    if (AsChar(local.ProvideFile.Flag) == 'Y')
    {
      local.File.Action = "CLOSE";
      UseFnExtSweff15WriteFile2();
    }

    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();
    UseCabControlReport1();

    if (AsChar(local.TestOnly.Flag) == 'Y')
    {
      ExitState = "ACO_RE0000_INPUT_FILE_EMPTY_RB";
    }
  }

  private static void MoveEabFileHandling(EabFileHandling source,
    EabFileHandling target)
  {
    target.Action = source.Action;
    target.Status = source.Status;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.NeededToOpenEabReportSend, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);
  }

  private void UseCabControlReport3()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);
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
    MoveEabReportSend(local.NeededToOpenEabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport4()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.Max.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.External.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnExtSweff15WriteFile1()
  {
    var useImport = new FnExtSweff15WriteFile.Import();
    var useExport = new FnExtSweff15WriteFile.Export();

    MoveEabFileHandling(local.File, useImport.EabFileHandling);
    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      entities.Obligation.SystemGeneratedIdentifier;
    useImport.ObligationTransaction.SystemGeneratedIdentifier =
      entities.Debt.SystemGeneratedIdentifier;
    useImport.NumberDdshUpdated.Count = local.NumberOfDdsh.Count;
    useExport.EabFileHandling.Status = local.File.Status;

    Call(FnExtSweff15WriteFile.Execute, useImport, useExport);

    local.File.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnExtSweff15WriteFile2()
  {
    var useImport = new FnExtSweff15WriteFile.Import();
    var useExport = new FnExtSweff15WriteFile.Export();

    MoveEabFileHandling(local.File, useImport.EabFileHandling);

    Call(FnExtSweff15WriteFile.Execute, useImport, useExport);
  }

  private void UseFnExtSweff15WriteFile3()
  {
    var useImport = new FnExtSweff15WriteFile.Import();
    var useExport = new FnExtSweff15WriteFile.Export();

    MoveEabFileHandling(local.File, useImport.EabFileHandling);
    useExport.EabFileHandling.Status = local.File.Status;

    Call(FnExtSweff15WriteFile.Execute, useImport, useExport);

    local.File.Status = useExport.EabFileHandling.Status;
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private IEnumerable<bool> ReadDebtDetailStatusHistory1()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.DebtDetailStatusHistory.Populated = false;

    return ReadEach("ReadDebtDetailStatusHistory1",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Debt.OtyType);
        db.SetInt32(command, "obgId", entities.Debt.ObgGeneratedId);
        db.SetString(command, "otrType", entities.Debt.Type1);
        db.SetInt32(command, "otrId", entities.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.Debt.CpaType);
        db.SetString(command, "cspNumber", entities.Debt.CspNumber);
        db.SetNullableDate(
          command, "discontinueDt", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DebtDetailStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DebtDetailStatusHistory.EffectiveDt = db.GetDate(reader, 1);
        entities.DebtDetailStatusHistory.DiscontinueDt =
          db.GetNullableDate(reader, 2);
        entities.DebtDetailStatusHistory.CreatedTmst =
          db.GetDateTime(reader, 3);
        entities.DebtDetailStatusHistory.OtrType = db.GetString(reader, 4);
        entities.DebtDetailStatusHistory.OtrId = db.GetInt32(reader, 5);
        entities.DebtDetailStatusHistory.CpaType = db.GetString(reader, 6);
        entities.DebtDetailStatusHistory.CspNumber = db.GetString(reader, 7);
        entities.DebtDetailStatusHistory.ObgId = db.GetInt32(reader, 8);
        entities.DebtDetailStatusHistory.Code = db.GetString(reader, 9);
        entities.DebtDetailStatusHistory.OtyType = db.GetInt32(reader, 10);
        entities.DebtDetailStatusHistory.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDebtDetailStatusHistory2()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.DebtDetailStatusHistory.Populated = false;

    return ReadEach("ReadDebtDetailStatusHistory2",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Debt.OtyType);
        db.SetInt32(command, "obgId", entities.Debt.ObgGeneratedId);
        db.SetString(command, "otrType", entities.Debt.Type1);
        db.SetInt32(command, "otrId", entities.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.Debt.CpaType);
        db.SetString(command, "cspNumber", entities.Debt.CspNumber);
      },
      (db, reader) =>
      {
        entities.DebtDetailStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DebtDetailStatusHistory.EffectiveDt = db.GetDate(reader, 1);
        entities.DebtDetailStatusHistory.DiscontinueDt =
          db.GetNullableDate(reader, 2);
        entities.DebtDetailStatusHistory.CreatedTmst =
          db.GetDateTime(reader, 3);
        entities.DebtDetailStatusHistory.OtrType = db.GetString(reader, 4);
        entities.DebtDetailStatusHistory.OtrId = db.GetInt32(reader, 5);
        entities.DebtDetailStatusHistory.CpaType = db.GetString(reader, 6);
        entities.DebtDetailStatusHistory.CspNumber = db.GetString(reader, 7);
        entities.DebtDetailStatusHistory.ObgId = db.GetInt32(reader, 8);
        entities.DebtDetailStatusHistory.Code = db.GetString(reader, 9);
        entities.DebtDetailStatusHistory.OtyType = db.GetInt32(reader, 10);
        entities.DebtDetailStatusHistory.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDebtObligationCsePerson()
  {
    entities.Obligation.Populated = false;
    entities.Debt.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadDebtObligationCsePerson",
      null,
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.Obligation.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.Debt.DebtType = db.GetString(reader, 5);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 6);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 7);
        entities.Debt.OtyType = db.GetInt32(reader, 8);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 8);
        entities.Obligation.Populated = true;
        entities.Debt.Populated = true;
        entities.CsePerson.Populated = true;

        return true;
      });
  }

  private void UpdateDebtDetailStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.DebtDetailStatusHistory.Populated);

    var discontinueDt = local.DebtDetailStatusHistory.DiscontinueDt;

    entities.DebtDetailStatusHistory.Populated = false;
    Update("UpdateDebtDetailStatusHistory",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDt", discontinueDt);
        db.SetInt32(
          command, "obTrnStatHstId",
          entities.DebtDetailStatusHistory.SystemGeneratedIdentifier);
        db.SetString(
          command, "otrType", entities.DebtDetailStatusHistory.OtrType);
        db.SetInt32(command, "otrId", entities.DebtDetailStatusHistory.OtrId);
        db.SetString(
          command, "cpaType", entities.DebtDetailStatusHistory.CpaType);
        db.SetString(
          command, "cspNumber", entities.DebtDetailStatusHistory.CspNumber);
        db.SetInt32(command, "obgId", entities.DebtDetailStatusHistory.ObgId);
        db.
          SetString(command, "obTrnStCd", entities.DebtDetailStatusHistory.Code);
          
        db.
          SetInt32(command, "otyType", entities.DebtDetailStatusHistory.OtyType);
          
      });

    entities.DebtDetailStatusHistory.DiscontinueDt = discontinueDt;
    entities.DebtDetailStatusHistory.Populated = true;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local;
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
  public class Local: IInitializable
  {
    /// <summary>
    /// A value of CommitPoint.
    /// </summary>
    [JsonPropertyName("commitPoint")]
    public Common CommitPoint
    {
      get => commitPoint ??= new();
      set => commitPoint = value;
    }

    /// <summary>
    /// A value of TestOnly.
    /// </summary>
    [JsonPropertyName("testOnly")]
    public Common TestOnly
    {
      get => testOnly ??= new();
      set => testOnly = value;
    }

    /// <summary>
    /// A value of ProvideFile.
    /// </summary>
    [JsonPropertyName("provideFile")]
    public Common ProvideFile
    {
      get => provideFile ??= new();
      set => provideFile = value;
    }

    /// <summary>
    /// A value of Old.
    /// </summary>
    [JsonPropertyName("old")]
    public DebtDetailStatusHistory Old
    {
      get => old ??= new();
      set => old = value;
    }

    /// <summary>
    /// A value of File.
    /// </summary>
    [JsonPropertyName("file")]
    public EabFileHandling File
    {
      get => file ??= new();
      set => file = value;
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

    /// <summary>
    /// A value of ForCommit.
    /// </summary>
    [JsonPropertyName("forCommit")]
    public Common ForCommit
    {
      get => forCommit ??= new();
      set => forCommit = value;
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
    /// A value of NeededToOpenEabReportSend.
    /// </summary>
    [JsonPropertyName("neededToOpenEabReportSend")]
    public EabReportSend NeededToOpenEabReportSend
    {
      get => neededToOpenEabReportSend ??= new();
      set => neededToOpenEabReportSend = value;
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
    /// A value of NeededToOpenEabFileHandling.
    /// </summary>
    [JsonPropertyName("neededToOpenEabFileHandling")]
    public EabFileHandling NeededToOpenEabFileHandling
    {
      get => neededToOpenEabFileHandling ??= new();
      set => neededToOpenEabFileHandling = value;
    }

    /// <summary>
    /// A value of DebtDetailsUpdated.
    /// </summary>
    [JsonPropertyName("debtDetailsUpdated")]
    public Common DebtDetailsUpdated
    {
      get => debtDetailsUpdated ??= new();
      set => debtDetailsUpdated = value;
    }

    /// <summary>
    /// A value of TotalEojDdsh.
    /// </summary>
    [JsonPropertyName("totalEojDdsh")]
    public Common TotalEojDdsh
    {
      get => totalEojDdsh ??= new();
      set => totalEojDdsh = value;
    }

    /// <summary>
    /// A value of NumberOfDebts.
    /// </summary>
    [JsonPropertyName("numberOfDebts")]
    public Common NumberOfDebts
    {
      get => numberOfDebts ??= new();
      set => numberOfDebts = value;
    }

    /// <summary>
    /// A value of NumberOfDdsh.
    /// </summary>
    [JsonPropertyName("numberOfDdsh")]
    public Common NumberOfDdsh
    {
      get => numberOfDdsh ??= new();
      set => numberOfDdsh = value;
    }

    /// <summary>
    /// A value of MostRecent.
    /// </summary>
    [JsonPropertyName("mostRecent")]
    public Common MostRecent
    {
      get => mostRecent ??= new();
      set => mostRecent = value;
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
    /// A value of DebtDetailStatusHistory.
    /// </summary>
    [JsonPropertyName("debtDetailStatusHistory")]
    public DebtDetailStatusHistory DebtDetailStatusHistory
    {
      get => debtDetailStatusHistory ??= new();
      set => debtDetailStatusHistory = value;
    }

    /// <summary>
    /// A value of DupDate.
    /// </summary>
    [JsonPropertyName("dupDate")]
    public Common DupDate
    {
      get => dupDate ??= new();
      set => dupDate = value;
    }

    /// <para>Resets the state.</para>
    void IInitializable.Initialize()
    {
      commitPoint = null;
      testOnly = null;
      provideFile = null;
      old = null;
      file = null;
      eabReportSend = null;
      programProcessingInfo = null;
      external = null;
      forCommit = null;
      eabFileHandling = null;
      neededToOpenEabReportSend = null;
      neededToWrite = null;
      neededToOpenEabFileHandling = null;
      debtDetailsUpdated = null;
      totalEojDdsh = null;
      numberOfDebts = null;
      numberOfDdsh = null;
      mostRecent = null;
      max = null;
      dupDate = null;
    }

    private Common commitPoint;
    private Common testOnly;
    private Common provideFile;
    private DebtDetailStatusHistory old;
    private EabFileHandling file;
    private EabReportSend eabReportSend;
    private ProgramProcessingInfo programProcessingInfo;
    private External external;
    private Common forCommit;
    private EabFileHandling eabFileHandling;
    private EabReportSend neededToOpenEabReportSend;
    private EabReportSend neededToWrite;
    private EabFileHandling neededToOpenEabFileHandling;
    private Common debtDetailsUpdated;
    private Common totalEojDdsh;
    private Common numberOfDebts;
    private Common numberOfDdsh;
    private Common mostRecent;
    private DateWorkArea max;
    private DebtDetailStatusHistory debtDetailStatusHistory;
    private Common dupDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of DebtDetailStatusHistory.
    /// </summary>
    [JsonPropertyName("debtDetailStatusHistory")]
    public DebtDetailStatusHistory DebtDetailStatusHistory
    {
      get => debtDetailStatusHistory ??= new();
      set => debtDetailStatusHistory = value;
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

    private CsePersonAccount csePersonAccount;
    private Obligation obligation;
    private ObligationTransaction debt;
    private CsePerson csePerson;
    private DebtDetailStatusHistory debtDetailStatusHistory;
    private DebtDetail debtDetail;
  }
#endregion
}
