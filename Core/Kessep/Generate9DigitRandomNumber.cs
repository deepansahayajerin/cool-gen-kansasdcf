// Program: GENERATE_9_DIGIT_RANDOM_NUMBER, ID: 371722215, model: 746.
// Short name: SWE00702
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: GENERATE_9_DIGIT_RANDOM_NUMBER.
/// </para>
/// <para>
/// RESP: FNCLMGMT
/// This action block will generate a 9 digit random number using the last nine 
/// digits of the time.
/// </para>
/// </summary>
[Serializable]
public partial class Generate9DigitRandomNumber: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the GENERATE_9_DIGIT_RANDOM_NUMBER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new Generate9DigitRandomNumber(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of Generate9DigitRandomNumber.
  /// </summary>
  public Generate9DigitRandomNumber(IContext context, Import import,
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
    // CHANGE LOG:
    // ----------------------------------------------------------------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // --------  ----------	-------------	
    // ------------------------------------------------------------------------------
    // ??/??/??  ??????	????????	Initial Development
    // 02/05/09  GVandy	CQ#8960		New algorithm for generating random numbers.
    // ----------------------------------------------------------------------------------------------------------------------
    // -----------------------------------------------------------------------------------------
    // 02/05/09  New algorithm for a 9 digit random number is as follows:
    // 	Digits	Derived From					Possible Values
    // 	------	
    // --------------------------------
    // 		-------------------------
    // 	1 to 6	Current microseconds (inverted)			000000 to 999999
    // 	7	Least significant digit of current second	0 to 9
    // 	8	Least significant digit of current minute	0 to 9
    // 	9	Least significant digit of a new microsecond	0 to 9
    // -----------------------------------------------------------------------------------------
    local.DateWorkArea.Timestamp = Now();
    local.Microseconds.Text8 =
      NumberToString(Microsecond(local.DateWorkArea.Timestamp), 8);

    // -- Positions 1 to 6 are the current microsecond inverted.
    for(local.Common.Count = 8; local.Common.Count >= 3; local
      .Common.Count += -1)
    {
      // -- Invert the microsecond value.  This will cause a wider range of 
      // values to be returned in
      //    instances where this routine is called repeatedly in succession.
      local.RandomNumber.Text10 = TrimEnd(local.RandomNumber.Text10) + Substring
        (local.Microseconds.Text8, TextWorkArea.Text8_MaxLength,
        local.Common.Count, 1);
    }

    // -- Position 7 is the least significant digit of the current second.
    local.RandomNumber.Text10 = TrimEnd(local.RandomNumber.Text10) + NumberToString
      (Second(local.DateWorkArea.Timestamp), 15, 1);

    // -- Position 8 is the least significant digit of the current minute.
    local.RandomNumber.Text10 = TrimEnd(local.RandomNumber.Text10) + NumberToString
      (Minute(local.DateWorkArea.Timestamp), 15, 1);

    // -- Position 9 is a new random value extracted from the least significant 
    // digit of a new microsecond value.
    local.RandomNumber.Text10 = TrimEnd(local.RandomNumber.Text10) + NumberToString
      (Microsecond(Now()), 15, 1);
    export.SystemGenerated.Attribute9DigitRandomNumber =
      (int)StringToNumber(local.RandomNumber.Text10);

    if (export.SystemGenerated.Attribute9DigitRandomNumber == 0)
    {
      // -- Don't return a zero value.  Most programs expect a non zero value.
      export.SystemGenerated.Attribute9DigitRandomNumber = 1;
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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of SystemGenerated.
    /// </summary>
    [JsonPropertyName("systemGenerated")]
    public SystemGenerated SystemGenerated
    {
      get => systemGenerated ??= new();
      set => systemGenerated = value;
    }

    private SystemGenerated systemGenerated;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of RandomNumber.
    /// </summary>
    [JsonPropertyName("randomNumber")]
    public TextWorkArea RandomNumber
    {
      get => randomNumber ??= new();
      set => randomNumber = value;
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
    /// A value of Microseconds.
    /// </summary>
    [JsonPropertyName("microseconds")]
    public TextWorkArea Microseconds
    {
      get => microseconds ??= new();
      set => microseconds = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of ZdelLocalDigits1To6.
    /// </summary>
    [JsonPropertyName("zdelLocalDigits1To6")]
    public Common ZdelLocalDigits1To6
    {
      get => zdelLocalDigits1To6 ??= new();
      set => zdelLocalDigits1To6 = value;
    }

    /// <summary>
    /// A value of ZdelLocalDigit7.
    /// </summary>
    [JsonPropertyName("zdelLocalDigit7")]
    public Common ZdelLocalDigit7
    {
      get => zdelLocalDigit7 ??= new();
      set => zdelLocalDigit7 = value;
    }

    /// <summary>
    /// A value of ZdelLocalDigit8.
    /// </summary>
    [JsonPropertyName("zdelLocalDigit8")]
    public Common ZdelLocalDigit8
    {
      get => zdelLocalDigit8 ??= new();
      set => zdelLocalDigit8 = value;
    }

    /// <summary>
    /// A value of ZdelLocalDigit9.
    /// </summary>
    [JsonPropertyName("zdelLocalDigit9")]
    public Common ZdelLocalDigit9
    {
      get => zdelLocalDigit9 ??= new();
      set => zdelLocalDigit9 = value;
    }

    /// <summary>
    /// A value of ZdelLocalSeconds.
    /// </summary>
    [JsonPropertyName("zdelLocalSeconds")]
    public Common ZdelLocalSeconds
    {
      get => zdelLocalSeconds ??= new();
      set => zdelLocalSeconds = value;
    }

    private TextWorkArea randomNumber;
    private Common common;
    private TextWorkArea microseconds;
    private DateWorkArea dateWorkArea;
    private Common zdelLocalDigits1To6;
    private Common zdelLocalDigit7;
    private Common zdelLocalDigit8;
    private Common zdelLocalDigit9;
    private Common zdelLocalSeconds;
  }
#endregion
}
