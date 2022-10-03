﻿// Program: FN_SRRUN220_GEN_REPORT_FILE, ID: 372799472, model: 746.
// Short name: SWEF220B
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
/// A program: FN_SRRUN220_GEN_REPORT_FILE.
/// </para>
/// <para>
/// This is a report that provides obligation/obligor suppression information.  
/// For each suppressed obligation found, the responsible worker is retrieved,
/// as well as the supervisor and office.  This information will be sorted, and
/// a report generated by SWEFB221.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnSrrun220GenReportFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_SRRUN220_GEN_REPORT_FILE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnSrrun220GenReportFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnSrrun220GenReportFile.
  /// </summary>
  public FnSrrun220GenReportFile(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // THIS IS THE DATA RETRIEVAL PROGRAM FOR THE OBLIGATION SUPPRESSION
    // REPORT BY WORKER
    // This job uses 3 Report Composer EABs: a file generator, a file reader, 
    // and a report generator.  If you change any eab view, you must make the
    // same change in the other 2 eabs, regenerate the stubs (on the
    // workstation, in Cobol), and regenerate the eab source code in Report
    // Composer for each eab.
    ExitState = "ACO_NN0000_ALL_OK";
    local.NeededToOpen.ProgramName = "SWEFB220";
    local.ProgramProcessingInfo.Name = "SWEFB220";
    local.TypeObligor.Type1 = "R";
    local.TypeObligation.Type1 = "O";
    local.Max.Date = new DateTime(2099, 12, 31);
    local.Current.Date = Now().Date;
    local.CanamSend.Parm2 = "";

    // : Open Error File
    local.EabFileHandling.Action = "OPEN";
    local.NeededToOpen.ProcessDate = local.Current.Date;
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }
    else
    {
      local.EabFileHandling.Action = "WRITE";
    }

    UseReadProgramProcessingInfo();

    // : Check exitstate from read program process cab.
    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabExtractExitStateMessage();
      local.NeededToWrite.RptDetail = local.ExitStateWorkArea.Message;
      UseCabErrorReport3();
      local.EabFileHandling.Action = "CLOSE";
      UseCabErrorReport4();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // : Open the CANAM output data file.
    local.CanamSend.Parm1 = "OF";
    UseFnEabGenerateRpt220File();

    if (!IsEmpty(local.CanamReturn.Parm1))
    {
      // Parm 2 contains the file status
      // :output error here
      local.NeededToWrite.RptDetail =
        "Problem opening report file.  File status is  " + local
        .CanamReturn.Parm2;
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // : Read suppressed statements and coupons by obligation.
    foreach(var item in ReadStmtCouponSuppStatusHist())
    {
      if (Equal(entities.StmtCouponSuppStatusHist.CreatedBy, "CONVERSN"))
      {
        continue;
      }

      if (ReadObligor())
      {
        if (ReadCsePerson())
        {
          local.CsePersonsWorkSet.Number = entities.Obligor2.Number;
          UseSiReadCsePersonBatch();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabExtractExitStateMessage();
            local.NeededToWrite.RptDetail = local.AbendData.AdabasResponseCd + local
              .CsePersonsWorkSet.Number + local.ExitStateWorkArea.Message;
            UseCabErrorReport3();

            break;
          }
        }
        else
        {
          ExitState = "FN0000_OBLIGOR_NF";

          break;
        }
      }
      else
      {
        ExitState = "CSE_PERSON_NF";

        break;
      }

      if (AsChar(entities.StmtCouponSuppStatusHist.Type1) == 'O')
      {
        // : Suppression is on an obligation level.
        if (ReadObligation1())
        {
          // : Call cab to retrieve the rest of the data.
          UseFnSrrun220RetrieveReportData();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            // : Write a message to the error report and reset the exitstate
            //   to continue processing.
            UseEabExtractExitStateMessage();
            UseCabErrorReport3();
            ExitState = "ACO_NN0000_ALL_OK";
          }
        }
        else
        {
          ExitState = "FN0000_OBLIGATION_NF";
          local.NeededToWrite.RptDetail =
            TrimEnd(local.ExitStateWorkArea.Message) + "    Obligor:  " + entities
            .Obligor2.Number + "    Obligation:  " + NumberToString
            (entities.Obligation.SystemGeneratedIdentifier, 15);
          UseCabErrorReport3();
          ExitState = "ACO_NN0000_ALL_OK";
        }
      }
      else if (AsChar(entities.StmtCouponSuppStatusHist.Type1) == 'R')
      {
        // : Suppression is by obligor.
        foreach(var item1 in ReadObligation2())
        {
          if (ReadDebtDetail())
          {
            // Continue
          }
          else
          {
            // : They may have an active accruing obligation, even if
            //   there is no debt detail with an amount due.
            if (ReadAccrualInstructions())
            {
              // Continue
            }
            else
            {
              // : This obligation is not active, so skip it.
              continue;
            }
          }

          // : Call cab to retrieve the rest of the data.
          UseFnSrrun220RetrieveReportData();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabExtractExitStateMessage();
            local.NeededToWrite.RptDetail =
              TrimEnd(local.ExitStateWorkArea.Message) + "    Obligor:  " + entities
              .Obligor2.Number + "    Obligation:  " + NumberToString
              (entities.Obligation.SystemGeneratedIdentifier, 15);
            UseCabErrorReport3();
            ExitState = "ACO_NN0000_ALL_OK";
          }
        }
      }
    }

    // : Close the CANAM output data file.
    local.CanamSend.Parm1 = "CF";
    UseFnEabGenerateRpt220File();

    if (!IsEmpty(local.CanamReturn.Parm1))
    {
      // : Error
      local.NeededToWrite.RptDetail =
        "Problem closing report file.  File status is  " + local
        .CanamReturn.Parm2;
      UseCabErrorReport3();
    }

    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport4();
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveReportParms(ReportParms source, ReportParms target)
  {
    target.Parm1 = source.Parm1;
    target.Parm2 = source.Parm2;
  }

  private static void MoveStmtCouponSuppStatusHist(
    StmtCouponSuppStatusHist source, StmtCouponSuppStatusHist target)
  {
    target.Type1 = source.Type1;
    target.EffectiveDate = source.EffectiveDate;
    target.ReasonText = source.ReasonText;
    target.CreatedBy = source.CreatedBy;
    target.LastUpdatedBy = source.LastUpdatedBy;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport4()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseFnEabGenerateRpt220File()
  {
    var useImport = new FnEabGenerateRpt220File.Import();
    var useExport = new FnEabGenerateRpt220File.Export();

    MoveReportParms(local.CanamSend, useImport.ReportParms);
    MoveReportParms(local.CanamReturn, useExport.ReportParms);

    Call(FnEabGenerateRpt220File.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.CanamReturn);
  }

  private void UseFnSrrun220RetrieveReportData()
  {
    var useImport = new FnSrrun220RetrieveReportData.Import();
    var useExport = new FnSrrun220RetrieveReportData.Export();

    useImport.PersistentCsePerson.Assign(entities.Obligor2);
    useImport.PersistentObligation.Assign(entities.Obligation);
    MoveStmtCouponSuppStatusHist(entities.StmtCouponSuppStatusHist,
      useImport.StmtCouponSuppStatusHist);
    MoveCsePersonsWorkSet(local.CsePersonsWorkSet, useImport.CsePersonsWorkSet);

    Call(FnSrrun220RetrieveReportData.Execute, useImport, useExport);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseSiReadCsePersonBatch()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
  }

  private bool ReadAccrualInstructions()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.AccrualInstructions.Populated = false;

    return Read("ReadAccrualInstructions",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetDate(command, "asOfDt", date);
      },
      (db, reader) =>
      {
        entities.AccrualInstructions.OtrType = db.GetString(reader, 0);
        entities.AccrualInstructions.OtyId = db.GetInt32(reader, 1);
        entities.AccrualInstructions.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.AccrualInstructions.CspNumber = db.GetString(reader, 3);
        entities.AccrualInstructions.CpaType = db.GetString(reader, 4);
        entities.AccrualInstructions.OtrGeneratedId = db.GetInt32(reader, 5);
        entities.AccrualInstructions.AsOfDt = db.GetDate(reader, 6);
        entities.AccrualInstructions.DiscontinueDt =
          db.GetNullableDate(reader, 7);
        entities.AccrualInstructions.Populated = true;
        CheckValid<AccrualInstructions>("OtrType",
          entities.AccrualInstructions.OtrType);
        CheckValid<AccrualInstructions>("CpaType",
          entities.AccrualInstructions.CpaType);
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor1.Populated);
    entities.Obligor2.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.Obligor1.CspNumber);
      },
      (db, reader) =>
      {
        entities.Obligor2.Number = db.GetString(reader, 0);
        entities.Obligor2.Populated = true;
      });
  }

  private bool ReadDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail",
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
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 6);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private bool ReadObligation1()
  {
    System.Diagnostics.Debug.
      Assert(entities.StmtCouponSuppStatusHist.Populated);
    entities.Obligation.Populated = false;

    return Read("ReadObligation1",
      (db, command) =>
      {
        db.SetInt32(
          command, "obId",
          entities.StmtCouponSuppStatusHist.ObgId.GetValueOrDefault());
        db.SetString(
          command, "cspNumber",
          entities.StmtCouponSuppStatusHist.CspNumberOblig ?? "");
        db.SetString(
          command, "cpaType",
          entities.StmtCouponSuppStatusHist.CpaTypeOblig ?? "");
        db.SetInt32(
          command, "dtyGeneratedId",
          entities.StmtCouponSuppStatusHist.OtyId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
      });
  }

  private IEnumerable<bool> ReadObligation2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor1.Populated);
    entities.Obligation.Populated = false;

    return ReadEach("ReadObligation2",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligor1.Type1);
        db.SetString(command, "cspNumber", entities.Obligor1.CspNumber);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);

        return true;
      });
  }

  private bool ReadObligor()
  {
    System.Diagnostics.Debug.
      Assert(entities.StmtCouponSuppStatusHist.Populated);
    entities.Obligor1.Populated = false;

    return Read("ReadObligor",
      (db, command) =>
      {
        db.
          SetString(command, "type", entities.StmtCouponSuppStatusHist.CpaType);
          
        db.SetString(
          command, "cspNumber", entities.StmtCouponSuppStatusHist.CspNumber);
      },
      (db, reader) =>
      {
        entities.Obligor1.CspNumber = db.GetString(reader, 0);
        entities.Obligor1.Type1 = db.GetString(reader, 1);
        entities.Obligor1.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Obligor1.Type1);
      });
  }

  private IEnumerable<bool> ReadStmtCouponSuppStatusHist()
  {
    entities.StmtCouponSuppStatusHist.Populated = false;

    return ReadEach("ReadStmtCouponSuppStatusHist",
      (db, command) =>
      {
        db.SetDate(
          command, "discontinueDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.StmtCouponSuppStatusHist.CpaType = db.GetString(reader, 0);
        entities.StmtCouponSuppStatusHist.CspNumber = db.GetString(reader, 1);
        entities.StmtCouponSuppStatusHist.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.StmtCouponSuppStatusHist.Type1 = db.GetString(reader, 3);
        entities.StmtCouponSuppStatusHist.EffectiveDate = db.GetDate(reader, 4);
        entities.StmtCouponSuppStatusHist.DiscontinueDate =
          db.GetDate(reader, 5);
        entities.StmtCouponSuppStatusHist.ReasonText =
          db.GetNullableString(reader, 6);
        entities.StmtCouponSuppStatusHist.CreatedBy = db.GetString(reader, 7);
        entities.StmtCouponSuppStatusHist.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.StmtCouponSuppStatusHist.OtyId =
          db.GetNullableInt32(reader, 9);
        entities.StmtCouponSuppStatusHist.CpaTypeOblig =
          db.GetNullableString(reader, 10);
        entities.StmtCouponSuppStatusHist.CspNumberOblig =
          db.GetNullableString(reader, 11);
        entities.StmtCouponSuppStatusHist.ObgId =
          db.GetNullableInt32(reader, 12);
        entities.StmtCouponSuppStatusHist.DocTypeToSuppress =
          db.GetString(reader, 13);
        entities.StmtCouponSuppStatusHist.Populated = true;
        CheckValid<StmtCouponSuppStatusHist>("CpaType",
          entities.StmtCouponSuppStatusHist.CpaType);
        CheckValid<StmtCouponSuppStatusHist>("Type1",
          entities.StmtCouponSuppStatusHist.Type1);
        CheckValid<StmtCouponSuppStatusHist>("CpaTypeOblig",
          entities.StmtCouponSuppStatusHist.CpaTypeOblig);
        CheckValid<StmtCouponSuppStatusHist>("DocTypeToSuppress",
          entities.StmtCouponSuppStatusHist.DocTypeToSuppress);

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
    /// A value of TypeObligor.
    /// </summary>
    [JsonPropertyName("typeObligor")]
    public StmtCouponSuppStatusHist TypeObligor
    {
      get => typeObligor ??= new();
      set => typeObligor = value;
    }

    /// <summary>
    /// A value of TypeObligation.
    /// </summary>
    [JsonPropertyName("typeObligation")]
    public StmtCouponSuppStatusHist TypeObligation
    {
      get => typeObligation ??= new();
      set => typeObligation = value;
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
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public CsePerson Prev
    {
      get => prev ??= new();
      set => prev = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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
    /// A value of CanamSend.
    /// </summary>
    [JsonPropertyName("canamSend")]
    public ReportParms CanamSend
    {
      get => canamSend ??= new();
      set => canamSend = value;
    }

    /// <summary>
    /// A value of CanamReturn.
    /// </summary>
    [JsonPropertyName("canamReturn")]
    public ReportParms CanamReturn
    {
      get => canamReturn ??= new();
      set => canamReturn = value;
    }

    private ExitStateWorkArea exitStateWorkArea;
    private StmtCouponSuppStatusHist typeObligor;
    private StmtCouponSuppStatusHist typeObligation;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson prev;
    private AbendData abendData;
    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea null1;
    private DateWorkArea current;
    private DateWorkArea max;
    private EabReportSend neededToOpen;
    private EabReportSend neededToWrite;
    private EabFileHandling eabFileHandling;
    private ReportParms canamSend;
    private ReportParms canamReturn;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Obligor1.
    /// </summary>
    [JsonPropertyName("obligor1")]
    public CsePersonAccount Obligor1
    {
      get => obligor1 ??= new();
      set => obligor1 = value;
    }

    /// <summary>
    /// A value of Obligor2.
    /// </summary>
    [JsonPropertyName("obligor2")]
    public CsePerson Obligor2
    {
      get => obligor2 ??= new();
      set => obligor2 = value;
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
    /// A value of StmtCouponSuppStatusHist.
    /// </summary>
    [JsonPropertyName("stmtCouponSuppStatusHist")]
    public StmtCouponSuppStatusHist StmtCouponSuppStatusHist
    {
      get => stmtCouponSuppStatusHist ??= new();
      set => stmtCouponSuppStatusHist = value;
    }

    private AccrualInstructions accrualInstructions;
    private ObligationTransaction debt;
    private DebtDetail debtDetail;
    private CsePersonAccount obligor1;
    private CsePerson obligor2;
    private Obligation obligation;
    private StmtCouponSuppStatusHist stmtCouponSuppStatusHist;
  }
#endregion
}
