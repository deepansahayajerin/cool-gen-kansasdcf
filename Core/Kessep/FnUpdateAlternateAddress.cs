// Program: FN_UPDATE_ALTERNATE_ADDRESS, ID: 372095438, model: 746.
// Short name: SWE00001
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_UPDATE_ALTERNATE_ADDRESS.
/// </para>
/// <para>
/// obligations can have an alternate mailing address assigned to them, either 
/// by Legal or by Finance.  This assignment is made by populating the
/// relationship  has_alternate_billing_locn_as  between CSE_Person and
/// Obligation.  If that relationship is already valued, this will DISASSOCIATE
/// it.  If the change is that there will be no alternate address, that's it.
/// If a new one has been designated, then
/// this will ASSOCIATE the Obligation to the new CSE_Person.
/// </para>
/// </summary>
[Serializable]
public partial class FnUpdateAlternateAddress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_UPDATE_ALTERNATE_ADDRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnUpdateAlternateAddress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnUpdateAlternateAddress.
  /// </summary>
  public FnUpdateAlternateAddress(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // =================================================
    // 1/4/99 - bud adams  -  creation
    //    Not a new funtion, but this currently exists as CRUD actions
    //    in all the debt screens - and maintaining them all individually
    //    is a nuisance and error prone.
    // =================================================
    ExitState = "ACO_NN0000_ALL_OK";

    if (ReadCsePersonCsePersonAccount())
    {
      // ***---  Cursor Only, because the Associate / Disassociate
      // ***---  actions will be Updating the Obligation table, and all
      // ***---  IEF updates are done via CURSOR.
      if (!ReadObligation())
      {
        ExitState = "FN0000_OBLIGATION_NF_RB";
      }
    }
    else
    {
      ExitState = "FN0000_CSE_PERSON_ACCOUNT_NF_RB";
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ***---  This is an optional relationship.  If one already exists,
    // ***---  just Disassociate the entities.
    if (ReadCsePerson2())
    {
      DisassociateCsePerson();
    }

    // ***---  The change may be to not have any alternate billing
    // ***---  address.  So, if the IF test passes, then we're done.
    // ***---  If it fails, that means we need to Associate the Obligation
    // ***---  to the new billing address.
    if (IsEmpty(import.AlternateBillingAddress.Number))
    {
    }
    else if (ReadCsePerson1())
    {
      AssociateCsePerson();
    }
    else
    {
      ExitState = "FN0000_ALTERNATE_ADDR_NF_RB";
    }
  }

  private void AssociateCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);

    var cspPNumber = entities.AlternateBillingAddress.Number;

    entities.Obligation.Populated = false;
    Update("AssociateCsePerson",
      (db, command) =>
      {
        db.SetNullableString(command, "cspPNumber", cspPNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetInt32(
          command, "obId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dtyGeneratedId", entities.Obligation.DtyGeneratedId);
      });

    entities.Obligation.CspPNumber = cspPNumber;
    entities.Obligation.Populated = true;
  }

  private void DisassociateCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);

    var cpaType = entities.Obligation.CpaType;
    var cspNumber = entities.Obligation.CspNumber;

    entities.Obligation.Populated = false;

    bool exists;

    Update("DisassociateCsePerson#1",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNumber1", cspNumber);
        db.SetInt32(
          command, "obId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dtyGeneratedId", entities.Obligation.DtyGeneratedId);
      });

    exists = Read("DisassociateCsePerson#2",
      (db, command) =>
      {
        db.SetString(command, "cpaType2", cpaType);
        db.SetString(command, "cspNumber2", cspNumber);
      },
      null);

    if (!exists)
    {
      Update("DisassociateCsePerson#3",
        (db, command) =>
        {
          db.SetString(command, "cpaType2", cpaType);
          db.SetString(command, "cspNumber2", cspNumber);
        });
    }

    entities.Obligation.CspPNumber = null;
    entities.Obligation.Populated = true;
  }

  private bool ReadCsePerson1()
  {
    entities.AlternateBillingAddress.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", import.AlternateBillingAddress.Number);
      },
      (db, reader) =>
      {
        entities.AlternateBillingAddress.Number = db.GetString(reader, 0);
        entities.AlternateBillingAddress.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.AlternateBillingAddress.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.Obligation.CspPNumber ?? "");
      },
      (db, reader) =>
      {
        entities.AlternateBillingAddress.Number = db.GetString(reader, 0);
        entities.AlternateBillingAddress.Populated = true;
      });
  }

  private bool ReadCsePersonCsePersonAccount()
  {
    entities.Obligor.Populated = false;
    entities.CsePersonAccount.Populated = false;

    return Read("ReadCsePersonCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Obligor.Number);
        db.SetString(command, "type", import.HcCpaObligor.Type1);
      },
      (db, reader) =>
      {
        entities.Obligor.Number = db.GetString(reader, 0);
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 0);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 1);
        entities.Obligor.Populated = true;
        entities.CsePersonAccount.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.CsePersonAccount.Type1);
      });
  }

  private bool ReadObligation()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);
    entities.Obligation.Populated = false;

    return Read("ReadObligation",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.CsePersonAccount.Type1);
        db.SetString(command, "cspNumber", entities.CsePersonAccount.CspNumber);
        db.
          SetInt32(command, "obId", import.Obligation.SystemGeneratedIdentifier);
          
        db.SetInt32(
          command, "dtyGeneratedId",
          import.ObligationType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.CspPNumber = db.GetNullableString(reader, 4);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
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
    /// A value of AlternateBillingAddress.
    /// </summary>
    [JsonPropertyName("alternateBillingAddress")]
    public CsePersonsWorkSet AlternateBillingAddress
    {
      get => alternateBillingAddress ??= new();
      set => alternateBillingAddress = value;
    }

    /// <summary>
    /// A value of HcCpaObligor.
    /// </summary>
    [JsonPropertyName("hcCpaObligor")]
    public CsePersonAccount HcCpaObligor
    {
      get => hcCpaObligor ??= new();
      set => hcCpaObligor = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    private CsePersonsWorkSet alternateBillingAddress;
    private CsePersonAccount hcCpaObligor;
    private ObligationType obligationType;
    private Obligation obligation;
    private CsePerson obligor;
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
    /// A value of AlternateBillingAddress.
    /// </summary>
    [JsonPropertyName("alternateBillingAddress")]
    public CsePerson AlternateBillingAddress
    {
      get => alternateBillingAddress ??= new();
      set => alternateBillingAddress = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    private CsePerson alternateBillingAddress;
    private ObligationType obligationType;
    private Obligation obligation;
    private CsePerson obligor;
    private CsePersonAccount csePersonAccount;
  }
#endregion
}
