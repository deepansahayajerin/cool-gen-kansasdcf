// Program: FN_UPDATE_RECAPTURE_RULE_OBLIGOR, ID: 372128808, model: 746.
// Short name: SWE00674
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_UPDATE_RECAPTURE_RULE_OBLIGOR.
/// </summary>
[Serializable]
public partial class FnUpdateRecaptureRuleObligor: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_UPDATE_RECAPTURE_RULE_OBLIGOR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnUpdateRecaptureRuleObligor(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnUpdateRecaptureRuleObligor.
  /// </summary>
  public FnUpdateRecaptureRuleObligor(IContext context, Import import,
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
    local.MaxDate.Date = UseCabSetMaximumDiscontinueDate();
    MoveRecaptureRule(import.ObligorRule, local.ObligorRule);

    if (Equal(local.ObligorRule.DiscontinueDate, local.InitialisedToZeros.Date))
    {
      local.ObligorRule.DiscontinueDate = local.MaxDate.Date;
    }

    // **Update expiry date of an existing rule if it conflicts with the start 
    // date of the rule being added.
    if (ReadObligorRule1())
    {
      if (!Lt(import.ObligorRule.EffectiveDate,
        entities.ObligorRule.EffectiveDate))
      {
        try
        {
          UpdateObligorRule1();

          // *** Only doing this for the rule currently in effect.
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_RECAPTURE_RULE_NU";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_RECAPTURE_RULE_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }
    else
    {
      // *** OK ***
    }

    if (ReadObligorRule2())
    {
      try
      {
        UpdateObligorRule2();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_OBLIGOR_RULE_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_OBLIGOR_RULE_PV";

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
      ExitState = "FN0000_OBLIGOR_RULE_NF";
    }
  }

  private static void MoveRecaptureRule(RecaptureRule source,
    RecaptureRule target)
  {
    target.NegotiatedDate = source.NegotiatedDate;
    target.NonAdcArrearsMaxAmount = source.NonAdcArrearsMaxAmount;
    target.NonAdcArrearsAmount = source.NonAdcArrearsAmount;
    target.NonAdcArrearsPercentage = source.NonAdcArrearsPercentage;
    target.NonAdcCurrentMaxAmount = source.NonAdcCurrentMaxAmount;
    target.NonAdcCurrentAmount = source.NonAdcCurrentAmount;
    target.NonAdcCurrentPercentage = source.NonAdcCurrentPercentage;
    target.Type1 = source.Type1;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.PassthruPercentage = source.PassthruPercentage;
    target.PassthruAmount = source.PassthruAmount;
    target.PassthruMaxAmount = source.PassthruMaxAmount;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private bool ReadObligorRule1()
  {
    entities.ObligorRule.Populated = false;

    return Read("ReadObligorRule1",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetNullableDate(command, "discontinueDate1", date);
        db.SetNullableDate(
          command, "discontinueDate2", local.MaxDate.Date.GetValueOrDefault());
        db.SetNullableString(command, "cspDNumber", import.CsePerson.Number);
        db.SetInt32(
          command, "recaptureRuleId",
          import.ObligorRule.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ObligorRule.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.ObligorRule.CpaDType = db.GetNullableString(reader, 1);
        entities.ObligorRule.CspDNumber = db.GetNullableString(reader, 2);
        entities.ObligorRule.EffectiveDate = db.GetDate(reader, 3);
        entities.ObligorRule.NegotiatedDate = db.GetNullableDate(reader, 4);
        entities.ObligorRule.DiscontinueDate = db.GetNullableDate(reader, 5);
        entities.ObligorRule.NonAdcArrearsMaxAmount =
          db.GetNullableDecimal(reader, 6);
        entities.ObligorRule.NonAdcArrearsAmount =
          db.GetNullableDecimal(reader, 7);
        entities.ObligorRule.NonAdcArrearsPercentage =
          db.GetNullableInt32(reader, 8);
        entities.ObligorRule.NonAdcCurrentMaxAmount =
          db.GetNullableDecimal(reader, 9);
        entities.ObligorRule.NonAdcCurrentAmount =
          db.GetNullableDecimal(reader, 10);
        entities.ObligorRule.NonAdcCurrentPercentage =
          db.GetNullableInt32(reader, 11);
        entities.ObligorRule.PassthruPercentage =
          db.GetNullableInt32(reader, 12);
        entities.ObligorRule.PassthruAmount = db.GetNullableDecimal(reader, 13);
        entities.ObligorRule.PassthruMaxAmount =
          db.GetNullableDecimal(reader, 14);
        entities.ObligorRule.CreatedBy = db.GetString(reader, 15);
        entities.ObligorRule.CreatedTimestamp = db.GetDateTime(reader, 16);
        entities.ObligorRule.LastUpdatedBy = db.GetNullableString(reader, 17);
        entities.ObligorRule.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 18);
        entities.ObligorRule.ObligorRuleFiller =
          db.GetNullableString(reader, 19);
        entities.ObligorRule.Type1 = db.GetString(reader, 20);
        entities.ObligorRule.RepaymentLetterPrintDate =
          db.GetNullableDate(reader, 21);
        entities.ObligorRule.Populated = true;
        CheckValid<RecaptureRule>("CpaDType", entities.ObligorRule.CpaDType);
        CheckValid<RecaptureRule>("Type1", entities.ObligorRule.Type1);
      });
  }

  private bool ReadObligorRule2()
  {
    entities.ObligorRule.Populated = false;

    return Read("ReadObligorRule2",
      (db, command) =>
      {
        db.SetInt32(
          command, "recaptureRuleId",
          import.ObligorRule.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ObligorRule.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.ObligorRule.CpaDType = db.GetNullableString(reader, 1);
        entities.ObligorRule.CspDNumber = db.GetNullableString(reader, 2);
        entities.ObligorRule.EffectiveDate = db.GetDate(reader, 3);
        entities.ObligorRule.NegotiatedDate = db.GetNullableDate(reader, 4);
        entities.ObligorRule.DiscontinueDate = db.GetNullableDate(reader, 5);
        entities.ObligorRule.NonAdcArrearsMaxAmount =
          db.GetNullableDecimal(reader, 6);
        entities.ObligorRule.NonAdcArrearsAmount =
          db.GetNullableDecimal(reader, 7);
        entities.ObligorRule.NonAdcArrearsPercentage =
          db.GetNullableInt32(reader, 8);
        entities.ObligorRule.NonAdcCurrentMaxAmount =
          db.GetNullableDecimal(reader, 9);
        entities.ObligorRule.NonAdcCurrentAmount =
          db.GetNullableDecimal(reader, 10);
        entities.ObligorRule.NonAdcCurrentPercentage =
          db.GetNullableInt32(reader, 11);
        entities.ObligorRule.PassthruPercentage =
          db.GetNullableInt32(reader, 12);
        entities.ObligorRule.PassthruAmount = db.GetNullableDecimal(reader, 13);
        entities.ObligorRule.PassthruMaxAmount =
          db.GetNullableDecimal(reader, 14);
        entities.ObligorRule.CreatedBy = db.GetString(reader, 15);
        entities.ObligorRule.CreatedTimestamp = db.GetDateTime(reader, 16);
        entities.ObligorRule.LastUpdatedBy = db.GetNullableString(reader, 17);
        entities.ObligorRule.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 18);
        entities.ObligorRule.ObligorRuleFiller =
          db.GetNullableString(reader, 19);
        entities.ObligorRule.Type1 = db.GetString(reader, 20);
        entities.ObligorRule.RepaymentLetterPrintDate =
          db.GetNullableDate(reader, 21);
        entities.ObligorRule.Populated = true;
        CheckValid<RecaptureRule>("CpaDType", entities.ObligorRule.CpaDType);
        CheckValid<RecaptureRule>("Type1", entities.ObligorRule.Type1);
      });
  }

  private void UpdateObligorRule1()
  {
    var discontinueDate = import.ObligorRule.EffectiveDate;

    entities.ObligorRule.Populated = false;
    Update("UpdateObligorRule1",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetInt32(
          command, "recaptureRuleId",
          entities.ObligorRule.SystemGeneratedIdentifier);
      });

    entities.ObligorRule.DiscontinueDate = discontinueDate;
    entities.ObligorRule.Populated = true;
  }

  private void UpdateObligorRule2()
  {
    var effectiveDate = local.ObligorRule.EffectiveDate;
    var negotiatedDate = local.ObligorRule.NegotiatedDate;
    var discontinueDate = local.ObligorRule.DiscontinueDate;
    var nonAdcArrearsMaxAmount =
      local.ObligorRule.NonAdcArrearsMaxAmount.GetValueOrDefault();
    var nonAdcArrearsAmount =
      local.ObligorRule.NonAdcArrearsAmount.GetValueOrDefault();
    var nonAdcArrearsPercentage =
      local.ObligorRule.NonAdcArrearsPercentage.GetValueOrDefault();
    var nonAdcCurrentMaxAmount =
      local.ObligorRule.NonAdcCurrentMaxAmount.GetValueOrDefault();
    var nonAdcCurrentAmount =
      local.ObligorRule.NonAdcCurrentAmount.GetValueOrDefault();
    var nonAdcCurrentPercentage =
      local.ObligorRule.NonAdcCurrentPercentage.GetValueOrDefault();
    var passthruPercentage =
      local.ObligorRule.PassthruPercentage.GetValueOrDefault();
    var passthruAmount = local.ObligorRule.PassthruAmount.GetValueOrDefault();
    var passthruMaxAmount =
      local.ObligorRule.PassthruMaxAmount.GetValueOrDefault();
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var repaymentLetterPrintDate = new DateTime(1, 1, 1);

    entities.ObligorRule.Populated = false;
    Update("UpdateObligorRule2",
      (db, command) =>
      {
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "negotiatedDate", negotiatedDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.
          SetNullableDecimal(command, "naArrearsMaxAmt", nonAdcArrearsMaxAmount);
          
        db.SetNullableDecimal(command, "naArrearsAmount", nonAdcArrearsAmount);
        db.SetNullableInt32(command, "naArrearsPct", nonAdcArrearsPercentage);
        db.
          SetNullableDecimal(command, "naCurrMaxAmount", nonAdcCurrentMaxAmount);
          
        db.SetNullableDecimal(command, "naCurrAmount", nonAdcCurrentAmount);
        db.
          SetNullableInt32(command, "naCurrPercentage", nonAdcCurrentPercentage);
          
        db.SetNullableInt32(command, "passthruPercentag", passthruPercentage);
        db.SetNullableDecimal(command, "passthruAmount", passthruAmount);
        db.SetNullableDecimal(command, "passthruMaxAmt", passthruMaxAmount);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.
          SetNullableDate(command, "repymntLtrPrtDt", repaymentLetterPrintDate);
          
        db.SetInt32(
          command, "recaptureRuleId",
          entities.ObligorRule.SystemGeneratedIdentifier);
      });

    entities.ObligorRule.EffectiveDate = effectiveDate;
    entities.ObligorRule.NegotiatedDate = negotiatedDate;
    entities.ObligorRule.DiscontinueDate = discontinueDate;
    entities.ObligorRule.NonAdcArrearsMaxAmount = nonAdcArrearsMaxAmount;
    entities.ObligorRule.NonAdcArrearsAmount = nonAdcArrearsAmount;
    entities.ObligorRule.NonAdcArrearsPercentage = nonAdcArrearsPercentage;
    entities.ObligorRule.NonAdcCurrentMaxAmount = nonAdcCurrentMaxAmount;
    entities.ObligorRule.NonAdcCurrentAmount = nonAdcCurrentAmount;
    entities.ObligorRule.NonAdcCurrentPercentage = nonAdcCurrentPercentage;
    entities.ObligorRule.PassthruPercentage = passthruPercentage;
    entities.ObligorRule.PassthruAmount = passthruAmount;
    entities.ObligorRule.PassthruMaxAmount = passthruMaxAmount;
    entities.ObligorRule.LastUpdatedBy = lastUpdatedBy;
    entities.ObligorRule.LastUpdatedTmst = lastUpdatedTmst;
    entities.ObligorRule.RepaymentLetterPrintDate = repaymentLetterPrintDate;
    entities.ObligorRule.Populated = true;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of ObligorRule.
    /// </summary>
    [JsonPropertyName("obligorRule")]
    public RecaptureRule ObligorRule
    {
      get => obligorRule ??= new();
      set => obligorRule = value;
    }

    private CsePerson csePerson;
    private RecaptureRule obligorRule;
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
    /// A value of ObligorRule.
    /// </summary>
    [JsonPropertyName("obligorRule")]
    public RecaptureRule ObligorRule
    {
      get => obligorRule ??= new();
      set => obligorRule = value;
    }

    /// <summary>
    /// A value of DateChanged.
    /// </summary>
    [JsonPropertyName("dateChanged")]
    public Common DateChanged
    {
      get => dateChanged ??= new();
      set => dateChanged = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    /// <summary>
    /// A value of InitialisedToZeros.
    /// </summary>
    [JsonPropertyName("initialisedToZeros")]
    public DateWorkArea InitialisedToZeros
    {
      get => initialisedToZeros ??= new();
      set => initialisedToZeros = value;
    }

    private RecaptureRule obligorRule;
    private Common dateChanged;
    private DateWorkArea maxDate;
    private DateWorkArea initialisedToZeros;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of ObligorRule.
    /// </summary>
    [JsonPropertyName("obligorRule")]
    public RecaptureRule ObligorRule
    {
      get => obligorRule ??= new();
      set => obligorRule = value;
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

    private CsePersonAccount obligor;
    private RecaptureRule obligorRule;
    private CsePerson csePerson;
  }
#endregion
}
