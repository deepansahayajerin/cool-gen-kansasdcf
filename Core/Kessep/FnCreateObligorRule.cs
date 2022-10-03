// Program: FN_CREATE_OBLIGOR_RULE, ID: 372128805, model: 746.
// Short name: SWE02173
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CREATE_OBLIGOR_RULE.
/// </summary>
[Serializable]
public partial class FnCreateObligorRule: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CREATE_OBLIGOR_RULE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCreateObligorRule(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCreateObligorRule.
  /// </summary>
  public FnCreateObligorRule(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.HighDate.Date = UseCabSetMaximumDiscontinueDate();
    local.Current.Date = Now().Date;

    if (!ReadObligor())
    {
      ExitState = "FN0000_OBLIGOR_NF";

      return;
    }

    // **Update expiry date of an existing rule if it conflicts with the start 
    // date of the rule being added.
    if (ReadObligorRule())
    {
      if (!Lt(import.ObligorRule.EffectiveDate,
        entities.ObligorRule.EffectiveDate))
      {
        try
        {
          UpdateObligorRule();

          // *** Only doing this for the rule closest to the one being created.
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

    // *** Create the recapture rule
    local.CreateAttemptCount.Count = 0;

    while(local.CreateAttemptCount.Count < 10)
    {
      try
      {
        CreateObligorRule();
        export.RecaptureRule.SystemGeneratedIdentifier =
          entities.ObligorRule.SystemGeneratedIdentifier;

        return;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_RECAPTURE_RULE_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      local.CreateAttemptCount.Count =
        (int)((long)local.CreateAttemptCount.Count + 1);

      if (local.CreateAttemptCount.Count >= 10)
      {
        ExitState = "FN0000_RECAPTURE_RULE_AE";

        return;
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

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void CreateObligorRule()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);

    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var cpaDType = entities.Obligor.Type1;
    var cspDNumber = entities.Obligor.CspNumber;
    var effectiveDate = import.ObligorRule.EffectiveDate;
    var negotiatedDate = import.ObligorRule.NegotiatedDate;
    var discontinueDate = import.ObligorRule.DiscontinueDate;
    var nonAdcArrearsMaxAmount =
      import.ObligorRule.NonAdcArrearsMaxAmount.GetValueOrDefault();
    var nonAdcArrearsAmount =
      import.ObligorRule.NonAdcArrearsAmount.GetValueOrDefault();
    var nonAdcArrearsPercentage =
      import.ObligorRule.NonAdcArrearsPercentage.GetValueOrDefault();
    var nonAdcCurrentMaxAmount =
      import.ObligorRule.NonAdcCurrentMaxAmount.GetValueOrDefault();
    var nonAdcCurrentAmount =
      import.ObligorRule.NonAdcCurrentAmount.GetValueOrDefault();
    var nonAdcCurrentPercentage =
      import.ObligorRule.NonAdcCurrentPercentage.GetValueOrDefault();
    var passthruPercentage =
      import.ObligorRule.PassthruPercentage.GetValueOrDefault();
    var passthruAmount = import.ObligorRule.PassthruAmount.GetValueOrDefault();
    var passthruMaxAmount =
      import.ObligorRule.PassthruMaxAmount.GetValueOrDefault();
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var type1 = "O";

    CheckValid<RecaptureRule>("CpaDType", cpaDType);
    CheckValid<RecaptureRule>("Type1", type1);
    entities.ObligorRule.Populated = false;
    Update("CreateObligorRule",
      (db, command) =>
      {
        db.SetInt32(command, "recaptureRuleId", systemGeneratedIdentifier);
        db.SetNullableString(command, "cpaDType", cpaDType);
        db.SetNullableString(command, "cspDNumber", cspDNumber);
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
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetNullableString(command, "defaultRuleFille", "");
        db.SetNullableString(command, "obligorRuleFille", "");
        db.SetString(command, "type", type1);
        db.SetNullableDate(command, "repymntLtrPrtDt", null);
      });

    entities.ObligorRule.SystemGeneratedIdentifier = systemGeneratedIdentifier;
    entities.ObligorRule.CpaDType = cpaDType;
    entities.ObligorRule.CspDNumber = cspDNumber;
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
    entities.ObligorRule.CreatedBy = createdBy;
    entities.ObligorRule.CreatedTimestamp = createdTimestamp;
    entities.ObligorRule.LastUpdatedBy = createdBy;
    entities.ObligorRule.LastUpdatedTmst = createdTimestamp;
    entities.ObligorRule.ObligorRuleFiller = "";
    entities.ObligorRule.Type1 = type1;
    entities.ObligorRule.RepaymentLetterPrintDate = null;
    entities.ObligorRule.Populated = true;
  }

  private bool ReadObligor()
  {
    entities.Obligor.Populated = false;

    return Read("ReadObligor",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Obligor.CspNumber = db.GetString(reader, 0);
        entities.Obligor.Type1 = db.GetString(reader, 1);
        entities.Obligor.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Obligor.Type1);
      });
  }

  private bool ReadObligorRule()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);
    entities.ObligorRule.Populated = false;

    return Read("ReadObligorRule",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate", local.HighDate.Date.GetValueOrDefault());
        db.SetNullableString(command, "cpaDType", entities.Obligor.Type1);
        db.SetNullableString(command, "cspDNumber", entities.Obligor.CspNumber);
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

  private void UpdateObligorRule()
  {
    var discontinueDate = import.ObligorRule.EffectiveDate;

    entities.ObligorRule.Populated = false;
    Update("UpdateObligorRule",
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
    /// <summary>
    /// A value of RecaptureRule.
    /// </summary>
    [JsonPropertyName("recaptureRule")]
    public RecaptureRule RecaptureRule
    {
      get => recaptureRule ??= new();
      set => recaptureRule = value;
    }

    private RecaptureRule recaptureRule;
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
    /// A value of CreateAttemptCount.
    /// </summary>
    [JsonPropertyName("createAttemptCount")]
    public Common CreateAttemptCount
    {
      get => createAttemptCount ??= new();
      set => createAttemptCount = value;
    }

    /// <summary>
    /// A value of HighDate.
    /// </summary>
    [JsonPropertyName("highDate")]
    public DateWorkArea HighDate
    {
      get => highDate ??= new();
      set => highDate = value;
    }

    private DateWorkArea current;
    private Common createAttemptCount;
    private DateWorkArea highDate;
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

    private CsePersonAccount obligor;
    private CsePerson csePerson;
    private RecaptureRule obligorRule;
  }
#endregion
}
