// Program: LE_CREATE_LEGAL_ACTION_DETAIL, ID: 371993425, model: 746.
// Short name: SWE00741
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_CREATE_LEGAL_ACTION_DETAIL.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This process creates an occurrence of Legal Action Detail related to a 
/// specific Legal Action.
/// </para>
/// </summary>
[Serializable]
public partial class LeCreateLegalActionDetail: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_CREATE_LEGAL_ACTION_DETAIL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeCreateLegalActionDetail(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeCreateLegalActionDetail.
  /// </summary>
  public LeCreateLegalActionDetail(IContext context, Import import,
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
    // ------------------------------------------------------------
    // Date		Developer	Request #	Description
    // 05/30/95	Dave Allen			Initial Code
    // 03/11/98	Rod Grey			Add local current date/tmstmp
    // 03/29/02	K. Cole		PR142770 	Moved  check for duplicate details to P-Step.
    // ----------------------------------------------------------------------
    local.Current.Timestamp = Now();
    local.Current.Date = Now().Date;
    export.LegalActionDetail.Assign(import.LegalActionDetail);

    if (!ReadLegalAction())
    {
      ExitState = "LEGAL_ACTION_NF";

      return;
    }

    local.LegalActionDetail.Number = 1;

    if (ReadLegalActionDetail())
    {
      local.LegalActionDetail.Number = entities.LegalActionDetail.Number + 1;
    }

    if (AsChar(import.LegalActionDetail.DetailType) == 'F')
    {
      if (!ReadObligationType())
      {
        ExitState = "FN0000_OBLIGATION_TYPE_NF";

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
      try
      {
        CreateLegalActionDetail1();
        export.LegalActionDetail.Assign(entities.LegalActionDetail);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "LEGAL_ACTION_DETAIL_AE";

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
        CreateLegalActionDetail2();
        export.LegalActionDetail.Assign(entities.LegalActionDetail);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "LEGAL_ACTION_DETAIL_AE";

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

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void CreateLegalActionDetail1()
  {
    var lgaIdentifier = entities.LegalAction.Identifier;
    var number = local.LegalActionDetail.Number;
    var endDate = local.LegalActionDetail.EndDate;
    var effectiveDate = import.LegalActionDetail.EffectiveDate;
    var bondAmount = import.LegalActionDetail.BondAmount.GetValueOrDefault();
    var createdBy = global.UserId;
    var createdTstamp = local.Current.Timestamp;
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
    Update("CreateLegalActionDetail1",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", lgaIdentifier);
        db.SetInt32(command, "laDetailNo", number);
        db.SetNullableDate(command, "endDt", endDate);
        db.SetDate(command, "effectiveDt", effectiveDate);
        db.SetNullableDecimal(command, "bondAmt", bondAmount);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", createdTstamp);
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
        db.SetNullableDate(command, "kpcDate", default(DateTime));
      });

    entities.LegalActionDetail.LgaIdentifier = lgaIdentifier;
    entities.LegalActionDetail.Number = number;
    entities.LegalActionDetail.EndDate = endDate;
    entities.LegalActionDetail.EffectiveDate = effectiveDate;
    entities.LegalActionDetail.BondAmount = bondAmount;
    entities.LegalActionDetail.CreatedBy = createdBy;
    entities.LegalActionDetail.CreatedTstamp = createdTstamp;
    entities.LegalActionDetail.LastUpdatedBy = createdBy;
    entities.LegalActionDetail.LastUpdatedTstamp = createdTstamp;
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

  private void CreateLegalActionDetail2()
  {
    var lgaIdentifier = entities.LegalAction.Identifier;
    var number = local.LegalActionDetail.Number;
    var endDate = local.LegalActionDetail.EndDate;
    var effectiveDate = import.LegalActionDetail.EffectiveDate;
    var bondAmount = import.LegalActionDetail.BondAmount.GetValueOrDefault();
    var createdBy = global.UserId;
    var createdTstamp = local.Current.Timestamp;
    var arrearsAmount = 0M;
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
    Update("CreateLegalActionDetail2",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", lgaIdentifier);
        db.SetInt32(command, "laDetailNo", number);
        db.SetNullableDate(command, "endDt", endDate);
        db.SetDate(command, "effectiveDt", effectiveDate);
        db.SetNullableDecimal(command, "bondAmt", bondAmount);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", createdTstamp);
        db.SetNullableDecimal(command, "arrearsAmount", arrearsAmount);
        db.SetNullableDecimal(command, "currentAmount", arrearsAmount);
        db.SetNullableDecimal(command, "judgementAmount", arrearsAmount);
        db.SetNullableInt32(command, "limit", 0);
        db.SetNullableString(command, "nonFinOblgType", nonFinOblgType);
        db.SetString(command, "detailType", detailType);
        db.SetNullableString(command, "frqPrdCd", freqPeriodCode);
        db.SetNullableInt32(command, "dayOfWeek", dayOfWeek);
        db.SetNullableInt32(command, "dayOfMonth1", dayOfMonth1);
        db.SetNullableInt32(command, "dayOfMonth2", dayOfMonth2);
        db.SetNullableString(command, "periodInd", periodInd);
        db.SetString(command, "description", description);
        db.SetNullableDate(command, "kpcDate", default(DateTime));
      });

    entities.LegalActionDetail.LgaIdentifier = lgaIdentifier;
    entities.LegalActionDetail.Number = number;
    entities.LegalActionDetail.EndDate = endDate;
    entities.LegalActionDetail.EffectiveDate = effectiveDate;
    entities.LegalActionDetail.BondAmount = bondAmount;
    entities.LegalActionDetail.CreatedBy = createdBy;
    entities.LegalActionDetail.CreatedTstamp = createdTstamp;
    entities.LegalActionDetail.LastUpdatedBy = createdBy;
    entities.LegalActionDetail.LastUpdatedTstamp = createdTstamp;
    entities.LegalActionDetail.ArrearsAmount = arrearsAmount;
    entities.LegalActionDetail.CurrentAmount = arrearsAmount;
    entities.LegalActionDetail.JudgementAmount = arrearsAmount;
    entities.LegalActionDetail.Limit = 0;
    entities.LegalActionDetail.NonFinOblgType = nonFinOblgType;
    entities.LegalActionDetail.DetailType = detailType;
    entities.LegalActionDetail.FreqPeriodCode = freqPeriodCode;
    entities.LegalActionDetail.DayOfWeek = dayOfWeek;
    entities.LegalActionDetail.DayOfMonth1 = dayOfMonth1;
    entities.LegalActionDetail.DayOfMonth2 = dayOfMonth2;
    entities.LegalActionDetail.PeriodInd = periodInd;
    entities.LegalActionDetail.Description = description;
    entities.LegalActionDetail.OtyId = null;
    entities.LegalActionDetail.Populated = true;
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
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
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
    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    private LegalActionDetail legalActionDetail;
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
    /// A value of InitialisedToZeros.
    /// </summary>
    [JsonPropertyName("initialisedToZeros")]
    public LegalActionDetail InitialisedToZeros
    {
      get => initialisedToZeros ??= new();
      set => initialisedToZeros = value;
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

    private DateWorkArea current;
    private LegalActionDetail initialisedToZeros;
    private LegalActionDetail legalActionDetail;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
#endregion
}
