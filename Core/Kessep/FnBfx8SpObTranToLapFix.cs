// Program: FN_BFX8_SP_OB_TRAN_TO_LAP_FIX, ID: 372965379, model: 746.
// Short name: SWEBFX8P
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_BFX8_SP_OB_TRAN_TO_LAP_FIX.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnBfx8SpObTranToLapFix: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_BFX8_SP_OB_TRAN_TO_LAP_FIX program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnBfx8SpObTranToLapFix(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnBfx8SpObTranToLapFix.
  /// </summary>
  public FnBfx8SpObTranToLapFix(IContext context, Import import, Export export):
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
    local.MaxDiscontinue.Date = UseCabSetMaximumDiscontinueDate();
    local.UserId.Text8 = global.UserId;
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();
    local.EabReportSend.ProcessDate = Now().Date;
    local.EabReportSend.ProgramName = local.UserId.Text8;
    UseFnExtGetParmsThruJclSysin();

    if (local.External.NumericReturnCode != 0)
    {
      ExitState = "FN0000_SYSIN_PARM_ERROR_A";

      return;
    }

    if (IsEmpty(local.ProgramProcessingInfo.ParameterList))
    {
      ExitState = "FN0000_SYSIN_PARM_ERROR_A";

      return;
    }

    local.ProcessUpdatesInd.Flag =
      Substring(local.ProgramProcessingInfo.ParameterList, 1, 1);

    if (IsEmpty(local.ProcessUpdatesInd.Flag))
    {
      local.ProcessUpdatesInd.Flag = "N";
    }

    local.EabFileHandling.Action = "OPEN";

    // **********************************************************
    // OPEN OUTPUT CONTROL REPORT 98
    // **********************************************************
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    // **********************************************************
    // OPEN OUTPUT ERROR REPORT 99
    // **********************************************************
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_ERROR_RPT_A";

      return;
    }

    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail = "PARMS:";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabReportSend.RptDetail = "Perform Updates . . . . . . . : " + local
      .ProcessUpdatesInd.Flag;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabReportSend.RptDetail =
      "ACTION         OBLIGOR  OB-ID      SPOUSE  CRT ORD               FR-LAP-ID  TO-LAP-ID  ERROR TEXT";
      
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    foreach(var item in ReadCsePersonObligationDebtCsePerson())
    {
      ++local.ReadCnt.Count;

      if (ReadLegalActionPerson1())
      {
        continue;
      }
      else
      {
        // : ERROR - Fix It!!!!
      }

      local.ObId.Text4 =
        NumberToString(entities.ExistingObligation.SystemGeneratedIdentifier,
        13, 3);
      local.CorrectLap.Text9 = "      N/A";
      local.IncorrectLap.Text9 = "      N/A";

      if (!ReadLegalAction())
      {
        ++local.ErrorCnt.Count;
        local.EabReportSend.RptDetail = "**ERROR***" + "  " + entities
          .ExistingObligor1.Number + "   " + local.ObId.Text4 + "  " + entities
          .ExistingSupported1.Number + "  " + entities
          .ExistingLegalAction.StandardNumber + "  " + local
          .IncorrectLap.Text9 + "  " + local.CorrectLap.Text9 + "  " + "LEGAL ACTION NF";
          
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        continue;
      }

      if (ReadLegalActionPerson2())
      {
        local.Tmp.Count = entities.Correct.Identifier;
        UseCabTextnum1();
      }
      else
      {
        ++local.ErrorCnt.Count;
        local.EabReportSend.RptDetail = "**ERROR***" + "  " + entities
          .ExistingObligor1.Number + "   " + local.ObId.Text4 + "  " + entities
          .ExistingSupported1.Number + "  " + entities
          .ExistingLegalAction.StandardNumber + "  " + local
          .IncorrectLap.Text9 + "  " + local.CorrectLap.Text9 + "  " + "CORRECT LEGAL ACTION PERSON NF";
          
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        continue;
      }

      if (ReadLegalActionPerson3())
      {
        local.Tmp.Count = entities.Incorrect.Identifier;
        UseCabTextnum2();
      }
      else
      {
        ++local.ErrorCnt.Count;
        local.EabReportSend.RptDetail = "**ERROR***" + "  " + entities
          .ExistingObligor1.Number + "   " + local.ObId.Text4 + "  " + entities
          .ExistingSupported1.Number + "  " + entities
          .ExistingLegalAction.StandardNumber + "  " + local
          .IncorrectLap.Text9 + "  " + local.CorrectLap.Text9 + "  " + "INCORRECT LEGAL ACTION PERSON NF";
          
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        continue;
      }

      ++local.UpdateCnt.Count;
      local.EabReportSend.RptDetail = "UPDATE    " + "  " + entities
        .ExistingObligor1.Number + "   " + local.ObId.Text4 + "  " + entities
        .ExistingSupported1.Number + "  " + entities
        .ExistingLegalAction.StandardNumber + "  " + local
        .IncorrectLap.Text9 + "  " + local.CorrectLap.Text9 + "  " + "SUCCESSFULLY UPDATED";
        
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      // : Process the Adjustment?
      if (AsChar(local.ProcessUpdatesInd.Flag) == 'Y')
      {
        try
        {
          UpdateDebt2();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_OBLIG_TRANS_NU_RB";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_OBLIG_TRANS_PV_RB";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        try
        {
          UpdateDebt1();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_OBLIG_TRANS_NU_RB";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_OBLIG_TRANS_PV_RB";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }

    // : Print Summary Totals
    UseCabTextnum3();
    local.EabReportSend.RptDetail = "Read Count . . . . . . . . . . . . : " + local
      .WorkArea.Text9;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    UseCabTextnum4();
    local.EabReportSend.RptDetail = "Update Count . . . . . . . . . . . : " + local
      .WorkArea.Text9;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    UseCabTextnum4();
    local.EabReportSend.RptDetail = "Error Count. . . . . . . . . . . . : " + local
      .WorkArea.Text9;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabFileHandling.Action = "CLOSE";

    // **********************************************************
    // CLOSE OUTPUT CONTROL REPORT 98
    // **********************************************************
    UseCabControlReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    // **********************************************************
    // CLOSE OUTPUT ERROR REPORT 99
    // **********************************************************
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_ERROR_RPT_A";
    }
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

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
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

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
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

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseCabTextnum1()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.Tmp.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.CorrectLap.Text9 = useExport.TextNumber.Text9;
  }

  private void UseCabTextnum2()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.Tmp.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.IncorrectLap.Text9 = useExport.TextNumber.Text9;
  }

  private void UseCabTextnum3()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.ReadCnt.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.WorkArea.Text9 = useExport.TextNumber.Text9;
  }

  private void UseCabTextnum4()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.UpdateCnt.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.WorkArea.Text9 = useExport.TextNumber.Text9;
  }

  private void UseFnExtGetParmsThruJclSysin()
  {
    var useImport = new FnExtGetParmsThruJclSysin.Import();
    var useExport = new FnExtGetParmsThruJclSysin.Export();

    useExport.External.NumericReturnCode = local.External.NumericReturnCode;
    useExport.ProgramProcessingInfo.ParameterList =
      local.ProgramProcessingInfo.ParameterList;

    Call(FnExtGetParmsThruJclSysin.Execute, useImport, useExport);

    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
    local.ProgramProcessingInfo.ParameterList =
      useExport.ProgramProcessingInfo.ParameterList;
  }

  private IEnumerable<bool> ReadCsePersonObligationDebtCsePerson()
  {
    entities.ExistingObligor1.Populated = false;
    entities.ExistingSupported1.Populated = false;
    entities.ExistingObligation.Populated = false;
    entities.ExistingDebt.Populated = false;

    return ReadEach("ReadCsePersonObligationDebtCsePerson",
      null,
      (db, reader) =>
      {
        entities.ExistingObligor1.Number = db.GetString(reader, 0);
        entities.ExistingObligation.CspNumber = db.GetString(reader, 0);
        entities.ExistingDebt.CspNumber = db.GetString(reader, 0);
        entities.ExistingDebt.CspNumber = db.GetString(reader, 0);
        entities.ExistingObligation.CpaType = db.GetString(reader, 1);
        entities.ExistingDebt.CpaType = db.GetString(reader, 1);
        entities.ExistingObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingDebt.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.ExistingObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ExistingDebt.OtyType = db.GetInt32(reader, 3);
        entities.ExistingObligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.ExistingDebt.SystemGeneratedIdentifier =
          db.GetInt32(reader, 5);
        entities.ExistingDebt.Type1 = db.GetString(reader, 6);
        entities.ExistingDebt.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.ExistingDebt.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 8);
        entities.ExistingDebt.CspSupNumber = db.GetNullableString(reader, 9);
        entities.ExistingSupported1.Number = db.GetString(reader, 9);
        entities.ExistingDebt.CpaSupType = db.GetNullableString(reader, 10);
        entities.ExistingDebt.LapId = db.GetNullableInt32(reader, 11);
        entities.ExistingObligor1.Populated = true;
        entities.ExistingSupported1.Populated = true;
        entities.ExistingObligation.Populated = true;
        entities.ExistingDebt.Populated = true;

        return true;
      });
  }

  private bool ReadLegalAction()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);
    entities.ExistingLegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.ExistingObligation.LgaId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.StandardNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingLegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionPerson1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingDebt.Populated);
    entities.Check.Populated = false;

    return Read("ReadLegalActionPerson1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNumber", entities.ExistingSupported1.Number);
        db.SetInt32(
          command, "laPersonId",
          entities.ExistingDebt.LapId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Check.Identifier = db.GetInt32(reader, 0);
        entities.Check.CspNumber = db.GetNullableString(reader, 1);
        entities.Check.Populated = true;
      });
  }

  private bool ReadLegalActionPerson2()
  {
    entities.Correct.Populated = false;

    return Read("ReadLegalActionPerson2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNumber", entities.ExistingSupported1.Number);
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.ExistingLegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Correct.Identifier = db.GetInt32(reader, 0);
        entities.Correct.CspNumber = db.GetNullableString(reader, 1);
        entities.Correct.LgaIdentifier = db.GetNullableInt32(reader, 2);
        entities.Correct.Populated = true;
      });
  }

  private bool ReadLegalActionPerson3()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingDebt.Populated);
    entities.Incorrect.Populated = false;

    return Read("ReadLegalActionPerson3",
      (db, command) =>
      {
        db.SetInt32(
          command, "laPersonId",
          entities.ExistingDebt.LapId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Incorrect.Identifier = db.GetInt32(reader, 0);
        entities.Incorrect.Populated = true;
      });
  }

  private void UpdateDebt1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingDebt.Populated);

    var lastUpdatedBy = local.UserId.Text8;
    var lastUpdatedTmst = local.Current.Timestamp;
    var lapId = entities.Correct.Identifier;

    entities.ExistingDebt.Populated = false;
    Update("UpdateDebt1",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableInt32(command, "lapId", lapId);
        db.SetInt32(
          command, "obgGeneratedId", entities.ExistingDebt.ObgGeneratedId);
        db.SetString(command, "cspNumber", entities.ExistingDebt.CspNumber);
        db.SetString(command, "cpaType", entities.ExistingDebt.CpaType);
        db.SetInt32(
          command, "obTrnId", entities.ExistingDebt.SystemGeneratedIdentifier);
        db.SetString(command, "obTrnTyp", entities.ExistingDebt.Type1);
        db.SetInt32(command, "otyType", entities.ExistingDebt.OtyType);
      });

    entities.ExistingDebt.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingDebt.LastUpdatedTmst = lastUpdatedTmst;
    entities.ExistingDebt.LapId = lapId;
    entities.ExistingDebt.Populated = true;
  }

  private void UpdateDebt2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingDebt.Populated);

    var obgGeneratedId = entities.ExistingDebt.ObgGeneratedId;
    var cspNumber = entities.ExistingDebt.CspNumber;
    var cpaType = entities.ExistingDebt.CpaType;
    var otyType = entities.ExistingDebt.OtyType;

    entities.ExistingDebt.Populated = false;

    bool exists;

    Update("UpdateDebt2#1",
      (db, command) =>
      {
        db.SetInt32(command, "obgGeneratedId1", obgGeneratedId);
        db.SetString(command, "cspNumber1", cspNumber);
        db.SetString(command, "cpaType1", cpaType);
        db.SetInt32(
          command, "obTrnId", entities.ExistingDebt.SystemGeneratedIdentifier);
        db.SetString(command, "obTrnTyp", entities.ExistingDebt.Type1);
        db.SetInt32(command, "otyType1", otyType);
      });

    exists = Read("UpdateDebt2#2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cpaSupType", entities.ExistingDebt.CpaSupType ?? "");
        db.SetNullableString(
          command, "cspSupNumber", entities.ExistingDebt.CspSupNumber ?? "");
      },
      null);

    if (!exists)
    {
      Update("UpdateDebt2#3",
        (db, command) =>
        {
          db.SetNullableString(
            command, "cpaSupType", entities.ExistingDebt.CpaSupType ?? "");
          db.SetNullableString(
            command, "cspSupNumber", entities.ExistingDebt.CspSupNumber ?? "");
        });
    }

    exists = Read("UpdateDebt2#4",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cpaSupType", entities.ExistingDebt.CpaSupType ?? "");
        db.SetNullableString(
          command, "cspSupNumber", entities.ExistingDebt.CspSupNumber ?? "");
      },
      null);

    if (!exists)
    {
      Update("UpdateDebt2#5",
        (db, command) =>
        {
          db.SetNullableString(
            command, "cpaSupType", entities.ExistingDebt.CpaSupType ?? "");
          db.SetNullableString(
            command, "cspSupNumber", entities.ExistingDebt.CspSupNumber ?? "");
        });
    }

    exists = Read("UpdateDebt2#6",
      (db, command) =>
      {
        db.SetInt32(command, "otyType2", otyType);
        db.SetInt32(command, "obgGeneratedId2", obgGeneratedId);
        db.SetString(command, "cspNumber2", cspNumber);
        db.SetString(command, "cpaType2", cpaType);
      },
      null);

    if (!exists)
    {
      Update("UpdateDebt2#7",
        (db, command) =>
        {
          db.SetInt32(command, "otyType2", otyType);
          db.SetInt32(command, "obgGeneratedId2", obgGeneratedId);
          db.SetString(command, "cspNumber2", cspNumber);
          db.SetString(command, "cpaType2", cpaType);
        });

      exists = Read("UpdateDebt2#8",
        (db, command) =>
        {
          db.SetInt32(command, "otyType2", otyType);
          db.SetInt32(command, "obgGeneratedId2", obgGeneratedId);
          db.SetString(command, "cspNumber2", cspNumber);
          db.SetString(command, "cpaType2", cpaType);
        },
        null);

      if (!exists)
      {
        Update("UpdateDebt2#9",
          (db, command) =>
          {
            db.SetString(command, "cpaType2", cspNumber);
            db.SetString(command, "cspNumber2", cpaType);
          });
      }
    }

    entities.ExistingDebt.LapId = null;
    entities.ExistingDebt.Populated = true;
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
    /// A value of Tmp.
    /// </summary>
    [JsonPropertyName("tmp")]
    public Common Tmp
    {
      get => tmp ??= new();
      set => tmp = value;
    }

    /// <summary>
    /// A value of CorrectLap.
    /// </summary>
    [JsonPropertyName("correctLap")]
    public WorkArea CorrectLap
    {
      get => correctLap ??= new();
      set => correctLap = value;
    }

    /// <summary>
    /// A value of IncorrectLap.
    /// </summary>
    [JsonPropertyName("incorrectLap")]
    public WorkArea IncorrectLap
    {
      get => incorrectLap ??= new();
      set => incorrectLap = value;
    }

    /// <summary>
    /// A value of ObId.
    /// </summary>
    [JsonPropertyName("obId")]
    public TextWorkArea ObId
    {
      get => obId ??= new();
      set => obId = value;
    }

    /// <summary>
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    /// <summary>
    /// A value of MaxDiscontinue.
    /// </summary>
    [JsonPropertyName("maxDiscontinue")]
    public DateWorkArea MaxDiscontinue
    {
      get => maxDiscontinue ??= new();
      set => maxDiscontinue = value;
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
    /// A value of ProcessUpdatesInd.
    /// </summary>
    [JsonPropertyName("processUpdatesInd")]
    public Common ProcessUpdatesInd
    {
      get => processUpdatesInd ??= new();
      set => processUpdatesInd = value;
    }

    /// <summary>
    /// A value of ReadCnt.
    /// </summary>
    [JsonPropertyName("readCnt")]
    public Common ReadCnt
    {
      get => readCnt ??= new();
      set => readCnt = value;
    }

    /// <summary>
    /// A value of UpdateCnt.
    /// </summary>
    [JsonPropertyName("updateCnt")]
    public Common UpdateCnt
    {
      get => updateCnt ??= new();
      set => updateCnt = value;
    }

    /// <summary>
    /// A value of ErrorCnt.
    /// </summary>
    [JsonPropertyName("errorCnt")]
    public Common ErrorCnt
    {
      get => errorCnt ??= new();
      set => errorCnt = value;
    }

    /// <summary>
    /// A value of UserId.
    /// </summary>
    [JsonPropertyName("userId")]
    public TextWorkArea UserId
    {
      get => userId ??= new();
      set => userId = value;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
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
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
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

    private Common tmp;
    private WorkArea correctLap;
    private WorkArea incorrectLap;
    private TextWorkArea obId;
    private WorkArea workArea;
    private DateWorkArea maxDiscontinue;
    private DateWorkArea null1;
    private Common processUpdatesInd;
    private Common readCnt;
    private Common updateCnt;
    private Common errorCnt;
    private TextWorkArea userId;
    private DateWorkArea current;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private External external;
    private ProgramProcessingInfo programProcessingInfo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingObligor1.
    /// </summary>
    [JsonPropertyName("existingObligor1")]
    public CsePerson ExistingObligor1
    {
      get => existingObligor1 ??= new();
      set => existingObligor1 = value;
    }

    /// <summary>
    /// A value of ExistingObligor2.
    /// </summary>
    [JsonPropertyName("existingObligor2")]
    public CsePersonAccount ExistingObligor2
    {
      get => existingObligor2 ??= new();
      set => existingObligor2 = value;
    }

    /// <summary>
    /// A value of ExistingSupported1.
    /// </summary>
    [JsonPropertyName("existingSupported1")]
    public CsePerson ExistingSupported1
    {
      get => existingSupported1 ??= new();
      set => existingSupported1 = value;
    }

    /// <summary>
    /// A value of ExistingSupported2.
    /// </summary>
    [JsonPropertyName("existingSupported2")]
    public CsePersonAccount ExistingSupported2
    {
      get => existingSupported2 ??= new();
      set => existingSupported2 = value;
    }

    /// <summary>
    /// A value of ExistingObligationType.
    /// </summary>
    [JsonPropertyName("existingObligationType")]
    public ObligationType ExistingObligationType
    {
      get => existingObligationType ??= new();
      set => existingObligationType = value;
    }

    /// <summary>
    /// A value of ExistingObligation.
    /// </summary>
    [JsonPropertyName("existingObligation")]
    public Obligation ExistingObligation
    {
      get => existingObligation ??= new();
      set => existingObligation = value;
    }

    /// <summary>
    /// A value of ExistingDebt.
    /// </summary>
    [JsonPropertyName("existingDebt")]
    public ObligationTransaction ExistingDebt
    {
      get => existingDebt ??= new();
      set => existingDebt = value;
    }

    /// <summary>
    /// A value of ExistingLegalAction.
    /// </summary>
    [JsonPropertyName("existingLegalAction")]
    public LegalAction ExistingLegalAction
    {
      get => existingLegalAction ??= new();
      set => existingLegalAction = value;
    }

    /// <summary>
    /// A value of Check.
    /// </summary>
    [JsonPropertyName("check")]
    public LegalActionPerson Check
    {
      get => check ??= new();
      set => check = value;
    }

    /// <summary>
    /// A value of Incorrect.
    /// </summary>
    [JsonPropertyName("incorrect")]
    public LegalActionPerson Incorrect
    {
      get => incorrect ??= new();
      set => incorrect = value;
    }

    /// <summary>
    /// A value of Correct.
    /// </summary>
    [JsonPropertyName("correct")]
    public LegalActionPerson Correct
    {
      get => correct ??= new();
      set => correct = value;
    }

    private CsePerson existingObligor1;
    private CsePersonAccount existingObligor2;
    private CsePerson existingSupported1;
    private CsePersonAccount existingSupported2;
    private ObligationType existingObligationType;
    private Obligation existingObligation;
    private ObligationTransaction existingDebt;
    private LegalAction existingLegalAction;
    private LegalActionPerson check;
    private LegalActionPerson incorrect;
    private LegalActionPerson correct;
  }
#endregion
}
