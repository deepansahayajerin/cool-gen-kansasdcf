// Program: SI_PEPR_VALIDATE_MIN_MAX_DATES, ID: 373297658, model: 746.
// Short name: SWE00533
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_PEPR_VALIDATE_MIN_MAX_DATES.
/// </para>
/// <para>
/// This AB reads all the programs that a person is currently on.  The new 
/// person program cannot be open at the same time as a protected program.  Also
/// if the new person program is NAI, it cannot be open at the same time as
/// AFI, FCI, or MAI.
/// </para>
/// </summary>
[Serializable]
public partial class SiPeprValidateMinMaxDates: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_PEPR_VALIDATE_MIN_MAX_DATES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiPeprValidateMinMaxDates(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiPeprValidateMinMaxDates.
  /// </summary>
  public SiPeprValidateMinMaxDates(IContext context, Import import,
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
    // **************************************************************
    // 09/06/00   M.Lachowicz    WR # 00188.
    //                          Validate Start and End dates for
    //                          Person Programs.
    // *************************************************************
    local.Current.Date = Now().Date;
    export.ValidDiscontinueDate.Flag = "Y";
    export.ValidEffectiveDate.Flag = "Y";

    switch(TrimEnd(import.ChProgram.Code))
    {
      case "AF":
        if (Lt(import.ChPersonProgram.EffectiveDate, new DateTime(1960, 1, 1)))
        {
          ExitState = "SI0000_ADD_PROGRAM_PRIOR_0101196";
          export.ValidEffectiveDate.Flag = "N";
        }

        if (Lt(new DateTime(1989, 7, 31), import.ChPersonProgram.DiscontinueDate)
          || Equal(import.ChPersonProgram.DiscontinueDate, local.Initial.Date))
        {
          ExitState = "SI0000_ADD_PA_PRIOR_08011989";
          export.ValidDiscontinueDate.Flag = "N";
        }

        break;
      case "NF":
        if (Lt(import.ChPersonProgram.EffectiveDate, new DateTime(1960, 1, 1)))
        {
          ExitState = "SI0000_ADD_PROGRAM_PRIOR_0101196";
          export.ValidEffectiveDate.Flag = "N";
        }

        if (Lt(local.Current.Date, import.ChPersonProgram.DiscontinueDate))
        {
          ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";
          export.ValidDiscontinueDate.Flag = "N";
        }

        break;
      case "FS":
        if (Lt(import.ChPersonProgram.EffectiveDate, new DateTime(1960, 1, 1)))
        {
          ExitState = "SI0000_ADD_PROGRAM_PRIOR_0101196";
          export.ValidEffectiveDate.Flag = "N";
        }

        if (Lt(new DateTime(1989, 7, 31), import.ChPersonProgram.DiscontinueDate)
          || Equal(import.ChPersonProgram.DiscontinueDate, local.Initial.Date))
        {
          ExitState = "SI0000_ADD_PA_PRIOR_08011989";
          export.ValidDiscontinueDate.Flag = "N";
        }

        break;
      case "CC":
        if (Lt(import.ChPersonProgram.EffectiveDate, new DateTime(1960, 1, 1)))
        {
          ExitState = "SI0000_ADD_PROGRAM_PRIOR_0101196";
          export.ValidEffectiveDate.Flag = "N";
        }

        if (Lt(new DateTime(1996, 2, 29), import.ChPersonProgram.DiscontinueDate)
          || Equal(import.ChPersonProgram.DiscontinueDate, local.Initial.Date))
        {
          ExitState = "SI0000_ADD_AFTER_02291996";
          export.ValidDiscontinueDate.Flag = "N";
        }

        break;
      case "MA":
        if (Lt(import.ChPersonProgram.EffectiveDate, new DateTime(1960, 1, 1)))
        {
          ExitState = "SI0000_ADD_PROGRAM_PRIOR_0101196";
          export.ValidEffectiveDate.Flag = "N";
        }

        if (Lt(new DateTime(1989, 7, 31), import.ChPersonProgram.DiscontinueDate)
          || Equal(import.ChPersonProgram.DiscontinueDate, local.Initial.Date))
        {
          ExitState = "SI0000_ADD_PA_PRIOR_08011989";
          export.ValidDiscontinueDate.Flag = "N";
        }

        break;
      case "MS":
        if (Lt(import.ChPersonProgram.EffectiveDate, new DateTime(1960, 1, 1)))
        {
          ExitState = "SI0000_ADD_PROGRAM_PRIOR_0101196";
          export.ValidEffectiveDate.Flag = "N";
        }

        if (Lt(new DateTime(1989, 7, 31), import.ChPersonProgram.DiscontinueDate)
          || Equal(import.ChPersonProgram.DiscontinueDate, local.Initial.Date))
        {
          ExitState = "SI0000_ADD_PA_PRIOR_08011989";
          export.ValidDiscontinueDate.Flag = "N";
        }

        break;
      case "CI":
        if (Lt(import.ChPersonProgram.EffectiveDate, new DateTime(1960, 1, 1)))
        {
          ExitState = "SI0000_ADD_PROGRAM_PRIOR_0101196";
          export.ValidEffectiveDate.Flag = "N";
        }

        if (Lt(new DateTime(1989, 7, 31), import.ChPersonProgram.DiscontinueDate)
          || Equal(import.ChPersonProgram.DiscontinueDate, local.Initial.Date))
        {
          ExitState = "SI0000_ADD_PA_PRIOR_08011989";
          export.ValidDiscontinueDate.Flag = "N";
        }

        break;
      case "SI":
        if (Lt(import.ChPersonProgram.EffectiveDate, new DateTime(1960, 1, 1)))
        {
          ExitState = "SI0000_ADD_PROGRAM_PRIOR_0101196";
          export.ValidEffectiveDate.Flag = "N";
        }

        if (Lt(new DateTime(1989, 7, 31), import.ChPersonProgram.DiscontinueDate)
          || Equal(import.ChPersonProgram.DiscontinueDate, local.Initial.Date))
        {
          ExitState = "SI0000_ADD_PA_PRIOR_08011989";
          export.ValidDiscontinueDate.Flag = "N";
        }

        break;
      case "MP":
        if (Lt(import.ChPersonProgram.EffectiveDate, new DateTime(1960, 1, 1)))
        {
          ExitState = "SI0000_ADD_PROGRAM_PRIOR_0101196";
          export.ValidEffectiveDate.Flag = "N";
        }

        if (Lt(new DateTime(1989, 7, 31), import.ChPersonProgram.DiscontinueDate)
          || Equal(import.ChPersonProgram.DiscontinueDate, local.Initial.Date))
        {
          ExitState = "SI0000_ADD_PA_PRIOR_08011989";
          export.ValidDiscontinueDate.Flag = "N";
        }

        break;
      case "MK":
        if (Lt(import.ChPersonProgram.EffectiveDate, new DateTime(1960, 1, 1)))
        {
          ExitState = "SI0000_ADD_PROGRAM_PRIOR_0101196";
          export.ValidEffectiveDate.Flag = "N";
        }

        if (Lt(new DateTime(1989, 7, 31), import.ChPersonProgram.DiscontinueDate)
          || Equal(import.ChPersonProgram.DiscontinueDate, local.Initial.Date))
        {
          ExitState = "SI0000_ADD_PA_PRIOR_08011989";
          export.ValidDiscontinueDate.Flag = "N";
        }

        break;
      case "NA":
        if (Lt(import.ChPersonProgram.EffectiveDate, new DateTime(1960, 1, 1)))
        {
          ExitState = "SI0000_ADD_PROGRAM_PRIOR_0101196";
          export.ValidEffectiveDate.Flag = "N";
        }

        if (Lt(local.Current.Date, import.ChPersonProgram.DiscontinueDate))
        {
          ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";
          export.ValidDiscontinueDate.Flag = "N";
        }

        break;
      case "NC":
        if (Lt(import.ChPersonProgram.EffectiveDate, new DateTime(1996, 7, 1)))
        {
          ExitState = "SI0000_ADD_PROGRAM_PRIOR_0701199";
          export.ValidEffectiveDate.Flag = "N";
        }

        if (Lt(local.Current.Date, import.ChPersonProgram.DiscontinueDate))
        {
          ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";
          export.ValidDiscontinueDate.Flag = "N";
        }

        break;
      case "AFI":
        if (Lt(import.ChPersonProgram.EffectiveDate, new DateTime(1960, 1, 1)))
        {
          ExitState = "SI0000_ADD_PROGRAM_PRIOR_0101196";
          export.ValidEffectiveDate.Flag = "N";
        }

        if (Lt(local.Current.Date, import.ChPersonProgram.DiscontinueDate))
        {
          ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";
          export.ValidDiscontinueDate.Flag = "N";
        }

        break;
      case "FC":
        if (Lt(import.ChPersonProgram.EffectiveDate, new DateTime(1960, 1, 1)))
        {
          ExitState = "SI0000_ADD_PROGRAM_PRIOR_0101196";
          export.ValidEffectiveDate.Flag = "N";
        }

        if (Lt(local.Current.Date, import.ChPersonProgram.DiscontinueDate))
        {
          ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";
          export.ValidDiscontinueDate.Flag = "N";
        }

        break;
      case "FCI":
        if (Lt(import.ChPersonProgram.EffectiveDate, new DateTime(1960, 1, 1)))
        {
          ExitState = "SI0000_ADD_PROGRAM_PRIOR_0101196";
          export.ValidEffectiveDate.Flag = "N";
        }

        if (Lt(local.Current.Date, import.ChPersonProgram.DiscontinueDate))
        {
          ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";
          export.ValidDiscontinueDate.Flag = "N";
        }

        break;
      case "MAI":
        if (Lt(import.ChPersonProgram.EffectiveDate, new DateTime(1960, 1, 1)))
        {
          ExitState = "SI0000_ADD_PROGRAM_PRIOR_0101196";
          export.ValidEffectiveDate.Flag = "N";
        }

        if (Lt(local.Current.Date, import.ChPersonProgram.DiscontinueDate))
        {
          ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";
          export.ValidDiscontinueDate.Flag = "N";
        }

        break;
      case "NAI":
        if (Lt(import.ChPersonProgram.EffectiveDate, new DateTime(1960, 1, 1)))
        {
          ExitState = "SI0000_ADD_PROGRAM_PRIOR_0101196";
          export.ValidEffectiveDate.Flag = "N";
        }

        if (Lt(local.Current.Date, import.ChPersonProgram.DiscontinueDate))
        {
          ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";
          export.ValidDiscontinueDate.Flag = "N";
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
  protected readonly Local local = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of ChPersonProgram.
    /// </summary>
    [JsonPropertyName("chPersonProgram")]
    public PersonProgram ChPersonProgram
    {
      get => chPersonProgram ??= new();
      set => chPersonProgram = value;
    }

    /// <summary>
    /// A value of ChProgram.
    /// </summary>
    [JsonPropertyName("chProgram")]
    public Program ChProgram
    {
      get => chProgram ??= new();
      set => chProgram = value;
    }

    private PersonProgram chPersonProgram;
    private Program chProgram;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ValidDiscontinueDate.
    /// </summary>
    [JsonPropertyName("validDiscontinueDate")]
    public Common ValidDiscontinueDate
    {
      get => validDiscontinueDate ??= new();
      set => validDiscontinueDate = value;
    }

    /// <summary>
    /// A value of ValidEffectiveDate.
    /// </summary>
    [JsonPropertyName("validEffectiveDate")]
    public Common ValidEffectiveDate
    {
      get => validEffectiveDate ??= new();
      set => validEffectiveDate = value;
    }

    private Common validDiscontinueDate;
    private Common validEffectiveDate;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Initial.
    /// </summary>
    [JsonPropertyName("initial")]
    public DateWorkArea Initial
    {
      get => initial ??= new();
      set => initial = value;
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

    private DateWorkArea initial;
    private DateWorkArea current;
  }
#endregion
}
