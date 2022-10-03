// Program: FN_CAB_CREATE_OBLIGATION_SUMMARY, ID: 372692922, model: 746.
// Short name: SWE02407
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CAB_CREATE_OBLIGATION_SUMMARY.
/// </summary>
[Serializable]
public partial class FnCabCreateObligationSummary: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CAB_CREATE_OBLIGATION_SUMMARY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCabCreateObligationSummary(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCabCreateObligationSummary.
  /// </summary>
  public FnCabCreateObligationSummary(IContext context, Import import,
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
    local.DuplicateId.Count = 0;

    if (ReadMonthlyObligorSummary())
    {
      ExitState = "FN0000_MTH_OBLIGOR_SUM_AE";
    }
    else
    {
      do
      {
        ExitState = "ACO_NN0000_ALL_OK";

        try
        {
          CreateMonthlyObligorSummary();

          return;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_MTH_OBLIGOR_SUM_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_MTH_OBLIGOR_SUM_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        ++local.DuplicateId.Count;
      }
      while(local.DuplicateId.Count <= 99);
    }
  }

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void CreateMonthlyObligorSummary()
  {
    System.Diagnostics.Debug.Assert(import.Obligation.Populated);

    var type1 = import.New1.Type1;
    var yearMonth = import.New1.YearMonth;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var cpaSType = import.Obligation.CpaType;
    var cspSNumber = import.Obligation.CspNumber;
    var obgSGeneratedId = import.Obligation.SystemGeneratedIdentifier;
    var otyType = import.Obligation.DtyGeneratedId;
    var forMthCurrBal = import.New1.ForMthCurrBal.GetValueOrDefault();

    CheckValid<MonthlyObligorSummary>("Type1", type1);
    CheckValid<MonthlyObligorSummary>("CpaSType", cpaSType);
    entities.MonthlyObligorSummary.Populated = false;
    Update("CreateMonthlyObligorSummary",
      (db, command) =>
      {
        db.SetString(command, "fnclMsumTyp", type1);
        db.SetInt32(command, "fnclMsumYrMth", yearMonth);
        db.SetInt32(command, "mfsGeneratedId", systemGeneratedIdentifier);
        db.SetNullableString(command, "cpaSType", cpaSType);
        db.SetNullableString(command, "cspSNumber", cspSNumber);
        db.SetNullableInt32(command, "obgSGeneratedId", obgSGeneratedId);
        db.SetNullableString(
          command, "cpaType", GetImplicitValue<MonthlyObligorSummary,
          string>("CpaType"));
        db.SetNullableInt32(command, "otyType", otyType);
        db.SetNullableDecimal(command, "tmGiftColl", 0M);
        db.SetNullableDecimal(command, "fmCurrBal", forMthCurrBal);
      });

    entities.MonthlyObligorSummary.Type1 = type1;
    entities.MonthlyObligorSummary.YearMonth = yearMonth;
    entities.MonthlyObligorSummary.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.MonthlyObligorSummary.CpaSType = cpaSType;
    entities.MonthlyObligorSummary.CspSNumber = cspSNumber;
    entities.MonthlyObligorSummary.ObgSGeneratedId = obgSGeneratedId;
    entities.MonthlyObligorSummary.OtyType = otyType;
    entities.MonthlyObligorSummary.ForMthCurrBal = forMthCurrBal;
    entities.MonthlyObligorSummary.Populated = true;
  }

  private bool ReadMonthlyObligorSummary()
  {
    System.Diagnostics.Debug.Assert(import.Obligation.Populated);
    entities.MonthlyObligorSummary.Populated = false;

    return Read("ReadMonthlyObligorSummary",
      (db, command) =>
      {
        db.
          SetNullableInt32(command, "otyType", import.Obligation.DtyGeneratedId);
          
        db.SetNullableInt32(
          command, "obgSGeneratedId",
          import.Obligation.SystemGeneratedIdentifier);
        db.
          SetNullableString(command, "cspSNumber", import.Obligation.CspNumber);
          
        db.SetNullableString(command, "cpaSType", import.Obligation.CpaType);
        db.SetInt32(command, "fnclMsumYrMth", import.New1.YearMonth);
      },
      (db, reader) =>
      {
        entities.MonthlyObligorSummary.Type1 = db.GetString(reader, 0);
        entities.MonthlyObligorSummary.YearMonth = db.GetInt32(reader, 1);
        entities.MonthlyObligorSummary.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.MonthlyObligorSummary.CpaSType =
          db.GetNullableString(reader, 3);
        entities.MonthlyObligorSummary.CspSNumber =
          db.GetNullableString(reader, 4);
        entities.MonthlyObligorSummary.ObgSGeneratedId =
          db.GetNullableInt32(reader, 5);
        entities.MonthlyObligorSummary.OtyType = db.GetNullableInt32(reader, 6);
        entities.MonthlyObligorSummary.ForMthCurrBal =
          db.GetNullableDecimal(reader, 7);
        entities.MonthlyObligorSummary.Populated = true;
        CheckValid<MonthlyObligorSummary>("Type1",
          entities.MonthlyObligorSummary.Type1);
        CheckValid<MonthlyObligorSummary>("CpaSType",
          entities.MonthlyObligorSummary.CpaSType);
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public MonthlyObligorSummary New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    private Obligation obligation;
    private MonthlyObligorSummary new1;
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
    /// A value of DuplicateId.
    /// </summary>
    [JsonPropertyName("duplicateId")]
    public Common DuplicateId
    {
      get => duplicateId ??= new();
      set => duplicateId = value;
    }

    private Common duplicateId;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of MonthlyObligorSummary.
    /// </summary>
    [JsonPropertyName("monthlyObligorSummary")]
    public MonthlyObligorSummary MonthlyObligorSummary
    {
      get => monthlyObligorSummary ??= new();
      set => monthlyObligorSummary = value;
    }

    private MonthlyObligorSummary monthlyObligorSummary;
  }
#endregion
}
