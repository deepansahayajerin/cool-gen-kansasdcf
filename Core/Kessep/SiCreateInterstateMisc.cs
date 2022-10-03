// Program: SI_CREATE_INTERSTATE_MISC, ID: 371084005, model: 746.
// Short name: SWE02623
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_CREATE_INTERSTATE_MISC.
/// </summary>
[Serializable]
public partial class SiCreateInterstateMisc: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CREATE_INTERSTATE_MISC program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCreateInterstateMisc(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCreateInterstateMisc.
  /// </summary>
  public SiCreateInterstateMisc(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------------------------------------
    // Date		Developer	Request		Description
    // --------------------------------------------------------------------------
    // 2001/04/16	M Ramirez			Initial Development
    // --------------------------------------------------------------------------
    if (!ReadInterstateCase())
    {
      ExitState = "INTERSTATE_CASE_NF";

      return;
    }

    try
    {
      CreateInterstateMiscellaneous();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "SI0000_INTERSTATE_MISC_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "SI0000_INTERSTATE_MISC_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void CreateInterstateMiscellaneous()
  {
    var statusChangeCode = import.InterstateMiscellaneous.StatusChangeCode;
    var newCaseId = import.InterstateMiscellaneous.NewCaseId ?? "";
    var informationTextLine1 =
      import.InterstateMiscellaneous.InformationTextLine1 ?? "";
    var informationTextLine2 =
      import.InterstateMiscellaneous.InformationTextLine2 ?? "";
    var informationTextLine3 =
      import.InterstateMiscellaneous.InformationTextLine3 ?? "";
    var ccaTransSerNum = entities.InterstateCase.TransSerialNumber;
    var ccaTransactionDt = entities.InterstateCase.TransactionDate;
    var informationTextLine4 =
      import.InterstateMiscellaneous.InformationTextLine4 ?? "";
    var informationTextLine5 =
      import.InterstateMiscellaneous.InformationTextLine5 ?? "";

    entities.InterstateMiscellaneous.Populated = false;
    Update("CreateInterstateMiscellaneous",
      (db, command) =>
      {
        db.SetString(command, "statusChangeCode", statusChangeCode);
        db.SetNullableString(command, "newCaseId", newCaseId);
        db.SetNullableString(command, "infoText1", informationTextLine1);
        db.SetNullableString(command, "infoText2", informationTextLine2);
        db.SetNullableString(command, "infoText3", informationTextLine3);
        db.SetInt64(command, "ccaTransSerNum", ccaTransSerNum);
        db.SetDate(command, "ccaTransactionDt", ccaTransactionDt);
        db.SetNullableString(command, "infoTextLine4", informationTextLine4);
        db.SetNullableString(command, "infoTextLine5", informationTextLine5);
      });

    entities.InterstateMiscellaneous.StatusChangeCode = statusChangeCode;
    entities.InterstateMiscellaneous.NewCaseId = newCaseId;
    entities.InterstateMiscellaneous.InformationTextLine1 =
      informationTextLine1;
    entities.InterstateMiscellaneous.InformationTextLine2 =
      informationTextLine2;
    entities.InterstateMiscellaneous.InformationTextLine3 =
      informationTextLine3;
    entities.InterstateMiscellaneous.CcaTransSerNum = ccaTransSerNum;
    entities.InterstateMiscellaneous.CcaTransactionDt = ccaTransactionDt;
    entities.InterstateMiscellaneous.InformationTextLine4 =
      informationTextLine4;
    entities.InterstateMiscellaneous.InformationTextLine5 =
      informationTextLine5;
    entities.InterstateMiscellaneous.Populated = true;
  }

  private bool ReadInterstateCase()
  {
    entities.InterstateCase.Populated = false;

    return Read("ReadInterstateCase",
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
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 0);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 1);
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

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
