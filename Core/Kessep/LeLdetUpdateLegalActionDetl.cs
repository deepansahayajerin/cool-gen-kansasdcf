// Program: LE_LDET_UPDATE_LEGAL_ACTION_DETL, ID: 371993424, model: 746.
// Short name: SWE00793
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_LDET_UPDATE_LEGAL_ACTION_DETL.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This process creates an occurrence of Legal Action Detail related to a 
/// specific Legal Action.
/// </para>
/// </summary>
[Serializable]
public partial class LeLdetUpdateLegalActionDetl: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LDET_UPDATE_LEGAL_ACTION_DETL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeLdetUpdateLegalActionDetl(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeLdetUpdateLegalActionDetl.
  /// </summary>
  public LeLdetUpdateLegalActionDetl(IContext context, Import import,
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
    // --------------------------------------------------------------------------------------------------
    //                                 
    // C H A N G E    L O G
    // --------------------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // --------  ---------	----------	
    // ----------------------------------------------------------
    // 02/21/96  Govind    			Initial Code
    // 10/07/97  R Grey	H29871		When update effective date on LOPS/LDET
    // 02/17/99  PSharp 			Changed the update of last updated by from spaces to
    // 					user-id
    // 03/09/00  DJean		PR89604		Add edit that stops identical legal action 
    // detail rows to be added.
    // 03/15/00  DJean		PR88003		Add read to warn if associated legal action 
    // persons are found.
    // 04/17/02  KCole		PR142770	Moved check for duplicate details to PStep
    // 04/11/03  GVandy	PR175292	Blank out non financial obigation type when 
    // changing from a
    // 					non financial to a financial legal detail.
    // --------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.UpdateLeActPer.Flag = "N";

    // ****************************************************************
    // * 3/9/00	D. Jean	PR89604- Add edit that stops identical
    // * legal action detail rows to be added.
    // ****************************************************************
    if (ReadLegalActionDetail())
    {
      local.LegalActionDetail.Assign(entities.LegalActionDetail);

      if (Equal(entities.LegalActionDetail.EffectiveDate,
        import.LegalActionDetail.EffectiveDate))
      {
      }
      else
      {
        local.UpdateLeActPer.Flag = "Y";
      }

      if (Equal(entities.LegalActionDetail.EndDate,
        import.LegalActionDetail.EndDate))
      {
      }
      else
      {
        local.UpdateLeActPer.Flag = "Y";
      }
    }
    else if (ReadLegalAction())
    {
      ExitState = "LEGAL_ACTION_DETAIL_NF";

      return;
    }
    else
    {
      ExitState = "LEGAL_ACTION_NF";

      return;
    }

    // ---------------------------------------------
    // If the previous legal detail was a Financial one and the new detail is 
    // non financial - Dont allow - ask the user to delete it first since there
    // could be some obligations tied to it.
    // ---------------------------------------------
    if (AsChar(entities.LegalActionDetail.DetailType) == 'F')
    {
      if (AsChar(import.LegalActionDetail.DetailType) == 'N')
      {
        ExitState = "LE0000_CANT_CHANGE_FIN_TO_NONFIN";

        return;
      }
    }

    if (!Lt(local.InitialisedToZeros.EndDate, import.LegalActionDetail.EndDate))
    {
      local.LegalActionDetail.EndDate = UseCabSetMaximumDiscontinueDate();
    }
    else
    {
      local.LegalActionDetail.EndDate = import.LegalActionDetail.EndDate;
    }

    if (AsChar(import.LegalActionDetail.DetailType) == 'F')
    {
      if (!ReadObligationType())
      {
        ExitState = "FN0000_OBLIGATION_TYPE_NF";

        return;
      }

      try
      {
        UpdateLegalActionDetail1();

        // ****************************************************************
        // 3/15/00	D. Jean	PR88003- Add read to warn if associated legal action 
        // persons are found.
        // ****************************************************************
        if (local.LegalActionDetail.ArrearsAmount.GetValueOrDefault() != import
          .LegalActionDetail.ArrearsAmount.GetValueOrDefault() || local
          .LegalActionDetail.BondAmount.GetValueOrDefault() != import
          .LegalActionDetail.BondAmount.GetValueOrDefault() || local
          .LegalActionDetail.CurrentAmount.GetValueOrDefault() != import
          .LegalActionDetail.CurrentAmount.GetValueOrDefault() || local
          .LegalActionDetail.JudgementAmount.GetValueOrDefault() != import
          .LegalActionDetail.JudgementAmount.GetValueOrDefault())
        {
          if (ReadLegalActionPerson())
          {
            ExitState = "LE0000_UPDT_OK_AMT_CHGD_CHK_LOPS";
          }
        }

        if (AsChar(local.UpdateLeActPer.Flag) == 'Y')
        {
          UseLeLdetUpdtLeActPersDates();
        }
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "LEGAL_ACTION_DETAIL_NU";

            break;
          case ErrorCode.PermittedValueViolation:
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
      try
      {
        UpdateLegalActionDetail2();

        // ****************************************************************
        // 3/15/00	D. Jean	PR88003- Add read to warn if associated legal action 
        // persons are found.
        // ****************************************************************
        if (local.LegalActionDetail.ArrearsAmount.GetValueOrDefault() != import
          .LegalActionDetail.ArrearsAmount.GetValueOrDefault() || local
          .LegalActionDetail.BondAmount.GetValueOrDefault() != import
          .LegalActionDetail.BondAmount.GetValueOrDefault() || local
          .LegalActionDetail.CurrentAmount.GetValueOrDefault() != import
          .LegalActionDetail.CurrentAmount.GetValueOrDefault() || local
          .LegalActionDetail.JudgementAmount.GetValueOrDefault() != import
          .LegalActionDetail.JudgementAmount.GetValueOrDefault())
        {
          if (ReadLegalActionPerson())
          {
            ExitState = "LE0000_UPDT_OK_AMT_CHGD_CHK_LOPS";
          }
        }

        if (AsChar(local.UpdateLeActPer.Flag) == 'Y')
        {
          UseLeLdetUpdtLeActPersDates();
        }
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "LEGAL_ACTION_DETAIL_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
  }

  private static void MoveLegalActionDetail(LegalActionDetail source,
    LegalActionDetail target)
  {
    target.Number = source.Number;
    target.DetailType = source.DetailType;
    target.EndDate = source.EndDate;
    target.EffectiveDate = source.EffectiveDate;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseLeLdetUpdtLeActPersDates()
  {
    var useImport = new LeLdetUpdtLeActPersDates.Import();
    var useExport = new LeLdetUpdtLeActPersDates.Export();

    useImport.LegalAction.Identifier = import.LegalAction.Identifier;
    MoveLegalActionDetail(import.LegalActionDetail, useImport.LegalActionDetail);
      

    Call(LeLdetUpdtLeActPersDates.Execute, useImport, useExport);
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionDetail()
  {
    entities.LegalActionDetail.Populated = false;

    return Read("ReadLegalActionDetail",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", import.LegalAction.Identifier);
        db.SetInt32(command, "laDetailNo", import.LegalActionDetail.Number);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail.BondAmount =
          db.GetNullableDecimal(reader, 4);
        entities.LegalActionDetail.CreatedBy = db.GetString(reader, 5);
        entities.LegalActionDetail.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.LegalActionDetail.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.LegalActionDetail.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 8);
        entities.LegalActionDetail.ArrearsAmount =
          db.GetNullableDecimal(reader, 9);
        entities.LegalActionDetail.CurrentAmount =
          db.GetNullableDecimal(reader, 10);
        entities.LegalActionDetail.JudgementAmount =
          db.GetNullableDecimal(reader, 11);
        entities.LegalActionDetail.Limit = db.GetNullableInt32(reader, 12);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 13);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 14);
        entities.LegalActionDetail.FreqPeriodCode =
          db.GetNullableString(reader, 15);
        entities.LegalActionDetail.DayOfWeek = db.GetNullableInt32(reader, 16);
        entities.LegalActionDetail.DayOfMonth1 =
          db.GetNullableInt32(reader, 17);
        entities.LegalActionDetail.DayOfMonth2 =
          db.GetNullableInt32(reader, 18);
        entities.LegalActionDetail.PeriodInd = db.GetNullableString(reader, 19);
        entities.LegalActionDetail.Description = db.GetString(reader, 20);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 21);
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);
      });
  }

  private bool ReadLegalActionPerson()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.LegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 1);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 2);
        entities.LegalActionPerson.Populated = true;
      });
  }

  private bool ReadObligationType()
  {
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetString(command, "debtTypCd", import.ObligationType.Code);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Populated = true;
      });
  }

  private void UpdateLegalActionDetail1()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);

    var endDate = local.LegalActionDetail.EndDate;
    var effectiveDate = import.LegalActionDetail.EffectiveDate;
    var bondAmount = import.LegalActionDetail.BondAmount.GetValueOrDefault();
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();
    var arrearsAmount =
      import.LegalActionDetail.ArrearsAmount.GetValueOrDefault();
    var currentAmount =
      import.LegalActionDetail.CurrentAmount.GetValueOrDefault();
    var judgementAmount =
      import.LegalActionDetail.JudgementAmount.GetValueOrDefault();
    var limit = import.LegalActionDetail.Limit.GetValueOrDefault();
    var detailType = import.LegalActionDetail.DetailType;
    var freqPeriodCode = import.LegalActionDetail.FreqPeriodCode ?? "";
    var dayOfWeek = import.LegalActionDetail.DayOfWeek.GetValueOrDefault();
    var dayOfMonth1 = import.LegalActionDetail.DayOfMonth1.GetValueOrDefault();
    var dayOfMonth2 = import.LegalActionDetail.DayOfMonth2.GetValueOrDefault();
    var periodInd = import.LegalActionDetail.PeriodInd ?? "";
    var description = import.LegalActionDetail.Description;
    var otyId = entities.ObligationType.SystemGeneratedIdentifier;

    CheckValid<LegalActionDetail>("DetailType", detailType);
    entities.LegalActionDetail.Populated = false;
    Update("UpdateLegalActionDetail1",
      (db, command) =>
      {
        db.SetNullableDate(command, "endDt", endDate);
        db.SetDate(command, "effectiveDt", effectiveDate);
        db.SetNullableDecimal(command, "bondAmt", bondAmount);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetNullableDecimal(command, "arrearsAmount", arrearsAmount);
        db.SetNullableDecimal(command, "currentAmount", currentAmount);
        db.SetNullableDecimal(command, "judgementAmount", judgementAmount);
        db.SetNullableInt32(command, "limit", limit);
        db.SetNullableString(command, "nonFinOblgType", "");
        db.SetString(command, "detailType", detailType);
        db.SetNullableString(command, "frqPrdCd", freqPeriodCode);
        db.SetNullableInt32(command, "dayOfWeek", dayOfWeek);
        db.SetNullableInt32(command, "dayOfMonth1", dayOfMonth1);
        db.SetNullableInt32(command, "dayOfMonth2", dayOfMonth2);
        db.SetNullableString(command, "periodInd", periodInd);
        db.SetString(command, "description", description);
        db.SetNullableInt32(command, "otyId", otyId);
        db.SetInt32(
          command, "lgaIdentifier", entities.LegalActionDetail.LgaIdentifier);
        db.SetInt32(command, "laDetailNo", entities.LegalActionDetail.Number);
      });

    entities.LegalActionDetail.EndDate = endDate;
    entities.LegalActionDetail.EffectiveDate = effectiveDate;
    entities.LegalActionDetail.BondAmount = bondAmount;
    entities.LegalActionDetail.LastUpdatedBy = lastUpdatedBy;
    entities.LegalActionDetail.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.LegalActionDetail.ArrearsAmount = arrearsAmount;
    entities.LegalActionDetail.CurrentAmount = currentAmount;
    entities.LegalActionDetail.JudgementAmount = judgementAmount;
    entities.LegalActionDetail.Limit = limit;
    entities.LegalActionDetail.NonFinOblgType = "";
    entities.LegalActionDetail.DetailType = detailType;
    entities.LegalActionDetail.FreqPeriodCode = freqPeriodCode;
    entities.LegalActionDetail.DayOfWeek = dayOfWeek;
    entities.LegalActionDetail.DayOfMonth1 = dayOfMonth1;
    entities.LegalActionDetail.DayOfMonth2 = dayOfMonth2;
    entities.LegalActionDetail.PeriodInd = periodInd;
    entities.LegalActionDetail.Description = description;
    entities.LegalActionDetail.OtyId = otyId;
    entities.LegalActionDetail.Populated = true;
  }

  private void UpdateLegalActionDetail2()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);

    var endDate = local.LegalActionDetail.EndDate;
    var effectiveDate = import.LegalActionDetail.EffectiveDate;
    var bondAmount = import.LegalActionDetail.BondAmount.GetValueOrDefault();
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();
    var nonFinOblgType = Substring(import.ObligationType.Code, 1, 4);
    var detailType = import.LegalActionDetail.DetailType;
    var freqPeriodCode = import.LegalActionDetail.FreqPeriodCode ?? "";
    var dayOfWeek = import.LegalActionDetail.DayOfWeek.GetValueOrDefault();
    var dayOfMonth1 = import.LegalActionDetail.DayOfMonth1.GetValueOrDefault();
    var dayOfMonth2 = import.LegalActionDetail.DayOfMonth2.GetValueOrDefault();
    var periodInd = import.LegalActionDetail.PeriodInd ?? "";
    var description = import.LegalActionDetail.Description;

    CheckValid<LegalActionDetail>("DetailType", detailType);
    entities.LegalActionDetail.Populated = false;
    Update("UpdateLegalActionDetail2",
      (db, command) =>
      {
        db.SetNullableDate(command, "endDt", endDate);
        db.SetDate(command, "effectiveDt", effectiveDate);
        db.SetNullableDecimal(command, "bondAmt", bondAmount);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetNullableString(command, "nonFinOblgType", nonFinOblgType);
        db.SetString(command, "detailType", detailType);
        db.SetNullableString(command, "frqPrdCd", freqPeriodCode);
        db.SetNullableInt32(command, "dayOfWeek", dayOfWeek);
        db.SetNullableInt32(command, "dayOfMonth1", dayOfMonth1);
        db.SetNullableInt32(command, "dayOfMonth2", dayOfMonth2);
        db.SetNullableString(command, "periodInd", periodInd);
        db.SetString(command, "description", description);
        db.SetInt32(
          command, "lgaIdentifier", entities.LegalActionDetail.LgaIdentifier);
        db.SetInt32(command, "laDetailNo", entities.LegalActionDetail.Number);
      });

    entities.LegalActionDetail.EndDate = endDate;
    entities.LegalActionDetail.EffectiveDate = effectiveDate;
    entities.LegalActionDetail.BondAmount = bondAmount;
    entities.LegalActionDetail.LastUpdatedBy = lastUpdatedBy;
    entities.LegalActionDetail.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.LegalActionDetail.NonFinOblgType = nonFinOblgType;
    entities.LegalActionDetail.DetailType = detailType;
    entities.LegalActionDetail.FreqPeriodCode = freqPeriodCode;
    entities.LegalActionDetail.DayOfWeek = dayOfWeek;
    entities.LegalActionDetail.DayOfMonth1 = dayOfMonth1;
    entities.LegalActionDetail.DayOfMonth2 = dayOfMonth2;
    entities.LegalActionDetail.PeriodInd = periodInd;
    entities.LegalActionDetail.Description = description;
    entities.LegalActionDetail.Populated = true;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    private ObligationType obligationType;
    private LegalAction legalAction;
    private LegalActionDetail legalActionDetail;
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
    /// A value of UpdateLeActPer.
    /// </summary>
    [JsonPropertyName("updateLeActPer")]
    public Common UpdateLeActPer
    {
      get => updateLeActPer ??= new();
      set => updateLeActPer = value;
    }

    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of InitialisedToZeros.
    /// </summary>
    [JsonPropertyName("initialisedToZeros")]
    public LegalActionDetail InitialisedToZeros
    {
      get => initialisedToZeros ??= new();
      set => initialisedToZeros = value;
    }

    private Common updateLeActPer;
    private LegalActionDetail legalActionDetail;
    private LegalActionDetail initialisedToZeros;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    private LegalActionPerson legalActionPerson;
    private ObligationType obligationType;
    private LegalAction legalAction;
    private LegalActionDetail legalActionDetail;
  }
#endregion
}
