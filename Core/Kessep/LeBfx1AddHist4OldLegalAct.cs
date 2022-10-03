// Program: LE_BFX1_ADD_HIST_4_OLD_LEGAL_ACT, ID: 373502103, model: 746.
// Short name: SWELFX1B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_BFX1_ADD_HIST_4_OLD_LEGAL_ACT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeBfx1AddHist4OldLegalAct: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_BFX1_ADD_HIST_4_OLD_LEGAL_ACT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeBfx1AddHist4OldLegalAct(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeBfx1AddHist4OldLegalAct.
  /// </summary>
  public LeBfx1AddHist4OldLegalAct(IContext context, Import import,
    Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    ExitState = "ACO_NN0000_ALL_OK";
    local.EabFileHandling.Action = "OPEN";
    local.Open.ProcessDate = Now().Date;
    local.Open.ProgramName = "SRRUNGV2";
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "SI0000_ERR_OPENING_CTL_RPT_FILE";

      return;
    }

    local.EabFileHandling.Action = "OPEN";
    local.Open.ProcessDate = Now().Date;
    local.Open.ProgramName = "SRRUNGV2";
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "SI0000_ERR_OPENING_ERR_RPT_FILE";

      return;
    }

    if (!ReadEvent())
    {
      ExitState = "SP0000_EVENT_NF";

      return;
    }

    local.Common.Count = 0;

    do
    {
      ++local.Common.Count;

      switch(local.Common.Count)
      {
        case 1:
          local.LegalAction.ActionTaken = "ACKOFPAT";

          break;
        case 2:
          local.LegalAction.ActionTaken = "AMREQNK";

          break;
        case 3:
          local.LegalAction.ActionTaken = "APANTBRF";

          break;
        case 4:
          local.LegalAction.ActionTaken = "APDOCST";

          break;
        case 5:
          local.LegalAction.ActionTaken = "APEEBRF";

          break;
        case 6:
          local.LegalAction.ActionTaken = "APPLNOT";

          break;
        case 7:
          local.LegalAction.ActionTaken = "APPOPN";

          break;
        case 8:
          local.LegalAction.ActionTaken = "BCHWARMO";

          break;
        case 9:
          local.LegalAction.ActionTaken = "BKRPCONF";

          break;
        case 10:
          local.LegalAction.ActionTaken = "BKRPDISM";

          break;
        case 11:
          local.LegalAction.ActionTaken = "BKRPDOD";

          break;
        case 12:
          local.LegalAction.ActionTaken = "BKRPNO13";

          break;
        case 13:
          local.LegalAction.ActionTaken = "BKRPNO7";

          break;
        case 14:
          local.LegalAction.ActionTaken = "BKRPPOC";

          break;
        case 15:
          local.LegalAction.ActionTaken = "COMPDISM";

          break;
        case 16:
          local.LegalAction.ActionTaken = "CONMODJ";

          break;
        case 17:
          local.LegalAction.ActionTaken = "CONTINEM";

          break;
        case 18:
          local.LegalAction.ActionTaken = "CONTINUE";

          break;
        case 19:
          local.LegalAction.ActionTaken = "CRIMINAL";

          break;
        case 20:
          local.LegalAction.ActionTaken = "CSUPARRA";

          break;
        case 21:
          local.LegalAction.ActionTaken = "CUSTODY";

          break;
        case 22:
          local.LegalAction.ActionTaken = "CUSTODYO";

          break;
        case 23:
          local.LegalAction.ActionTaken = "DEPOREQ";

          break;
        case 24:
          local.LegalAction.ActionTaken = "DISCLOSU";

          break;
        case 25:
          local.LegalAction.ActionTaken = "DRAFFDVT";

          break;
        case 26:
          local.LegalAction.ActionTaken = "EMPDEMND";

          break;
        case 27:
          local.LegalAction.ActionTaken = "ESTBCSMO";

          break;
        case 28:
          local.LegalAction.ActionTaken = "FOREIGNP";

          break;
        case 29:
          local.LegalAction.ActionTaken = "FORSUPRT";

          break;
        case 30:
          local.LegalAction.ActionTaken = "GENTEST";

          break;
        case 31:
          local.LegalAction.ActionTaken = "INTERPET";

          break;
        case 32:
          local.LegalAction.ActionTaken = "INTERSP";

          break;
        case 33:
          local.LegalAction.ActionTaken = "INTERST";

          break;
        case 34:
          local.LegalAction.ActionTaken = "INTERSTJ";

          break;
        case 35:
          local.LegalAction.ActionTaken = "IWONOTKM";

          break;
        case 36:
          local.LegalAction.ActionTaken = "IWONOTKS";

          break;
        case 37:
          local.LegalAction.ActionTaken = "IWONOTKT";

          break;
        case 38:
          local.LegalAction.ActionTaken = "JDGFRMJ";

          break;
        case 39:
          local.LegalAction.ActionTaken = "JEFBC";

          break;
        case 40:
          local.LegalAction.ActionTaken = "JEFMOD";

          break;
        case 41:
          local.LegalAction.ActionTaken = "JEMPIWOM";

          break;
        case 42:
          local.LegalAction.ActionTaken = "JENF";

          break;
        case 43:
          local.LegalAction.ActionTaken = "JSTAYM";

          break;
        case 44:
          local.LegalAction.ActionTaken = "LIMINEM";

          break;
        case 45:
          local.LegalAction.ActionTaken = "MEDONLYP";

          break;
        case 46:
          local.LegalAction.ActionTaken = "MEDSUPTJ";

          break;
        case 47:
          local.LegalAction.ActionTaken = "MODBC";

          break;
        case 48:
          local.LegalAction.ActionTaken = "MOTION";

          break;
        case 49:
          local.LegalAction.ActionTaken = "MOTOSTAY";

          break;
        case 50:
          local.LegalAction.ActionTaken = "PATMEDJ";

          break;
        case 51:
          local.LegalAction.ActionTaken = "PATMEDP";

          break;
        case 52:
          local.LegalAction.ActionTaken = "PETCSE";

          break;
        case 53:
          local.LegalAction.ActionTaken = "PETITION";

          break;
        case 54:
          local.LegalAction.ActionTaken = "PROCSRVI";

          break;
        case 55:
          local.LegalAction.ActionTaken = "PRODDOCR";

          break;
        case 56:
          local.LegalAction.ActionTaken = "PROTECTM";

          break;
        case 57:
          local.LegalAction.ActionTaken = "QUASHM";

          break;
        case 58:
          local.LegalAction.ActionTaken = "RECOVRYJ";

          break;
        case 59:
          local.LegalAction.ActionTaken = "RECOVRYP";

          break;
        case 60:
          local.LegalAction.ActionTaken = "REGENFNJ";

          break;
        case 61:
          local.LegalAction.ActionTaken = "REGMODNJ";

          break;
        case 62:
          local.LegalAction.ActionTaken = "REINSTAT";

          break;
        case 63:
          local.LegalAction.ActionTaken = "RULE170O";

          break;
        case 64:
          local.LegalAction.ActionTaken = "SETARRSJ";

          break;
        case 65:
          local.LegalAction.ActionTaken = "SETARRSM";

          break;
        case 66:
          local.LegalAction.ActionTaken = "SETARRXJ";

          break;
        case 67:
          local.LegalAction.ActionTaken = "SPECPROC";

          break;
        case 68:
          local.LegalAction.ActionTaken = "SUBPOENA";

          break;
        case 69:
          local.LegalAction.ActionTaken = "SUMMONS";

          break;
        case 70:
          local.LegalAction.ActionTaken = "VOLGENOR";

          break;
        case 71:
          local.LegalAction.ActionTaken = "VOLPATPK";

          break;
        case 72:
          local.LegalAction.ActionTaken = "VOLSUPPK";

          break;
        case 73:
          local.LegalAction.ActionTaken = "VOLUNLTR";

          break;
        case 74:
          local.LegalAction.ActionTaken = "VOL718PK";

          break;
        case 75:
          local.LegalAction.ActionTaken = "WITHDRWM";

          break;
        case 76:
          local.LegalAction.ActionTaken = "WITHDRWO";

          break;
        default:
          goto AfterCycle;
      }

      local.Code.CodeName = "ACTION TAKEN";
      local.CodeValue.Cdvalue = local.LegalAction.ActionTaken;
      UseCabGetCodeValueDescription();

      if (AsChar(local.ErrorInDecoding.Flag) == 'Y')
      {
        local.EabFileHandling.Action = "WRITE";
        local.Write.RptDetail =
          "Error retrieving description for action taken : " + local
          .LegalAction.ActionTaken;
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

          return;
        }

        continue;
      }

      local.EventDetail.ReasonCode = "A" + local.LegalAction.ActionTaken;

      if (!ReadEventDetail())
      {
        local.Find.Count = Find(local.CodeValue.Description, "  ");

        if (local.Find.Count == 0)
        {
          local.EventDetail.DetailName =
            TrimEnd(local.CodeValue.Description) + " ADDED";
          local.EventDetail.Description = "A " + TrimEnd
            (local.CodeValue.Description) + " HAS BEEN ADDED TO THE SYSTEM";
        }
        else
        {
          local.EventDetail.DetailName =
            Substring(local.CodeValue.Description, 1, local.Find.Count) + "ADDED"
            ;
          local.EventDetail.Description = "A " + Substring
            (local.CodeValue.Description, 1, local.Find.Count) + "HAS BEEN ADDED TO THE SYSTEM";
            
        }

        local.EventDetail.DetailName =
          Substring(local.EventDetail.DetailName, 1, 40);

        try
        {
          CreateEventDetail();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "SP0000_EVENT_DETAIL_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "SP0000_EVENT_DETAIL_AE";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      local.ThisActionTakenAdded.Count = 0;
      local.ThisActionTakenNoUnits.Count = 0;

      foreach(var item in ReadLegalAction())
      {
        // ----------------------------------------------------
        // Raise event 95
        // ----------------------------------------------------
        local.Infrastructure.SituationNumber = 0;
        local.Infrastructure.ReasonCode = "A" + entities
          .LegalAction.ActionTaken;
        local.DateWorkArea.Date = Date(entities.LegalAction.CreatedTstamp);
        UseLeBfx1GetRelCaseUnits();

        if (local.CaseUnits.Count > 0)
        {
          ++local.ThisActionTakenAdded.Count;
          local.Infrastructure.EventId = 95;
          local.Infrastructure.CsenetInOutCode = "";
          local.Infrastructure.ProcessStatus = "Q";
          local.Infrastructure.UserId = "LACT";
          local.Infrastructure.BusinessObjectCd = "LEA";
          local.Infrastructure.DenormNumeric12 =
            entities.LegalAction.Identifier;
          local.Infrastructure.DenormText12 =
            entities.LegalAction.CourtCaseNumber;
          local.Infrastructure.Detail = local.CodeValue.Description;
          local.Infrastructure.ReferenceDate =
            Date(entities.LegalAction.CreatedTstamp);
          local.Infrastructure.CreatedBy = entities.LegalAction.CreatedBy;
          local.Infrastructure.CreatedTimestamp =
            entities.LegalAction.CreatedTstamp;

          if (!IsEmpty(entities.LegalAction.InitiatingState) && !
            Equal(entities.LegalAction.InitiatingState, "KS"))
          {
            local.Infrastructure.InitiatingStateCode = "OS";
          }
          else
          {
            local.Infrastructure.InitiatingStateCode = "KS";
          }

          for(local.CaseUnits.Index = 0; local.CaseUnits.Index < local
            .CaseUnits.Count; ++local.CaseUnits.Index)
          {
            if (!local.CaseUnits.CheckSize())
            {
              break;
            }

            local.Infrastructure.CaseNumber =
              local.CaseUnits.Item.DetailCase.Number;
            local.Infrastructure.CaseUnitNumber =
              local.CaseUnits.Item.DetailCaseUnit.CuNumber;
            local.Infrastructure.CsePersonNumber =
              local.CaseUnits.Item.DetailObligor.Number;

            // @@@
            // @@@
            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              // @@@
              return;
            }
            else
            {
              // -------------------
              // Continue processing
              // -------------------
            }
          }

          local.CaseUnits.CheckIndex();
        }
        else
        {
          ++local.ThisActionTakenNoUnits.Count;

          // Write key info to the error report with message that no active case
          // units were found.
          local.EabFileHandling.Action = "WRITE";
          local.Write.RptDetail =
            Substring(local.LegalAction.ActionTaken,
            LegalAction.ActionTaken_MaxLength, 1, 8) + "  Standard Number: " + entities
            .LegalAction.StandardNumber + "  Legal action ID: " + NumberToString
            (entities.LegalAction.Identifier, 15) + "  -- No active case units.";
            
          UseCabErrorReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

            return;
          }
        }
      }

      local.EabFileHandling.Action = "WRITE";
      local.Write.RptDetail = "";
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

        return;
      }

      // --  Write the counts for this action taken to the control report.
      local.EabFileHandling.Action = "WRITE";
      local.Write.RptDetail = TrimEnd(local.LegalAction.ActionTaken) + " - " + local
        .CodeValue.Description;
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

        return;
      }

      local.EabFileHandling.Action = "WRITE";
      local.Write.RptDetail =
        "     -- Number of legal actions for which a HIST record was created: " +
        NumberToString(local.ThisActionTakenAdded.Count, 15);
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

        return;
      }

      local.EabFileHandling.Action = "WRITE";
      local.Write.RptDetail =
        "     -- Number of legal actions with no active case units          : " +
        NumberToString(local.ThisActionTakenNoUnits.Count, 15);
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

        return;
      }

      local.AllActionsTakenAdded.Count += local.ThisActionTakenAdded.Count;
      local.AllActionsTakenNoUnits.Count += local.ThisActionTakenNoUnits.Count;
    }
    while(!Equal(local.LegalAction.ActionTaken, "STOP"));

