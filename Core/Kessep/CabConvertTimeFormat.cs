// Program: CAB_CONVERT_TIME_FORMAT, ID: 371793134, model: 746.
// Short name: SWE00030
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CAB_CONVERT_TIME_FORMAT.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This common action block converts the time format
///  - from time view to a character format
///    'HHMM X' where X is A/P (am/pm).
///  - from 'HHMM X' format to time view.
/// depending on which view is supplied.
/// </para>
/// </summary>
[Serializable]
public partial class CabConvertTimeFormat: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_CONVERT_TIME_FORMAT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabConvertTimeFormat(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabConvertTimeFormat.
  /// </summary>
  public CabConvertTimeFormat(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------
    // Changes made to this CAB by Sid. 07/30/1997.
    // Valid times are 0100A/P - 1259A/P. 00hr is not valid.
    // Store 1201A - 1259A as 0001A - 0059A.
    // ---------------------------------------------
    MoveWorkTime(import.WorkTime, export.WorkTime);
    export.ErrorInConversion.Flag = "N";

    if (IsEmpty(export.WorkTime.TimeWithAmPm))
    {
      // ---------------------------------------------
      // Convert time view to char view (During Display)
      // ---------------------------------------------
      if (export.WorkTime.Wtime.Hours == 24)
      {
        // ---------------------------------------------
        // DB2/2 seems to allow a time of 24:00:00 !!
        // ---------------------------------------------
        export.WorkTime.Wtime -= new TimeSpan(12, 0, 0);
        local.AmPmInd.Text1 = "P";
      }
      else if (export.WorkTime.Wtime.Hours <= 12)
      {
        local.AmPmInd.Text1 = "A";

        if (export.WorkTime.Wtime.Hours == 0)
        {
          export.WorkTime.Wtime += new TimeSpan(12, 0, 0);
        }
        else if (export.WorkTime.Wtime.Hours == 12 && export
          .WorkTime.Wtime.Minutes > 0)
        {
          local.AmPmInd.Text1 = "P";
        }
      }
      else if (export.WorkTime.Wtime.Hours > 12)
      {
        local.AmPmInd.Text1 = "P";
        export.WorkTime.Wtime -= new TimeSpan(12, 0, 0);
      }

      export.WorkTime.TimeWithAmPm =
        NumberToString(export.WorkTime.Wtime.Hours, 14, 2) + NumberToString
        (export.WorkTime.Wtime.Minutes, 14, 2) + local.AmPmInd.Text1;
    }
    else
    {
      // ---------------------------------------------
      // Convert char view to time view (During create)
      // ---------------------------------------------
      if (Verify(Substring(
        export.WorkTime.TimeWithAmPm, WorkTime.TimeWithAmPm_MaxLength, 1, 4),
        " 0123456789") != 0)
      {
        export.ErrorInConversion.Flag = "Y";
        ExitState = "INVALID_TIME";

        return;
      }

      local.WorkTime.Hh24 =
        (int)StringToNumber(Substring(
          export.WorkTime.TimeWithAmPm, WorkTime.TimeWithAmPm_MaxLength, 1, 2));
        
      local.WorkTime.Mi =
        (int)StringToNumber(Substring(
          export.WorkTime.TimeWithAmPm, WorkTime.TimeWithAmPm_MaxLength, 3, 2));
        

      if (local.WorkTime.Hh24 > 12 || local.WorkTime.Hh24 == 0 || local
        .WorkTime.Mi > 59)
      {
        export.ErrorInConversion.Flag = "Y";
        ExitState = "INVALID_TIME";

        return;
      }

      local.WorkTime.Ss = 0;

      switch(TrimEnd(Substring(export.WorkTime.TimeWithAmPm, 5, 1)))
      {
        case "A":
          if (local.WorkTime.Hh24 == 12)
          {
            if (local.WorkTime.Mi > 0)
            {
              local.WorkTime.Hh24 -= 12;
            }
          }

          break;
        case "P":
          if (local.WorkTime.Hh24 != 12 || local.WorkTime.Hh24 == 12 && local
            .WorkTime.Mi == 0)
          {
            local.WorkTime.Hh24 += 12;
          }

          break;
        default:
          ExitState = "INVALID_TIME";
          export.ErrorInConversion.Flag = "Y";

          return;
      }

      export.WorkTime.Wtime = IntToTime(local.WorkTime.Hh24 * 10000 + local
        .WorkTime.Mi * 100 + local.WorkTime.Ss);
    }
  }

  private static void MoveWorkTime(WorkTime source, WorkTime target)
  {
    target.Wtime = source.Wtime;
    target.TimeWithAmPm = source.TimeWithAmPm;
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
    /// A value of WorkTime.
    /// </summary>
    [JsonPropertyName("workTime")]
    public WorkTime WorkTime
    {
      get => workTime ??= new();
      set => workTime = value;
    }

    private WorkTime workTime;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ErrorInConversion.
    /// </summary>
    [JsonPropertyName("errorInConversion")]
    public Common ErrorInConversion
    {
      get => errorInConversion ??= new();
      set => errorInConversion = value;
    }

    /// <summary>
    /// A value of WorkTime.
    /// </summary>
    [JsonPropertyName("workTime")]
    public WorkTime WorkTime
    {
      get => workTime ??= new();
      set => workTime = value;
    }

    private Common errorInConversion;
    private WorkTime workTime;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of WorkTime.
    /// </summary>
    [JsonPropertyName("workTime")]
    public WorkTime WorkTime
    {
      get => workTime ??= new();
      set => workTime = value;
    }

    /// <summary>
    /// A value of AmPmInd.
    /// </summary>
    [JsonPropertyName("amPmInd")]
    public WorkArea AmPmInd
    {
      get => amPmInd ??= new();
      set => amPmInd = value;
    }

    private WorkTime workTime;
    private WorkArea amPmInd;
  }
#endregion
}
