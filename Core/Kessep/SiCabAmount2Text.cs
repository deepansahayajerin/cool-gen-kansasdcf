// Program: SI_CAB_AMOUNT2TEXT, ID: 371766297, model: 746.
// Short name: SWE01831
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_CAB_AMOUNT2TEXT.
/// </para>
/// <para>
/// RESP: SRVINIT
/// Written by Raju : Dec 25 1996 : 1000 hrs CST
/// input  : real numeric 9(11).9(4)
/// output : text field of 14 chars
/// </para>
/// </summary>
[Serializable]
public partial class SiCabAmount2Text: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CAB_AMOUNT2TEXT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCabAmount2Text(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCabAmount2Text.
  /// </summary>
  public SiCabAmount2Text(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------
    // Change log
    // date		author	remarks
    // 12/25/96	raju	initial creation
    // --------------------------------------------
    // --------------------------------------------
    // All those mortals who feel this peice of
    //   code is unnecessary be warned
    //   textnum truncates the decimal part if the
    //   input is a real or currency with valid
    //   decimal places.
    //   So we fool around and use the good old
    //   multiplication trick which we humans are
    //   so good at ( pun intended )
    // --------------------------------------------
    local.Workaround.TotalCurrency = import.Currency.TotalCurrency * 100;
    local.Text15.Text15 =
      NumberToString((long)local.Workaround.TotalCurrency, 15);

    for(local.Common.Count = 1; local.Common.Count <= 15; ++local.Common.Count)
    {
      local.CharPosition.MenuOption = Find(local.Text15.Text15, "0");

      if (local.CharPosition.MenuOption == 0)
      {
        break;
      }
      else
      {
        if (local.CharPosition.MenuOption != 1)
        {
          break;
        }

        local.Text15.Text15 =
          Substring(local.Text15.Text15, local.CharPosition.MenuOption + 1, 15 -
          local.CharPosition.MenuOption + 1);
      }
    }

    local.Text13.Text13 =
      Substring(local.Text15.Text15, 1, Length(TrimEnd(local.Text15.Text15)) - 2);
      
    local.Text3.Text3 = "." + Substring
      (local.Text15.Text15, WorkArea.Text15_MaxLength,
      Length(TrimEnd(local.Text15.Text15)) - 1, 2);
    export.Text.Text16 = TrimEnd(local.Text13.Text13) + local.Text3.Text3;
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
    /// A value of Currency.
    /// </summary>
    [JsonPropertyName("currency")]
    public Common Currency
    {
      get => currency ??= new();
      set => currency = value;
    }

    private Common currency;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Text.
    /// </summary>
    [JsonPropertyName("text")]
    public WorkArea Text
    {
      get => text ??= new();
      set => text = value;
    }

    private WorkArea text;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CharPosition.
    /// </summary>
    [JsonPropertyName("charPosition")]
    public Standard CharPosition
    {
      get => charPosition ??= new();
      set => charPosition = value;
    }

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of Workaround.
    /// </summary>
    [JsonPropertyName("workaround")]
    public Common Workaround
    {
      get => workaround ??= new();
      set => workaround = value;
    }

    /// <summary>
    /// A value of Text15.
    /// </summary>
    [JsonPropertyName("text15")]
    public WorkArea Text15
    {
      get => text15 ??= new();
      set => text15 = value;
    }

    /// <summary>
    /// A value of Text13.
    /// </summary>
    [JsonPropertyName("text13")]
    public WorkArea Text13
    {
      get => text13 ??= new();
      set => text13 = value;
    }

    /// <summary>
    /// A value of Text3.
    /// </summary>
    [JsonPropertyName("text3")]
    public WorkArea Text3
    {
      get => text3 ??= new();
      set => text3 = value;
    }

    private Standard charPosition;
    private Common common;
    private Common workaround;
    private WorkArea text15;
    private WorkArea text13;
    private WorkArea text3;
  }
#endregion
}
