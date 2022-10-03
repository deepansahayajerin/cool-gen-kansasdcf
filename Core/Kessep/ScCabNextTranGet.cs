// Program: SC_CAB_NEXT_TRAN_GET, ID: 371425723, model: 746.
// Short name: SWE01076
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SC_CAB_NEXT_TRAN_GET.
/// </para>
/// <para>
/// RESP: SECURITY Obtain all information to populate screen from entity 
/// NEXT_TRAN_INFO
/// </para>
/// </summary>
[Serializable]
public partial class ScCabNextTranGet: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SC_CAB_NEXT_TRAN_GET program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ScCabNextTranGet(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ScCabNextTranGet.
  /// </summary>
  public ScCabNextTranGet(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadNextTranInfo())
    {
      export.NextTranInfo.Assign(entities.ExistingNextTranInfo);
    }
    else
    {
      ExitState = "SC0000_NEXT_TRAN_INFO_NF";
    }
  }

  private bool ReadNextTranInfo()
  {
    entities.ExistingNextTranInfo.Populated = false;

    return Read("ReadNextTranInfo",
      (db, command) =>
      {
        db.SetString(command, "userId", global.UserId);
      },
      (db, reader) =>
      {
        entities.ExistingNextTranInfo.LastTran =
          db.GetNullableString(reader, 0);
        entities.ExistingNextTranInfo.LegalActionIdentifier =
          db.GetNullableInt32(reader, 1);
        entities.ExistingNextTranInfo.CourtCaseNumber =
          db.GetNullableString(reader, 2);
        entities.ExistingNextTranInfo.CaseNumber =
          db.GetNullableString(reader, 3);
        entities.ExistingNextTranInfo.CsePersonNumber =
          db.GetNullableString(reader, 4);
        entities.ExistingNextTranInfo.CsePersonNumberAp =
          db.GetNullableString(reader, 5);
        entities.ExistingNextTranInfo.CsePersonNumberObligee =
          db.GetNullableString(reader, 6);
        entities.ExistingNextTranInfo.CsePersonNumberObligor =
          db.GetNullableString(reader, 7);
        entities.ExistingNextTranInfo.CourtOrderNumber =
          db.GetNullableString(reader, 8);
        entities.ExistingNextTranInfo.ObligationId =
          db.GetNullableInt32(reader, 9);
        entities.ExistingNextTranInfo.StandardCrtOrdNumber =
          db.GetNullableString(reader, 10);
        entities.ExistingNextTranInfo.InfrastructureId =
          db.GetNullableInt32(reader, 11);
        entities.ExistingNextTranInfo.MiscText1 =
          db.GetNullableString(reader, 12);
        entities.ExistingNextTranInfo.MiscText2 =
          db.GetNullableString(reader, 13);
        entities.ExistingNextTranInfo.MiscNum1 =
          db.GetNullableInt64(reader, 14);
        entities.ExistingNextTranInfo.MiscNum2 =
          db.GetNullableInt64(reader, 15);
        entities.ExistingNextTranInfo.MiscNum1V2 =
          db.GetNullableDecimal(reader, 16);
        entities.ExistingNextTranInfo.MiscNum2V2 =
          db.GetNullableDecimal(reader, 17);
        entities.ExistingNextTranInfo.OspId = db.GetInt32(reader, 18);
        entities.ExistingNextTranInfo.Populated = true;
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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
    }

    private NextTranInfo nextTranInfo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingNextTranInfo.
    /// </summary>
    [JsonPropertyName("existingNextTranInfo")]
    public NextTranInfo ExistingNextTranInfo
    {
      get => existingNextTranInfo ??= new();
      set => existingNextTranInfo = value;
    }

    /// <summary>
    /// A value of ExistingServiceProvider.
    /// </summary>
    [JsonPropertyName("existingServiceProvider")]
    public ServiceProvider ExistingServiceProvider
    {
      get => existingServiceProvider ??= new();
      set => existingServiceProvider = value;
    }

    private NextTranInfo existingNextTranInfo;
    private ServiceProvider existingServiceProvider;
  }
#endregion
}
