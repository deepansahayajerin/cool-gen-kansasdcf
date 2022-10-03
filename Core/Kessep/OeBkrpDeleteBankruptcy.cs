// Program: OE_BKRP_DELETE_BANKRUPTCY, ID: 372034247, model: 746.
// Short name: SWE00865
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_BKRP_DELETE_BANKRUPTCY.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This action block deletes an occurrence of BANKRUPTCY.
/// </para>
/// </summary>
[Serializable]
public partial class OeBkrpDeleteBankruptcy: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_BKRP_DELETE_BANKRUPTCY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeBkrpDeleteBankruptcy(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeBkrpDeleteBankruptcy.
  /// </summary>
  public OeBkrpDeleteBankruptcy(IContext context, Import import, Export export):
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
    //        M A I N T E N A N C E   L O G
    //  Date    Developer       request #    Description
    // 12/17/98 R.Jean       Remove extraneous views
    // ---------------------------------------------
    if (ReadBankruptcy())
    {
      DeleteBankruptcy();
    }
    else
    {
      ExitState = "BANKRUPTCY_NF";
    }
  }

  private void DeleteBankruptcy()
  {
    Update("DeleteBankruptcy",
      (db, command) =>
      {
        db.
          SetString(command, "cspNumber", entities.ExistingBankruptcy.CspNumber);
          
        db.SetInt32(
          command, "identifier", entities.ExistingBankruptcy.Identifier);
      });
  }

  private bool ReadBankruptcy()
  {
    entities.ExistingBankruptcy.Populated = false;

    return Read("ReadBankruptcy",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetInt32(command, "identifier", import.Bankruptcy.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingBankruptcy.CspNumber = db.GetString(reader, 0);
        entities.ExistingBankruptcy.Identifier = db.GetInt32(reader, 1);
        entities.ExistingBankruptcy.BankruptcyDischargeDate =
          db.GetNullableDate(reader, 2);
        entities.ExistingBankruptcy.Populated = true;
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
    /// A value of Bankruptcy.
    /// </summary>
    [JsonPropertyName("bankruptcy")]
    public Bankruptcy Bankruptcy
    {
      get => bankruptcy ??= new();
      set => bankruptcy = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private Bankruptcy bankruptcy;
    private CsePerson csePerson;
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
    /// A value of ExistingBankruptcy.
    /// </summary>
    [JsonPropertyName("existingBankruptcy")]
    public Bankruptcy ExistingBankruptcy
    {
      get => existingBankruptcy ??= new();
      set => existingBankruptcy = value;
    }

    /// <summary>
    /// A value of ExistingCsePerson.
    /// </summary>
    [JsonPropertyName("existingCsePerson")]
    public CsePerson ExistingCsePerson
    {
      get => existingCsePerson ??= new();
      set => existingCsePerson = value;
    }

    private Bankruptcy existingBankruptcy;
    private CsePerson existingCsePerson;
  }
#endregion
}