AfterCycle:

    local.EabFileHandling.Action = "WRITE";
    local.Write.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // --  Write the counts for all actions taken to the control report.
    local.EabFileHandling.Action = "WRITE";
    local.Write.RptDetail =
      "Total number of legal actions for which a HIST record was created: " + NumberToString
      (local.AllActionsTakenAdded.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    local.EabFileHandling.Action = "WRITE";
    local.Write.RptDetail =
      "Total number of legal actions for which a HIST record was not created because no active case units: " +
      NumberToString(local.AllActionsTakenNoUnits.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "SI0000_ERR_CLOSING_CTL_RPT_FILE";

      return;
    }

    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "SI0000_ERR_CLOSING_ERR_RPT_FILE";
    }
  }

  private static void MoveCode(Code source, Code target)
  {
    target.Id = source.Id;
    target.CodeName = source.CodeName;
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveRelatedCaseUnits(LeBfx1GetRelCaseUnits.Export.
    RelatedCaseUnitsGroup source, Local.CaseUnitsGroup target)
  {
    target.DetailCase.Number = source.DetailRelatedCase.Number;
    target.DetailCaseUnit.CuNumber = source.DetailRelatedCaseUnit.CuNumber;
    target.DetailObligor.Number = source.DtlRelatedObligor.Number;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.Write.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    MoveEabReportSend(local.Open, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport3()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.Write.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.Open, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabGetCodeValueDescription()
  {
    var useImport = new CabGetCodeValueDescription.Import();
    var useExport = new CabGetCodeValueDescription.Export();

    MoveCode(local.Code, useImport.Code);
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabGetCodeValueDescription.Execute, useImport, useExport);

    local.ErrorInDecoding.Flag = useExport.ErrorInDecoding.Flag;
    MoveCodeValue(useExport.CodeValue, local.CodeValue);
  }

  private void UseLeBfx1GetRelCaseUnits()
  {
    var useImport = new LeBfx1GetRelCaseUnits.Import();
    var useExport = new LeBfx1GetRelCaseUnits.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;
    useImport.LegalAction.Identifier = entities.LegalAction.Identifier;

    Call(LeBfx1GetRelCaseUnits.Execute, useImport, useExport);

    useExport.RelatedCaseUnits.CopyTo(local.CaseUnits, MoveRelatedCaseUnits);
  }

  private void CreateEventDetail()
  {
    var systemGeneratedIdentifier = 431 + local.Common.Count;
    var detailName = local.EventDetail.DetailName;
    var description = local.EventDetail.Description ?? "";
    var initiatingStateCode = "KS";
    var reasonCode = local.EventDetail.ReasonCode;
    var procedureName = "LACT";
    var lifecycleImpactCode = "N";
    var logToDiaryInd = "Y";
    var effectiveDate = new DateTime(1999, 3, 12);
    var discontinueDate = new DateTime(2099, 12, 31);
    var createdBy = "SWELBFX1";
    var createdTimestamp = Now();
    var eveNo = entities.Event1.ControlNumber;
    var function = "ENF";

    entities.EventDetail.Populated = false;
    Update("CreateEventDetail",
      (db, command) =>
      {
        db.SetInt32(command, "systemGeneratedI", systemGeneratedIdentifier);
        db.SetString(command, "detailName", detailName);
        db.SetNullableString(command, "description", description);
        db.SetString(command, "initiatingStCd", initiatingStateCode);
        db.SetString(command, "csenetInOutCode", "");
        db.SetString(command, "reasonCode", reasonCode);
        db.SetNullableString(command, "procedureName", procedureName);
        db.SetString(command, "lifecyclImpactCd", lifecycleImpactCode);
        db.SetString(command, "logToDiaryInd", logToDiaryInd);
        db.SetNullableInt32(command, "dateMonitorDays", 0);
        db.SetNullableInt32(command, "nextEventId", 0);
        db.SetNullableString(command, "nextEventDetail", "");
        db.SetString(command, "nextInitSt", "");
        db.SetNullableString(command, "nextCsenetIo", "");
        db.SetNullableString(command, "nextReason", "");
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdtdDtstamp", null);
        db.SetInt32(command, "eveNo", eveNo);
        db.SetNullableString(command, "function", function);
        db.SetNullableString(command, "exceptionRoutine", "");
      });

    entities.EventDetail.SystemGeneratedIdentifier = systemGeneratedIdentifier;
    entities.EventDetail.DetailName = detailName;
    entities.EventDetail.Description = description;
    entities.EventDetail.InitiatingStateCode = initiatingStateCode;
    entities.EventDetail.CsenetInOutCode = "";
    entities.EventDetail.ReasonCode = reasonCode;
    entities.EventDetail.ProcedureName = procedureName;
    entities.EventDetail.LifecycleImpactCode = lifecycleImpactCode;
    entities.EventDetail.LogToDiaryInd = logToDiaryInd;
    entities.EventDetail.DateMonitorDays = 0;
    entities.EventDetail.NextEventId = 0;
    entities.EventDetail.NextEventDetailId = "";
    entities.EventDetail.NextInitiatingState = "";
    entities.EventDetail.NextCsenetInOut = "";
    entities.EventDetail.NextReason = "";
    entities.EventDetail.EffectiveDate = effectiveDate;
    entities.EventDetail.DiscontinueDate = discontinueDate;
    entities.EventDetail.CreatedBy = createdBy;
    entities.EventDetail.CreatedTimestamp = createdTimestamp;
    entities.EventDetail.LastUpdatedBy = "";
    entities.EventDetail.LastUpdatedDtstamp = null;
    entities.EventDetail.EveNo = eveNo;
    entities.EventDetail.Function = function;
    entities.EventDetail.ExceptionRoutine = "";
    entities.EventDetail.Populated = true;
  }

  private bool ReadEvent()
  {
    entities.Event1.Populated = false;

    return Read("ReadEvent",
      null,
      (db, reader) =>
      {
        entities.Event1.ControlNumber = db.GetInt32(reader, 0);
        entities.Event1.Populated = true;
      });
  }

  private bool ReadEventDetail()
  {
    entities.EventDetail.Populated = false;

    return Read("ReadEventDetail",
      (db, command) =>
      {
        db.SetInt32(command, "eveNo", entities.Event1.ControlNumber);
        db.SetString(command, "reasonCode", local.EventDetail.ReasonCode);
      },
      (db, reader) =>
      {
        entities.EventDetail.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.EventDetail.DetailName = db.GetString(reader, 1);
        entities.EventDetail.Description = db.GetNullableString(reader, 2);
        entities.EventDetail.InitiatingStateCode = db.GetString(reader, 3);
        entities.EventDetail.CsenetInOutCode = db.GetString(reader, 4);
        entities.EventDetail.ReasonCode = db.GetString(reader, 5);
        entities.EventDetail.ProcedureName = db.GetNullableString(reader, 6);
        entities.EventDetail.LifecycleImpactCode = db.GetString(reader, 7);
        entities.EventDetail.LogToDiaryInd = db.GetString(reader, 8);
        entities.EventDetail.DateMonitorDays = db.GetNullableInt32(reader, 9);
        entities.EventDetail.NextEventId = db.GetNullableInt32(reader, 10);
        entities.EventDetail.NextEventDetailId =
          db.GetNullableString(reader, 11);
        entities.EventDetail.NextInitiatingState = db.GetString(reader, 12);
        entities.EventDetail.NextCsenetInOut = db.GetNullableString(reader, 13);
        entities.EventDetail.NextReason = db.GetNullableString(reader, 14);
        entities.EventDetail.EffectiveDate = db.GetDate(reader, 15);
        entities.EventDetail.DiscontinueDate = db.GetDate(reader, 16);
        entities.EventDetail.CreatedBy = db.GetString(reader, 17);
        entities.EventDetail.CreatedTimestamp = db.GetDateTime(reader, 18);
        entities.EventDetail.LastUpdatedBy = db.GetNullableString(reader, 19);
        entities.EventDetail.LastUpdatedDtstamp =
          db.GetNullableDateTime(reader, 20);
        entities.EventDetail.EveNo = db.GetInt32(reader, 21);
        entities.EventDetail.Function = db.GetNullableString(reader, 22);
        entities.EventDetail.ExceptionRoutine =
          db.GetNullableString(reader, 23);
        entities.EventDetail.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction",
      (db, command) =>
      {
        db.SetString(command, "actionTaken", local.LegalAction.ActionTaken);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.InitiatingState = db.GetNullableString(reader, 4);
        entities.LegalAction.RespondingState = db.GetNullableString(reader, 5);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 6);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 7);
        entities.LegalAction.CreatedBy = db.GetString(reader, 8);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 9);
        entities.LegalAction.EstablishmentCode =
          db.GetNullableString(reader, 10);
        entities.LegalAction.Populated = true;

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
    /// <summary>A CaseUnitsGroup group.</summary>
    [Serializable]
    public class CaseUnitsGroup
    {
      /// <summary>
      /// A value of DetailCase.
      /// </summary>
      [JsonPropertyName("detailCase")]
      public Case1 DetailCase
      {
        get => detailCase ??= new();
        set => detailCase = value;
      }

      /// <summary>
      /// A value of DetailCaseUnit.
      /// </summary>
      [JsonPropertyName("detailCaseUnit")]
      public CaseUnit DetailCaseUnit
      {
        get => detailCaseUnit ??= new();
        set => detailCaseUnit = value;
      }

      /// <summary>
      /// A value of DetailObligor.
      /// </summary>
      [JsonPropertyName("detailObligor")]
      public CsePerson DetailObligor
      {
        get => detailObligor ??= new();
        set => detailObligor = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Case1 detailCase;
      private CaseUnit detailCaseUnit;
      private CsePerson detailObligor;
    }

    /// <summary>
    /// Gets a value of CaseUnits.
    /// </summary>
    [JsonIgnore]
    public Array<CaseUnitsGroup> CaseUnits => caseUnits ??= new(
      CaseUnitsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of CaseUnits for json serialization.
    /// </summary>
    [JsonPropertyName("caseUnits")]
    [Computed]
    public IList<CaseUnitsGroup> CaseUnits_Json
    {
      get => caseUnits;
      set => CaseUnits.Assign(value);
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of ErrorInDecoding.
    /// </summary>
    [JsonPropertyName("errorInDecoding")]
    public Common ErrorInDecoding
    {
      get => errorInDecoding ??= new();
      set => errorInDecoding = value;
    }

    /// <summary>
    /// A value of Write.
    /// </summary>
    [JsonPropertyName("write")]
    public EabReportSend Write
    {
      get => write ??= new();
      set => write = value;
    }

    /// <summary>
    /// A value of Open.
    /// </summary>
    [JsonPropertyName("open")]
    public EabReportSend Open
    {
      get => open ??= new();
      set => open = value;
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
    /// A value of AllActionsTakenNoUnits.
    /// </summary>
    [JsonPropertyName("allActionsTakenNoUnits")]
    public Common AllActionsTakenNoUnits
    {
      get => allActionsTakenNoUnits ??= new();
      set => allActionsTakenNoUnits = value;
    }

    /// <summary>
    /// A value of ThisActionTakenNoUnits.
    /// </summary>
    [JsonPropertyName("thisActionTakenNoUnits")]
    public Common ThisActionTakenNoUnits
    {
      get => thisActionTakenNoUnits ??= new();
      set => thisActionTakenNoUnits = value;
    }

    /// <summary>
    /// A value of AllActionsTakenAdded.
    /// </summary>
    [JsonPropertyName("allActionsTakenAdded")]
    public Common AllActionsTakenAdded
    {
      get => allActionsTakenAdded ??= new();
      set => allActionsTakenAdded = value;
    }

    /// <summary>
    /// A value of ThisActionTakenAdded.
    /// </summary>
    [JsonPropertyName("thisActionTakenAdded")]
    public Common ThisActionTakenAdded
    {
      get => thisActionTakenAdded ??= new();
      set => thisActionTakenAdded = value;
    }

    /// <summary>
    /// A value of Find.
    /// </summary>
    [JsonPropertyName("find")]
    public Common Find
    {
      get => find ??= new();
      set => find = value;
    }

    /// <summary>
    /// A value of EventDetail.
    /// </summary>
    [JsonPropertyName("eventDetail")]
    public EventDetail EventDetail
    {
      get => eventDetail ??= new();
      set => eventDetail = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
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

    private Array<CaseUnitsGroup> caseUnits;
    private DateWorkArea dateWorkArea;
    private Common errorInDecoding;
    private EabReportSend write;
    private EabReportSend open;
    private EabFileHandling eabFileHandling;
    private Common allActionsTakenNoUnits;
    private Common thisActionTakenNoUnits;
    private Common allActionsTakenAdded;
    private Common thisActionTakenAdded;
    private Common find;
    private EventDetail eventDetail;
    private Code code;
    private CodeValue codeValue;
    private LegalAction legalAction;
    private Common common;
    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of EventDetail.
    /// </summary>
    [JsonPropertyName("eventDetail")]
    public EventDetail EventDetail
    {
      get => eventDetail ??= new();
      set => eventDetail = value;
    }

    /// <summary>
    /// A value of Event1.
    /// </summary>
    [JsonPropertyName("event1")]
    public Event1 Event1
    {
      get => event1 ??= new();
      set => event1 = value;
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

    private EventDetail eventDetail;
    private Event1 event1;
    private LegalAction legalAction;
  }
#endregion
}
