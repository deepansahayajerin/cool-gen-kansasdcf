// Program: SI_GEN_CSENET_TRANSACT_SERIAL_NO, ID: 372382229, model: 746.
// Short name: SWE01167
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_GEN_CSENET_TRANSACT_SERIAL_NO.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiGenCsenetTransactSerialNo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_GEN_CSENET_TRANSACT_SERIAL_NO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiGenCsenetTransactSerialNo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiGenCsenetTransactSerialNo.
  /// </summary>
  public SiGenCsenetTransactSerialNo(IContext context, Import import,
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
    // ------------------------------------------------------------------
    // 04/29/97    JeHoward          Current date fix.
    // ------------------------------------------------------------------
    local.Current.Date = Now().Date;

    do
    {
      // ***   Set the serial number to date and time   ***
      local.Microseconds.Text6 = NumberToString(Microsecond(Now()), 6);
      local.Time.Text6 = NumberToString(TimeToInt(Time(Now())), 6);
      local.OeWorkGroup.TransSerialNumber =
        StringToNumber(local.Microseconds.Text6 + local.Time.Text6);
      local.InterstateCase.TransSerialNumber =
        local.OeWorkGroup.TransSerialNumber;

      if (ReadInterstateCase())
      {
        continue;
      }
      else
      {
        export.InterstateCase.TransSerialNumber =
          local.InterstateCase.TransSerialNumber;
        export.InterstateCase.TransactionDate = local.Current.Date;
      }
    }
    while(export.InterstateCase.TransSerialNumber == 0);
  }

  private bool ReadInterstateCase()
  {
    entities.InterstateCase.Populated = false;

    return Read("ReadInterstateCase",
      (db, command) =>
      {
        db.SetInt64(
          command, "transSerialNbr", local.InterstateCase.TransSerialNumber);
        db.SetDate(
          command, "transactionDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 0);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 1);
        entities.InterstateCase.Populated = true;
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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    private InterstateCase interstateCase;
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
    /// A value of Microseconds.
    /// </summary>
    [JsonPropertyName("microseconds")]
    public WorkArea Microseconds
    {
      get => microseconds ??= new();
      set => microseconds = value;
    }

    /// <summary>
    /// A value of Time.
    /// </summary>
    [JsonPropertyName("time")]
    public WorkArea Time
    {
      get => time ??= new();
      set => time = value;
    }

    /// <summary>
    /// A value of OeWorkGroup.
    /// </summary>
    [JsonPropertyName("oeWorkGroup")]
    public OeWorkGroup OeWorkGroup
    {
      get => oeWorkGroup ??= new();
      set => oeWorkGroup = value;
    }

    /// <summary>
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    private DateWorkArea current;
    private WorkArea microseconds;
    private WorkArea time;
    private OeWorkGroup oeWorkGroup;
    private WorkArea workArea;
    private InterstateCase interstateCase;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    private InterstateCase interstateCase;
  }
#endregion
}
