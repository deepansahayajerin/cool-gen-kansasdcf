// Program: FN_BFX2_DUE_DATE_FIX, ID: 372882807, model: 746.
// Short name: SWEFFX2B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_BFX2_DUE_DATE_FIX.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnBfx2DueDateFix: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_BFX2_DUE_DATE_FIX program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnBfx2DueDateFix(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnBfx2DueDateFix.
  /// </summary>
  public FnBfx2DueDateFix(IContext context, Import import, Export export):
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
    local.UserId.Text8 = global.UserId;
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

    local.PerformUpdatesInd.Flag =
      Substring(local.ProgramProcessingInfo.ParameterList, 1, 1);

    if (IsEmpty(local.PerformUpdatesInd.Flag))
    {
      local.PerformUpdatesInd.Flag = "N";
    }

    local.ConversionDate.Date =
      StringToDate(Substring(local.ProgramProcessingInfo.ParameterList, 3, 10));
      

    if (Equal(local.ConversionDate.Date, local.Null1.Date))
    {
      ExitState = "FN0000_SYSIN_PARM_ERROR_A";

      return;
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

    local.EabReportSend.RptDetail = "Update Debt Detail : " + local
      .PerformUpdatesInd.Flag;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabReportSend.RptDetail = "Conversion Date : " + NumberToString
      (DateToInt(local.ConversionDate.Date), 15);
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

    foreach(var item in ReadCsePersonObligationTypeDebtCsePersonObligation())
    {
      if (entities.ExistingObligationType.SystemGeneratedIdentifier == 16)
      {
        // : Skip Voluntary Obligations!!!
        continue;
      }

      ++local.ReadCnt.Count;

      if (ReadDebtDetail())
      {
        if (!Equal(entities.ExistingDebtDetail.DueDt, local.ConversionDate.Date))
          
        {
          continue;
        }
      }
      else
      {
        // : Debt Detail Not Found - This is an ERROR!!!
        ++local.ErrorCnt.Count;
        local.EabReportSend.RptDetail =
          "ERROR : Debt Detail Not Found for Debt - Obligor : " + entities
          .ExistingObligor1.Number + " , Supported Person : " + entities
          .ExistingSupported1.Number;
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        continue;
      }

      local.CaseRoleFoundInd.Flag = "N";

      if (entities.ExistingObligationType.SystemGeneratedIdentifier == 2 || entities
        .ExistingObligationType.SystemGeneratedIdentifier == 17)
      {
        if (ReadApplicantRecipient())
        {
          local.CaseRoleFoundInd.Flag = "Y";
        }

        if (AsChar(local.CaseRoleFoundInd.Flag) == 'N')
        {
          // : No Case Role's found - This is an ERROR!!!
          ++local.ErrorCnt.Count;
          local.EabReportSend.RptDetail =
            "ERROR : No Active Case Role Found - Obligor : " + entities
            .ExistingObligor1.Number + " , Supported Person : " + entities
            .ExistingSupported1.Number;
          UseCabErrorReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          continue;
        }

        if (!Lt(entities.ExistingApplicantRecipient.EndDate,
          entities.ExistingDebtDetail.DueDt))
        {
          // : Active Case Role found for Debt Detail Due Date - Proceed to the 
          // next Debt Detail.
          continue;
        }

        if (AsChar(local.PerformUpdatesInd.Flag) == 'Y')
        {
          try
          {
            UpdateDebtDetail2();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0214_DEBT_DETAIL_NU_RB";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0218_DEBT_DETAIL_PV_RB";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }

        ++local.UpdateCnt.Count;

        // : Due Date has been changed to Case Role End Date.
        local.EabReportSend.RptDetail =
          "UPDATE : Debt Detail Due Date --- Obligor : " + entities
          .ExistingObligor1.Number + " , Supp Prsn : " + entities
          .ExistingSupported1.Number + " , New Due Dt: " + NumberToString
          (DateToInt(entities.ExistingApplicantRecipient.EndDate), 15);
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
      else
      {
        if (ReadChild())
        {
          local.CaseRoleFoundInd.Flag = "Y";
        }

        if (AsChar(local.CaseRoleFoundInd.Flag) == 'N')
        {
          // : No Case Role's found - This is an ERROR!!!
          ++local.ErrorCnt.Count;
          local.EabReportSend.RptDetail =
            "ERROR : No Active Case Role Found - Obligor : " + entities
            .ExistingObligor1.Number + " , Supported Person : " + entities
            .ExistingSupported1.Number;
          UseCabErrorReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          continue;
        }

        if (!Lt(entities.ExistingChild.EndDate,
          entities.ExistingDebtDetail.DueDt))
        {
          // : Active Case Role found for Debt Detail Due Date - Proceed to the 
          // next Debt Detail.
          continue;
        }

        if (AsChar(local.PerformUpdatesInd.Flag) == 'Y')
        {
          try
          {
            UpdateDebtDetail1();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0214_DEBT_DETAIL_NU_RB";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0218_DEBT_DETAIL_PV_RB";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }

        ++local.UpdateCnt.Count;

        // : Due Date has been changed to Case Role End Date.
        local.EabReportSend.RptDetail =
          "UPDATE : Debt Detail Due Date --- Obligor : " + entities
          .ExistingObligor1.Number + " , Supp Prsn : " + entities
          .ExistingSupported1.Number + " , New Due Dt: " + NumberToString
          (DateToInt(entities.ExistingChild.EndDate), 15);
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
    }

    local.EabReportSend.RptDetail = "Read Count . . . . . . . . . . . . : " + NumberToString
      (local.ReadCnt.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabReportSend.RptDetail = "Update Count . . . . . . . . . . . : " + NumberToString
      (local.UpdateCnt.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabReportSend.RptDetail = "Error Count. . . . . . . . . . . . : " + NumberToString
      (local.ErrorCnt.Count, 15);
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

  private bool ReadApplicantRecipient()
  {
    entities.ExistingApplicantRecipient.Populated = false;

    return Read("ReadApplicantRecipient",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", entities.ExistingSupported1.Number);
        db.SetString(command, "cspNumber2", entities.ExistingObligor1.Number);
        db.SetNullableDate(
          command, "startDate",
          entities.ExistingDebtDetail.DueDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingApplicantRecipient.CasNumber = db.GetString(reader, 0);
        entities.ExistingApplicantRecipient.CspNumber = db.GetString(reader, 1);
        entities.ExistingApplicantRecipient.Type1 = db.GetString(reader, 2);
        entities.ExistingApplicantRecipient.Identifier = db.GetInt32(reader, 3);
        entities.ExistingApplicantRecipient.StartDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingApplicantRecipient.EndDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingApplicantRecipient.Populated = true;
      });
  }

  private bool ReadChild()
  {
    entities.ExistingChild.Populated = false;

    return Read("ReadChild",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", entities.ExistingSupported1.Number);
        db.SetString(command, "cspNumber2", entities.ExistingObligor1.Number);
        db.SetNullableDate(
          command, "startDate",
          entities.ExistingDebtDetail.DueDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingChild.CasNumber = db.GetString(reader, 0);
        entities.ExistingChild.CspNumber = db.GetString(reader, 1);
        entities.ExistingChild.Type1 = db.GetString(reader, 2);
        entities.ExistingChild.Identifier = db.GetInt32(reader, 3);
        entities.ExistingChild.StartDate = db.GetNullableDate(reader, 4);
        entities.ExistingChild.EndDate = db.GetNullableDate(reader, 5);
        entities.ExistingChild.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCsePersonObligationTypeDebtCsePersonObligation()
  {
    entities.ExistingObligor1.Populated = false;
    entities.ExistingSupported1.Populated = false;
    entities.ExistingObligationType.Populated = false;
    entities.ExistingObligation.Populated = false;
    entities.ExistingDebt.Populated = false;

    return ReadEach("ReadCsePersonObligationTypeDebtCsePersonObligation",
      null,
      (db, reader) =>
      {
        entities.ExistingObligor1.Number = db.GetString(reader, 0);
        entities.ExistingDebt.CspNumber = db.GetString(reader, 0);
        entities.ExistingObligation.CspNumber = db.GetString(reader, 0);
        entities.ExistingObligation.CspNumber = db.GetString(reader, 0);
        entities.ExistingObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingDebt.OtyType = db.GetInt32(reader, 1);
        entities.ExistingObligation.DtyGeneratedId = db.GetInt32(reader, 1);
        entities.ExistingObligation.DtyGeneratedId = db.GetInt32(reader, 1);
        entities.ExistingDebt.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.ExistingObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingDebt.CpaType = db.GetString(reader, 3);
        entities.ExistingObligation.CpaType = db.GetString(reader, 3);
        entities.ExistingDebt.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingDebt.Type1 = db.GetString(reader, 5);
        entities.ExistingDebt.CreatedBy = db.GetString(reader, 6);
        entities.ExistingDebt.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.ExistingDebt.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 8);
        entities.ExistingDebt.DebtType = db.GetString(reader, 9);
        entities.ExistingDebt.CspSupNumber = db.GetNullableString(reader, 10);
        entities.ExistingSupported1.Number = db.GetString(reader, 10);
        entities.ExistingDebt.CpaSupType = db.GetNullableString(reader, 11);
        entities.ExistingObligor1.Populated = true;
        entities.ExistingSupported1.Populated = true;
        entities.ExistingObligationType.Populated = true;
        entities.ExistingObligation.Populated = true;
        entities.ExistingDebt.Populated = true;

        return true;
      });
  }

  private bool ReadDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingDebt.Populated);
    entities.ExistingDebtDetail.Populated = false;

    return Read("ReadDebtDetail",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.ExistingDebt.OtyType);
        db.SetInt32(
          command, "obgGeneratedId", entities.ExistingDebt.ObgGeneratedId);
        db.SetString(command, "otrType", entities.ExistingDebt.Type1);
        db.SetInt32(
          command, "otrGeneratedId",
          entities.ExistingDebt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.ExistingDebt.CpaType);
        db.SetString(command, "cspNumber", entities.ExistingDebt.CspNumber);
      },
      (db, reader) =>
      {
        entities.ExistingDebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingDebtDetail.CspNumber = db.GetString(reader, 1);
        entities.ExistingDebtDetail.CpaType = db.GetString(reader, 2);
        entities.ExistingDebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.ExistingDebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.ExistingDebtDetail.OtrType = db.GetString(reader, 5);
        entities.ExistingDebtDetail.DueDt = db.GetDate(reader, 6);
        entities.ExistingDebtDetail.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 7);
        entities.ExistingDebtDetail.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.ExistingDebtDetail.Populated = true;
      });
  }

  private void UpdateDebtDetail1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingDebtDetail.Populated);

    var dueDt = entities.ExistingChild.EndDate;
    var lastUpdatedTmst = local.Current.Timestamp;
    var lastUpdatedBy = local.UserId.Text8;

    entities.ExistingDebtDetail.Populated = false;
    Update("UpdateDebtDetail1",
      (db, command) =>
      {
        db.SetDate(command, "dueDt", dueDt);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ExistingDebtDetail.ObgGeneratedId);
        db.
          SetString(command, "cspNumber", entities.ExistingDebtDetail.CspNumber);
          
        db.SetString(command, "cpaType", entities.ExistingDebtDetail.CpaType);
        db.SetInt32(
          command, "otrGeneratedId",
          entities.ExistingDebtDetail.OtrGeneratedId);
        db.SetInt32(command, "otyType", entities.ExistingDebtDetail.OtyType);
        db.SetString(command, "otrType", entities.ExistingDebtDetail.OtrType);
      });

    entities.ExistingDebtDetail.DueDt = dueDt;
    entities.ExistingDebtDetail.LastUpdatedTmst = lastUpdatedTmst;
    entities.ExistingDebtDetail.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingDebtDetail.Populated = true;
  }

  private void UpdateDebtDetail2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingDebtDetail.Populated);

    var dueDt = entities.ExistingApplicantRecipient.EndDate;
    var lastUpdatedTmst = local.Current.Timestamp;
    var lastUpdatedBy = local.UserId.Text8;

    entities.ExistingDebtDetail.Populated = false;
    Update("UpdateDebtDetail2",
      (db, command) =>
      {
        db.SetDate(command, "dueDt", dueDt);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ExistingDebtDetail.ObgGeneratedId);
        db.
          SetString(command, "cspNumber", entities.ExistingDebtDetail.CspNumber);
          
        db.SetString(command, "cpaType", entities.ExistingDebtDetail.CpaType);
        db.SetInt32(
          command, "otrGeneratedId",
          entities.ExistingDebtDetail.OtrGeneratedId);
        db.SetInt32(command, "otyType", entities.ExistingDebtDetail.OtyType);
        db.SetString(command, "otrType", entities.ExistingDebtDetail.OtrType);
      });

    entities.ExistingDebtDetail.DueDt = dueDt;
    entities.ExistingDebtDetail.LastUpdatedTmst = lastUpdatedTmst;
    entities.ExistingDebtDetail.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingDebtDetail.Populated = true;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of ConversionDate.
    /// </summary>
    [JsonPropertyName("conversionDate")]
    public DateWorkArea ConversionDate
    {
      get => conversionDate ??= new();
      set => conversionDate = value;
    }

    /// <summary>
    /// A value of PerformUpdatesInd.
    /// </summary>
    [JsonPropertyName("performUpdatesInd")]
    public Common PerformUpdatesInd
    {
      get => performUpdatesInd ??= new();
      set => performUpdatesInd = value;
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
    /// A value of CaseRoleFoundInd.
    /// </summary>
    [JsonPropertyName("caseRoleFoundInd")]
    public Common CaseRoleFoundInd
    {
      get => caseRoleFoundInd ??= new();
      set => caseRoleFoundInd = value;
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

    private DateWorkArea null1;
    private DateWorkArea conversionDate;
    private Common performUpdatesInd;
    private Common readCnt;
    private Common updateCnt;
    private Common errorCnt;
    private Common caseRoleFoundInd;
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
    /// A value of ExistingDebtDetail.
    /// </summary>
    [JsonPropertyName("existingDebtDetail")]
    public DebtDetail ExistingDebtDetail
    {
      get => existingDebtDetail ??= new();
      set => existingDebtDetail = value;
    }

    /// <summary>
    /// A value of ExistingCase.
    /// </summary>
    [JsonPropertyName("existingCase")]
    public Case1 ExistingCase
    {
      get => existingCase ??= new();
      set => existingCase = value;
    }

    /// <summary>
    /// A value of ExistingAbsentParent.
    /// </summary>
    [JsonPropertyName("existingAbsentParent")]
    public CaseRole ExistingAbsentParent
    {
      get => existingAbsentParent ??= new();
      set => existingAbsentParent = value;
    }

    /// <summary>
    /// A value of ExistingChild.
    /// </summary>
    [JsonPropertyName("existingChild")]
    public CaseRole ExistingChild
    {
      get => existingChild ??= new();
      set => existingChild = value;
    }

    /// <summary>
    /// A value of ExistingApplicantRecipient.
    /// </summary>
    [JsonPropertyName("existingApplicantRecipient")]
    public CaseRole ExistingApplicantRecipient
    {
      get => existingApplicantRecipient ??= new();
      set => existingApplicantRecipient = value;
    }

    private CsePerson existingObligor1;
    private CsePersonAccount existingObligor2;
    private CsePerson existingSupported1;
    private CsePersonAccount existingSupported2;
    private ObligationType existingObligationType;
    private Obligation existingObligation;
    private ObligationTransaction existingDebt;
    private DebtDetail existingDebtDetail;
    private Case1 existingCase;
    private CaseRole existingAbsentParent;
    private CaseRole existingChild;
    private CaseRole existingApplicantRecipient;
  }
#endregion
}
