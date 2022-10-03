// Program: SI_CAB_DETERMINE_ACTIVE_INCOME, ID: 373411338, model: 746.
// Short name: SWE02635
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_CAB_DETERMINE_ACTIVE_INCOME.
/// </summary>
[Serializable]
public partial class SiCabDetermineActiveIncome: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CAB_DETERMINE_ACTIVE_INCOME program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCabDetermineActiveIncome(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCabDetermineActiveIncome.
  /// </summary>
  public SiCabDetermineActiveIncome(IContext context, Import import,
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
    export.Active.Flag = "N";

    switch(import.Aging.Month)
    {
      case 1:
        if (import.IncomeSource.LastQtrYr.GetValueOrDefault() >= import
          .Aging.Year && import
          .IncomeSource.LastQtrIncome.GetValueOrDefault() > 0)
        {
          export.Active.Flag = "Y";
        }

        if (import.IncomeSource.Attribute2NdQtrYr.GetValueOrDefault() >= import
          .Aging.Year && import
          .IncomeSource.Attribute2NdQtrIncome.GetValueOrDefault() > 0)
        {
          export.Active.Flag = "Y";
        }

        break;
      case 2:
        if (import.IncomeSource.LastQtrYr.GetValueOrDefault() >= import
          .Aging.Year && import
          .IncomeSource.LastQtrIncome.GetValueOrDefault() > 0)
        {
          export.Active.Flag = "Y";
        }

        if (import.IncomeSource.Attribute2NdQtrYr.GetValueOrDefault() >= import
          .Aging.Year && import
          .IncomeSource.Attribute2NdQtrIncome.GetValueOrDefault() > 0)
        {
          export.Active.Flag = "Y";
        }

        break;
      case 3:
        if (import.IncomeSource.LastQtrYr.GetValueOrDefault() >= import
          .Aging.Year && import
          .IncomeSource.LastQtrIncome.GetValueOrDefault() > 0)
        {
          export.Active.Flag = "Y";
        }

        if (import.IncomeSource.Attribute2NdQtrYr.GetValueOrDefault() >= import
          .Aging.Year && import
          .IncomeSource.Attribute2NdQtrIncome.GetValueOrDefault() > 0)
        {
          export.Active.Flag = "Y";
        }

        break;
      case 4:
        if (import.IncomeSource.Attribute2NdQtrYr.GetValueOrDefault() >= import
          .Aging.Year && import
          .IncomeSource.Attribute2NdQtrIncome.GetValueOrDefault() > 0)
        {
          export.Active.Flag = "Y";
        }

        if (import.IncomeSource.Attribute3RdQtrYr.GetValueOrDefault() >= import
          .Aging.Year && import
          .IncomeSource.Attribute3RdQtrIncome.GetValueOrDefault() > 0)
        {
          export.Active.Flag = "Y";
        }

        break;
      case 5:
        if (import.IncomeSource.Attribute2NdQtrYr.GetValueOrDefault() >= import
          .Aging.Year && import
          .IncomeSource.Attribute2NdQtrIncome.GetValueOrDefault() > 0)
        {
          export.Active.Flag = "Y";
        }

        if (import.IncomeSource.Attribute3RdQtrYr.GetValueOrDefault() >= import
          .Aging.Year && import
          .IncomeSource.Attribute3RdQtrIncome.GetValueOrDefault() > 0)
        {
          export.Active.Flag = "Y";
        }

        break;
      case 6:
        if (import.IncomeSource.Attribute2NdQtrYr.GetValueOrDefault() >= import
          .Aging.Year && import
          .IncomeSource.Attribute2NdQtrIncome.GetValueOrDefault() > 0)
        {
          export.Active.Flag = "Y";
        }

        if (import.IncomeSource.Attribute3RdQtrYr.GetValueOrDefault() >= import
          .Aging.Year && import
          .IncomeSource.Attribute3RdQtrIncome.GetValueOrDefault() > 0)
        {
          export.Active.Flag = "Y";
        }

        break;
      case 7:
        if (import.IncomeSource.Attribute3RdQtrYr.GetValueOrDefault() >= import
          .Aging.Year && import
          .IncomeSource.Attribute3RdQtrIncome.GetValueOrDefault() > 0)
        {
          export.Active.Flag = "Y";
        }

        if (import.IncomeSource.Attribute4ThQtrYr.GetValueOrDefault() >= import
          .Aging.Year && import
          .IncomeSource.Attribute4ThQtrIncome.GetValueOrDefault() > 0)
        {
          export.Active.Flag = "Y";
        }

        break;
      case 8:
        if (import.IncomeSource.Attribute3RdQtrYr.GetValueOrDefault() >= import
          .Aging.Year && import
          .IncomeSource.Attribute3RdQtrIncome.GetValueOrDefault() > 0)
        {
          export.Active.Flag = "Y";
        }

        if (import.IncomeSource.Attribute4ThQtrYr.GetValueOrDefault() >= import
          .Aging.Year && import
          .IncomeSource.Attribute4ThQtrIncome.GetValueOrDefault() > 0)
        {
          export.Active.Flag = "Y";
        }

        break;
      case 9:
        if (import.IncomeSource.Attribute3RdQtrYr.GetValueOrDefault() >= import
          .Aging.Year && import
          .IncomeSource.Attribute3RdQtrIncome.GetValueOrDefault() > 0)
        {
          export.Active.Flag = "Y";
        }

        if (import.IncomeSource.Attribute4ThQtrYr.GetValueOrDefault() >= import
          .Aging.Year && import
          .IncomeSource.Attribute4ThQtrIncome.GetValueOrDefault() > 0)
        {
          export.Active.Flag = "Y";
        }

        break;
      case 10:
        if (import.IncomeSource.Attribute4ThQtrYr.GetValueOrDefault() >= import
          .Aging.Year && import
          .IncomeSource.Attribute4ThQtrIncome.GetValueOrDefault() > 0)
        {
          export.Active.Flag = "Y";
        }

        if (import.IncomeSource.LastQtrYr.GetValueOrDefault() > import
          .Aging.Year && import
          .IncomeSource.LastQtrIncome.GetValueOrDefault() > 0)
        {
          export.Active.Flag = "Y";
        }

        break;
      case 11:
        if (import.IncomeSource.Attribute4ThQtrYr.GetValueOrDefault() >= import
          .Aging.Year && import
          .IncomeSource.Attribute4ThQtrIncome.GetValueOrDefault() > 0)
        {
          export.Active.Flag = "Y";
        }

        if (import.IncomeSource.LastQtrYr.GetValueOrDefault() > import
          .Aging.Year && import
          .IncomeSource.LastQtrIncome.GetValueOrDefault() > 0)
        {
          export.Active.Flag = "Y";
        }

        break;
      case 12:
        if (import.IncomeSource.Attribute4ThQtrYr.GetValueOrDefault() >= import
          .Aging.Year && import
          .IncomeSource.Attribute4ThQtrIncome.GetValueOrDefault() > 0)
        {
          export.Active.Flag = "Y";
        }

        if (import.IncomeSource.LastQtrYr.GetValueOrDefault() > import
          .Aging.Year && import
          .IncomeSource.LastQtrIncome.GetValueOrDefault() > 0)
        {
          export.Active.Flag = "Y";
        }

        break;
      default:
        break;
    }
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of Aging.
    /// </summary>
    [JsonPropertyName("aging")]
    public DateWorkArea Aging
    {
      get => aging ??= new();
      set => aging = value;
    }

    private IncomeSource incomeSource;
    private DateWorkArea aging;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Active.
    /// </summary>
    [JsonPropertyName("active")]
    public Common Active
    {
      get => active ??= new();
      set => active = value;
    }

    private Common active;
  }
#endregion
}
