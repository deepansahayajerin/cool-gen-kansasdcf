// Program: EXT_WRITE_SDSO_TO_TAPE, ID: 372663768, model: 746.
// Short name: SWEXLE02
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: EXT_WRITE_SDSO_TO_TAPE.
/// </para>
/// <para>
/// RESP: FNCLMNGT
/// This action block will be used in batch processes to access a driver file.  
/// It will receive an access instruction (OPEN, READ, CLOSE, POSITION) and will
/// send a return code and driver record.
/// </para>
/// </summary>
[Serializable]
public partial class ExtWriteSdsoToTape: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EXT_WRITE_SDSO_TO_TAPE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ExtWriteSdsoToTape(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ExtWriteSdsoToTape.
  /// </summary>
  public ExtWriteSdsoToTape(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    GetService<IEabStub>().Execute("SWEXLE02", context, import, export, 0);
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
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 1, Members = new[] { "FileInstruction" })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    /// <summary>
    /// A value of SdsoCertificationTapeRecord.
    /// </summary>
    [JsonPropertyName("sdsoCertificationTapeRecord")]
    [Member(Index = 2, AccessFields = false, Members
      = new[] { "CertificationRecord" })]
    public SdsoCertificationTapeRecord SdsoCertificationTapeRecord
    {
      get => sdsoCertificationTapeRecord ??= new();
      set => sdsoCertificationTapeRecord = value;
    }

    private External external;
    private SdsoCertificationTapeRecord sdsoCertificationTapeRecord;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 1, Members = new[] { "NumericReturnCode", "TextReturnCode" })
      ]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    private External external;
  }
#endregion
}
