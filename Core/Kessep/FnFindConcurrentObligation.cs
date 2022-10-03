// Program: FN_FIND_CONCURRENT_OBLIGATION, ID: 373388263, model: 746.
// Short name: SWE00494
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_FIND_CONCURRENT_OBLIGATION.
/// </summary>
[Serializable]
public partial class FnFindConcurrentObligation: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_FIND_CONCURRENT_OBLIGATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnFindConcurrentObligation(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnFindConcurrentObligation.
  /// </summary>
  public FnFindConcurrentObligation(IContext context, Import import,
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
    // ----------------------------------------------------------------
    // Date     Developer Name   Request#  Description
    // 1/03/01  Mark Ashworth   WR10504   Initial Development
    // ----------------------------------------------------------------
    if (ReadObligation())
    {
      export.FoundConcurrentObl.Flag = "N";

      // ----------------------------------------------------------------
      // Joined with obligation type so we can export it.
      // ----------------------------------------------------------------
      if (ReadObligationObligationType2())
      {
        export.FoundConcurrentObl.Flag = "Y";
      }
      else if (ReadObligationObligationType1())
      {
        export.FoundConcurrentObl.Flag = "Y";
      }
      else
      {
        ExitState = "FN0000_CONCURRENT_OBLIGATN_NF_RB";

        return;
      }

      if (AsChar(export.FoundConcurrentObl.Flag) == 'Y')
      {
        if (ReadCsePersonAccountCsePerson())
        {
          export.ConcurrentObligation.SystemGeneratedIdentifier =
            entities.Concurrent.SystemGeneratedIdentifier;
          export.ConcurrentCsePerson.Number = entities.CsePerson.Number;
          export.ConcurrentCsePersonAccount.Type1 =
            entities.CsePersonAccount.Type1;
          export.ConcurrentObligationType.SystemGeneratedIdentifier =
            entities.ObligationType.SystemGeneratedIdentifier;
        }
        else
        {
          ExitState = "CSE_PERSON_NF_RB";
        }
      }
    }
    else
    {
      ExitState = "FN0000_OBLIGATION_NF_RB";
    }
  }

  private bool ReadCsePersonAccountCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.Concurrent.Populated);
    entities.CsePersonAccount.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadCsePersonAccountCsePerson",
      (db, command) =>
      {
        db.SetString(command, "type", entities.Concurrent.CpaType);
        db.SetString(command, "cspNumber", entities.Concurrent.CspNumber);
      },
      (db, reader) =>
      {
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 1);
        entities.CsePersonAccount.Populated = true;
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadObligation()
  {
    entities.Existing.Populated = false;

    return Read("ReadObligation",
      (db, command) =>
      {
        db.
          SetInt32(command, "obId", import.Obligation.SystemGeneratedIdentifier);
          
        db.SetString(command, "cpaType", import.CsePersonAccount.Type1);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetInt32(
          command, "dtyGeneratedId",
          import.ObligationType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Existing.CpaType = db.GetString(reader, 0);
        entities.Existing.CspNumber = db.GetString(reader, 1);
        entities.Existing.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Existing.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Existing.Populated = true;
      });
  }

  private bool ReadObligationObligationType1()
  {
    System.Diagnostics.Debug.Assert(entities.Existing.Populated);
    entities.Concurrent.Populated = false;
    entities.ObligationType.Populated = false;

    return Read("ReadObligationObligationType1",
      (db, command) =>
      {
        db.SetInt32(command, "otySecondId", entities.Existing.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Existing.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Existing.CspNumber);
        db.SetString(command, "cpaType", entities.Existing.CpaType);
      },
      (db, reader) =>
      {
        entities.Concurrent.CpaType = db.GetString(reader, 0);
        entities.Concurrent.CspNumber = db.GetString(reader, 1);
        entities.Concurrent.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Concurrent.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.Concurrent.Populated = true;
        entities.ObligationType.Populated = true;
      });
  }

  private bool ReadObligationObligationType2()
  {
    System.Diagnostics.Debug.Assert(entities.Existing.Populated);
    entities.Concurrent.Populated = false;
    entities.ObligationType.Populated = false;

    return Read("ReadObligationObligationType2",
      (db, command) =>
      {
        db.SetInt32(command, "otyFirstId", entities.Existing.DtyGeneratedId);
        db.SetInt32(
          command, "obgFGeneratedId",
          entities.Existing.SystemGeneratedIdentifier);
        db.SetString(command, "cspFNumber", entities.Existing.CspNumber);
        db.SetString(command, "cpaFType", entities.Existing.CpaType);
      },
      (db, reader) =>
      {
        entities.Concurrent.CpaType = db.GetString(reader, 0);
        entities.Concurrent.CspNumber = db.GetString(reader, 1);
        entities.Concurrent.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Concurrent.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.Concurrent.Populated = true;
        entities.ObligationType.Populated = true;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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

    private Obligation obligation;
    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
    private ObligationType obligationType;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of FoundConcurrentObl.
    /// </summary>
    [JsonPropertyName("foundConcurrentObl")]
    public Common FoundConcurrentObl
    {
      get => foundConcurrentObl ??= new();
      set => foundConcurrentObl = value;
    }

    /// <summary>
    /// A value of ConcurrentObligation.
    /// </summary>
    [JsonPropertyName("concurrentObligation")]
    public Obligation ConcurrentObligation
    {
      get => concurrentObligation ??= new();
      set => concurrentObligation = value;
    }

    /// <summary>
    /// A value of ConcurrentCsePersonAccount.
    /// </summary>
    [JsonPropertyName("concurrentCsePersonAccount")]
    public CsePersonAccount ConcurrentCsePersonAccount
    {
      get => concurrentCsePersonAccount ??= new();
      set => concurrentCsePersonAccount = value;
    }

    /// <summary>
    /// A value of ConcurrentCsePerson.
    /// </summary>
    [JsonPropertyName("concurrentCsePerson")]
    public CsePerson ConcurrentCsePerson
    {
      get => concurrentCsePerson ??= new();
      set => concurrentCsePerson = value;
    }

    /// <summary>
    /// A value of ConcurrentObligationType.
    /// </summary>
    [JsonPropertyName("concurrentObligationType")]
    public ObligationType ConcurrentObligationType
    {
      get => concurrentObligationType ??= new();
      set => concurrentObligationType = value;
    }

    private Common foundConcurrentObl;
    private Obligation concurrentObligation;
    private CsePersonAccount concurrentCsePersonAccount;
    private CsePerson concurrentCsePerson;
    private ObligationType concurrentObligationType;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    /// <summary>
    /// A value of Concurrent.
    /// </summary>
    [JsonPropertyName("concurrent")]
    public Obligation Concurrent
    {
      get => concurrent ??= new();
      set => concurrent = value;
    }

    /// <summary>
    /// A value of ObligationRln.
    /// </summary>
    [JsonPropertyName("obligationRln")]
    public ObligationRln ObligationRln
    {
      get => obligationRln ??= new();
      set => obligationRln = value;
    }

    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public Obligation Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    private CsePersonAccount csePersonAccount;
    private Obligation concurrent;
    private ObligationRln obligationRln;
    private Obligation existing;
    private CsePersonAccount obligor;
    private CsePerson csePerson;
    private ObligationType obligationType;
  }
#endregion
}
