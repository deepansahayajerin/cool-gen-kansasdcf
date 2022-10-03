// Program: SI_READ_CSENET_INFO, ID: 372517560, model: 746.
// Short name: SWE01216
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_READ_CSENET_INFO.
/// </para>
/// <para>
/// RESP: SRVINIT
/// Read the CSENet miscellaneous information data block (entity).
/// </para>
/// </summary>
[Serializable]
public partial class SiReadCsenetInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_READ_CSENET_INFO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiReadCsenetInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiReadCsenetInfo.
  /// </summary>
  public SiReadCsenetInfo(IContext context, Import import, Export export):
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
    //         M A I N T E N A N C E   L O G
    // Date     Developer    Request #  Description
    // 6/12/95  Sherri Newman      0    Initial Dev.
    // ---------------------------------------------
    // ---------------------------------------------
    //   This PAB performs the READ's that populate
    //   the CSENet Miscellaneous Information
    //   Screen.
    // ---------------------------------------------
    if (ReadInterstateMiscellaneousInterstateCase())
    {
      export.InterstateMiscellaneous.Assign(entities.InterstateMiscellaneous);
      export.InterstateCase.AttachmentsInd =
        entities.InterstateCase.AttachmentsInd;
    }
    else
    {
      ExitState = "REFERRAL_MISC_INFO_NF";
    }
  }

  private bool ReadInterstateMiscellaneousInterstateCase()
  {
    entities.InterstateMiscellaneous.Populated = false;
    entities.InterstateCase.Populated = false;

    return Read("ReadInterstateMiscellaneousInterstateCase",
      (db, command) =>
      {
        db.SetInt64(
          command, "transSerialNbr", import.InterstateCase.TransSerialNumber);
        db.SetDate(
          command, "transactionDate",
          import.InterstateCase.TransactionDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateMiscellaneous.StatusChangeCode =
          db.GetString(reader, 0);
        entities.InterstateMiscellaneous.NewCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateMiscellaneous.InformationTextLine1 =
          db.GetNullableString(reader, 2);
        entities.InterstateMiscellaneous.InformationTextLine2 =
          db.GetNullableString(reader, 3);
        entities.InterstateMiscellaneous.InformationTextLine3 =
          db.GetNullableString(reader, 4);
        entities.InterstateMiscellaneous.CcaTransSerNum =
          db.GetInt64(reader, 5);
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 5);
        entities.InterstateMiscellaneous.CcaTransactionDt =
          db.GetDate(reader, 6);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 6);
        entities.InterstateMiscellaneous.InformationTextLine4 =
          db.GetNullableString(reader, 7);
        entities.InterstateMiscellaneous.InformationTextLine5 =
          db.GetNullableString(reader, 8);
        entities.InterstateCase.AttachmentsInd = db.GetString(reader, 9);
        entities.InterstateMiscellaneous.Populated = true;
        entities.InterstateCase.Populated = true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
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

    /// <summary>
    /// A value of InterstateMiscellaneous.
    /// </summary>
    [JsonPropertyName("interstateMiscellaneous")]
    public InterstateMiscellaneous InterstateMiscellaneous
    {
      get => interstateMiscellaneous ??= new();
      set => interstateMiscellaneous = value;
    }

    private InterstateCase interstateCase;
    private InterstateMiscellaneous interstateMiscellaneous;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InterstateMiscellaneous.
    /// </summary>
    [JsonPropertyName("interstateMiscellaneous")]
    public InterstateMiscellaneous InterstateMiscellaneous
    {
      get => interstateMiscellaneous ??= new();
      set => interstateMiscellaneous = value;
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

    private InterstateMiscellaneous interstateMiscellaneous;
    private InterstateCase interstateCase;
  }
#endregion
}
