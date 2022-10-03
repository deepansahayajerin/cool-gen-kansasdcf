// Program: SI_UPDATE_INTERSTATE_MISC, ID: 373441270, model: 746.
// Short name: SWE02759
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_UPDATE_INTERSTATE_MISC.
/// </summary>
[Serializable]
public partial class SiUpdateInterstateMisc: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_UPDATE_INTERSTATE_MISC program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiUpdateInterstateMisc(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiUpdateInterstateMisc.
  /// </summary>
  public SiUpdateInterstateMisc(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadInterstateMiscellaneous())
    {
      try
      {
        UpdateInterstateMiscellaneous();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SI0000_INTERSTATE_MISC_NU";

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
    else if (ReadInterstateCase())
    {
      ExitState = "SI0000_INTERSTATE_MISC_NF";
    }
    else
    {
      ExitState = "INTERSTATE_CASE_NF";
    }
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

  private bool ReadInterstateMiscellaneous()
  {
    entities.InterstateMiscellaneous.Populated = false;

    return Read("ReadInterstateMiscellaneous",
      (db, command) =>
      {
        db.SetInt64(
          command, "ccaTransSerNum", import.InterstateCase.TransSerialNumber);
        db.SetDate(
          command, "ccaTransactionDt",
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
        entities.InterstateMiscellaneous.CcaTransactionDt =
          db.GetDate(reader, 6);
        entities.InterstateMiscellaneous.InformationTextLine4 =
          db.GetNullableString(reader, 7);
        entities.InterstateMiscellaneous.InformationTextLine5 =
          db.GetNullableString(reader, 8);
        entities.InterstateMiscellaneous.Populated = true;
      });
  }

  private void UpdateInterstateMiscellaneous()
  {
    System.Diagnostics.Debug.Assert(entities.InterstateMiscellaneous.Populated);

    var statusChangeCode = import.InterstateMiscellaneous.StatusChangeCode;
    var newCaseId = import.InterstateMiscellaneous.NewCaseId ?? "";
    var informationTextLine1 =
      import.InterstateMiscellaneous.InformationTextLine1 ?? "";
    var informationTextLine2 =
      import.InterstateMiscellaneous.InformationTextLine2 ?? "";
    var informationTextLine3 =
      import.InterstateMiscellaneous.InformationTextLine3 ?? "";
    var informationTextLine4 =
      import.InterstateMiscellaneous.InformationTextLine4 ?? "";
    var informationTextLine5 =
      import.InterstateMiscellaneous.InformationTextLine5 ?? "";

    entities.InterstateMiscellaneous.Populated = false;
    Update("UpdateInterstateMiscellaneous",
      (db, command) =>
      {
        db.SetString(command, "statusChangeCode", statusChangeCode);
        db.SetNullableString(command, "newCaseId", newCaseId);
        db.SetNullableString(command, "infoText1", informationTextLine1);
        db.SetNullableString(command, "infoText2", informationTextLine2);
        db.SetNullableString(command, "infoText3", informationTextLine3);
        db.SetNullableString(command, "infoTextLine4", informationTextLine4);
        db.SetNullableString(command, "infoTextLine5", informationTextLine5);
        db.SetInt64(
          command, "ccaTransSerNum",
          entities.InterstateMiscellaneous.CcaTransSerNum);
        db.SetDate(
          command, "ccaTransactionDt",
          entities.InterstateMiscellaneous.CcaTransactionDt.
            GetValueOrDefault());
      });

    entities.InterstateMiscellaneous.StatusChangeCode = statusChangeCode;
    entities.InterstateMiscellaneous.NewCaseId = newCaseId;
    entities.InterstateMiscellaneous.InformationTextLine1 =
      informationTextLine1;
    entities.InterstateMiscellaneous.InformationTextLine2 =
      informationTextLine2;
    entities.InterstateMiscellaneous.InformationTextLine3 =
      informationTextLine3;
    entities.InterstateMiscellaneous.InformationTextLine4 =
      informationTextLine4;
    entities.InterstateMiscellaneous.InformationTextLine5 =
      informationTextLine5;
    entities.InterstateMiscellaneous.Populated = true;
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
