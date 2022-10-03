// Program: FN_CAB_UPDATE_DEBT_DTL_STATUS, ID: 372266805, model: 746.
// Short name: SWE02349
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CAB_UPDATE_DEBT_DTL_STATUS.
/// </summary>
[Serializable]
public partial class FnCabUpdateDebtDtlStatus: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CAB_UPDATE_DEBT_DTL_STATUS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCabUpdateDebtDtlStatus(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCabUpdateDebtDtlStatus.
  /// </summary>
  public FnCabUpdateDebtDtlStatus(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadDebtDetailStatusHistory())
    {
      try
      {
        UpdateDebtDetailStatusHistory();
        local.Common.Count = 0;

        while(local.Common.Count < 99)
        {
          try
          {
            CreateDebtDetailStatusHistory();

            if (IsExitState("FN0220_DEBT_DETL_STAT_HIST_AE"))
            {
              ExitState = "ACO_NN0000_ALL_OK";
            }

            return;
          }
          catch(Exception e1)
          {
            switch(GetErrorCode(e1))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0220_DEBT_DETL_STAT_HIST_AE";
                ++local.Common.Count;

                continue;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0226_DEBT_DETL_STAT_HIST_PV";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0224_DEBT_DETL_STAT_HIST_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0226_DEBT_DETL_STAT_HIST_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      ExitState = "FN0222_DEBT_DETL_STAT_HIST_NF";
    }
  }

  private int UseCabGenerate3DigitRandomNum()
  {
    var useImport = new CabGenerate3DigitRandomNum.Import();
    var useExport = new CabGenerate3DigitRandomNum.Export();

    Call(CabGenerate3DigitRandomNum.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute3DigitRandomNumber;
  }

  private void CreateDebtDetailStatusHistory()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);

    var systemGeneratedIdentifier = UseCabGenerate3DigitRandomNum();
    var effectiveDt = import.DebtDetailStatusHistory.EffectiveDt;
    var discontinueDt = import.Max.Date;
    var createdBy = import.DebtDetailStatusHistory.CreatedBy;
    var createdTmst = import.Current.Timestamp;
    var otrType = import.Persistent.OtrType;
    var otrId = import.Persistent.OtrGeneratedId;
    var cpaType = import.Persistent.CpaType;
    var cspNumber = import.Persistent.CspNumber;
    var obgId = import.Persistent.ObgGeneratedId;
    var code = import.DebtDetailStatusHistory.Code;
    var otyType = import.Persistent.OtyType;
    var reasonTxt = import.DebtDetailStatusHistory.ReasonTxt ?? "";

    CheckValid<DebtDetailStatusHistory>("OtrType", otrType);
    CheckValid<DebtDetailStatusHistory>("CpaType", cpaType);
    entities.DebtDetailStatusHistory.Populated = false;
    Update("CreateDebtDetailStatusHistory",
      (db, command) =>
      {
        db.SetInt32(command, "obTrnStatHstId", systemGeneratedIdentifier);
        db.SetDate(command, "effectiveDt", effectiveDt);
        db.SetNullableDate(command, "discontinueDt", discontinueDt);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetString(command, "otrType", otrType);
        db.SetInt32(command, "otrId", otrId);
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "obgId", obgId);
        db.SetString(command, "obTrnStCd", code);
        db.SetInt32(command, "otyType", otyType);
        db.SetNullableString(command, "rsnTxt", reasonTxt);
      });

    entities.DebtDetailStatusHistory.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DebtDetailStatusHistory.EffectiveDt = effectiveDt;
    entities.DebtDetailStatusHistory.DiscontinueDt = discontinueDt;
    entities.DebtDetailStatusHistory.CreatedBy = createdBy;
    entities.DebtDetailStatusHistory.CreatedTmst = createdTmst;
    entities.DebtDetailStatusHistory.OtrType = otrType;
    entities.DebtDetailStatusHistory.OtrId = otrId;
    entities.DebtDetailStatusHistory.CpaType = cpaType;
    entities.DebtDetailStatusHistory.CspNumber = cspNumber;
    entities.DebtDetailStatusHistory.ObgId = obgId;
    entities.DebtDetailStatusHistory.Code = code;
    entities.DebtDetailStatusHistory.OtyType = otyType;
    entities.DebtDetailStatusHistory.ReasonTxt = reasonTxt;
    entities.DebtDetailStatusHistory.Populated = true;
  }

  private bool ReadDebtDetailStatusHistory()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);
    entities.DebtDetailStatusHistory.Populated = false;

    return Read("ReadDebtDetailStatusHistory",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDt", import.Max.Date.GetValueOrDefault());
        db.SetInt32(command, "otyType", import.Persistent.OtyType);
        db.SetInt32(command, "obgId", import.Persistent.ObgGeneratedId);
        db.SetString(command, "cspNumber", import.Persistent.CspNumber);
        db.SetString(command, "cpaType", import.Persistent.CpaType);
        db.SetInt32(command, "otrId", import.Persistent.OtrGeneratedId);
        db.SetString(command, "otrType", import.Persistent.OtrType);
      },
      (db, reader) =>
      {
        entities.DebtDetailStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DebtDetailStatusHistory.EffectiveDt = db.GetDate(reader, 1);
        entities.DebtDetailStatusHistory.DiscontinueDt =
          db.GetNullableDate(reader, 2);
        entities.DebtDetailStatusHistory.CreatedBy = db.GetString(reader, 3);
        entities.DebtDetailStatusHistory.CreatedTmst =
          db.GetDateTime(reader, 4);
        entities.DebtDetailStatusHistory.OtrType = db.GetString(reader, 5);
        entities.DebtDetailStatusHistory.OtrId = db.GetInt32(reader, 6);
        entities.DebtDetailStatusHistory.CpaType = db.GetString(reader, 7);
        entities.DebtDetailStatusHistory.CspNumber = db.GetString(reader, 8);
        entities.DebtDetailStatusHistory.ObgId = db.GetInt32(reader, 9);
        entities.DebtDetailStatusHistory.Code = db.GetString(reader, 10);
        entities.DebtDetailStatusHistory.OtyType = db.GetInt32(reader, 11);
        entities.DebtDetailStatusHistory.ReasonTxt =
          db.GetNullableString(reader, 12);
        entities.DebtDetailStatusHistory.Populated = true;
        CheckValid<DebtDetailStatusHistory>("OtrType",
          entities.DebtDetailStatusHistory.OtrType);
        CheckValid<DebtDetailStatusHistory>("CpaType",
          entities.DebtDetailStatusHistory.CpaType);
      });
  }

  private void UpdateDebtDetailStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.DebtDetailStatusHistory.Populated);

    var discontinueDt = import.DebtDetailStatusHistory.EffectiveDt;

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
    /// A value of Persistent.
    /// </summary>
    [JsonPropertyName("persistent")]
    public DebtDetail Persistent
    {
      get => persistent ??= new();
      set => persistent = value;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
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

    private DebtDetail persistent;
    private DebtDetailStatusHistory debtDetailStatusHistory;
    private DateWorkArea max;
    private DateWorkArea current;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private Common common;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DebtDetailStatusHistory.
    /// </summary>
    [JsonPropertyName("debtDetailStatusHistory")]
    public DebtDetailStatusHistory DebtDetailStatusHistory
    {
      get => debtDetailStatusHistory ??= new();
      set => debtDetailStatusHistory = value;
    }

    private DebtDetailStatusHistory debtDetailStatusHistory;
  }
#endregion
}
