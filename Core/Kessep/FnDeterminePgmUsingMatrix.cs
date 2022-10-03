// Program: FN_DETERMINE_PGM_USING_MATRIX, ID: 372118506, model: 746.
// Short name: SWE02284
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DETERMINE_PGM_USING_MATRIX.
/// </summary>
[Serializable]
public partial class FnDeterminePgmUsingMatrix: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DETERMINE_PGM_USING_MATRIX program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDeterminePgmUsingMatrix(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDeterminePgmUsingMatrix.
  /// </summary>
  public FnDeterminePgmUsingMatrix(IContext context, Import import,
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
    // *************************************************************
    //  Madhu Kumar       Changes for PRWORA     05/22/2000
    //  Madhu Kumar       JJA Enhancements       08/22/2000
    // *************************************************************
    // : Set hardcoded values for Program.
    local.HardcodedAf.SystemGeneratedIdentifier = 2;
    local.HardcodedAfi.SystemGeneratedIdentifier = 14;
    local.HardcodedFc.SystemGeneratedIdentifier = 15;
    local.HardcodedFci.SystemGeneratedIdentifier = 16;
    local.HardcodedNa.SystemGeneratedIdentifier = 12;
    local.HardcodedNai.SystemGeneratedIdentifier = 18;
    local.HardcodedNc.SystemGeneratedIdentifier = 13;
    local.HardcodedNf.SystemGeneratedIdentifier = 3;
    local.HardcodedMai.SystemGeneratedIdentifier = 17;
    export.KeyOnly.SystemGeneratedIdentifier =
      import.KeyOnly.SystemGeneratedIdentifier;

    // : If no initial Program has been identified, then establish a baseline 
    // now.
    if (export.KeyOnly.SystemGeneratedIdentifier == 0)
    {
      foreach(var item in ReadPersonProgramProgram3())
      {
        if (entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier == local
          .HardcodedFc.SystemGeneratedIdentifier)
        {
          export.KeyOnly.SystemGeneratedIdentifier =
            entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier;

          goto Test;
        }
        else if (export.KeyOnly.SystemGeneratedIdentifier == local
          .HardcodedAf.SystemGeneratedIdentifier)
        {
          if (entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier != local
            .HardcodedFc.SystemGeneratedIdentifier)
          {
            continue;
          }
        }
        else if (export.KeyOnly.SystemGeneratedIdentifier == local
          .HardcodedNc.SystemGeneratedIdentifier)
        {
          if (entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier != local
            .HardcodedAf.SystemGeneratedIdentifier && entities
            .ExistingKeyOnlyProgram.SystemGeneratedIdentifier != local
            .HardcodedFc.SystemGeneratedIdentifier)
          {
            continue;
          }
        }
        else if (export.KeyOnly.SystemGeneratedIdentifier == local
          .HardcodedNf.SystemGeneratedIdentifier)
        {
          if (entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier != local
            .HardcodedAf.SystemGeneratedIdentifier && entities
            .ExistingKeyOnlyProgram.SystemGeneratedIdentifier != local
            .HardcodedFc.SystemGeneratedIdentifier && entities
            .ExistingKeyOnlyProgram.SystemGeneratedIdentifier != local
            .HardcodedNc.SystemGeneratedIdentifier)
          {
            continue;
          }
        }

        export.KeyOnly.SystemGeneratedIdentifier =
          entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier;
      }

      if (export.KeyOnly.SystemGeneratedIdentifier != 0)
      {
        // **********************************************************
        // Means we have already determined a program.
        // **********************************************************
        goto Test;
      }

      // : Not a problem, determine the default program at the time of the due 
      // date.
      if (ReadPersonProgramProgram1())
      {
        export.KeyOnly.SystemGeneratedIdentifier =
          local.HardcodedNai.SystemGeneratedIdentifier;
      }
      else if (ReadPersonProgramProgram2())
      {
        export.KeyOnly.SystemGeneratedIdentifier =
          local.HardcodedNa.SystemGeneratedIdentifier;
      }
      else if (AsChar(import.Obligation.OrderTypeCode) == 'K')
      {
        if (import.DebtDue.YearMonth >= import.Collection.YearMonth)
        {
          export.KeyOnly.SystemGeneratedIdentifier =
            local.HardcodedNa.SystemGeneratedIdentifier;
        }
        else
        {
          export.KeyOnly.SystemGeneratedIdentifier =
            local.HardcodedAf.SystemGeneratedIdentifier;
        }
      }
      else if (import.DebtDue.YearMonth >= import.Collection.YearMonth)
      {
        export.KeyOnly.SystemGeneratedIdentifier =
          local.HardcodedNai.SystemGeneratedIdentifier;
      }
      else
      {
        export.KeyOnly.SystemGeneratedIdentifier =
          local.HardcodedAfi.SystemGeneratedIdentifier;
      }
    }

Test:

    foreach(var item in ReadPersonProgramProgram4())
    {
      if (export.KeyOnly.SystemGeneratedIdentifier == local
        .HardcodedNa.SystemGeneratedIdentifier)
      {
        if (entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier == local
          .HardcodedAf.SystemGeneratedIdentifier || entities
          .ExistingKeyOnlyProgram.SystemGeneratedIdentifier == local
          .HardcodedAfi.SystemGeneratedIdentifier || entities
          .ExistingKeyOnlyProgram.SystemGeneratedIdentifier == local
          .HardcodedFci.SystemGeneratedIdentifier || entities
          .ExistingKeyOnlyProgram.SystemGeneratedIdentifier == local
          .HardcodedNai.SystemGeneratedIdentifier)
        {
          export.KeyOnly.SystemGeneratedIdentifier =
            entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier;
        }
      }
      else if (export.KeyOnly.SystemGeneratedIdentifier == local
        .HardcodedNai.SystemGeneratedIdentifier)
      {
        if (entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier == local
          .HardcodedAf.SystemGeneratedIdentifier || entities
          .ExistingKeyOnlyProgram.SystemGeneratedIdentifier == local
          .HardcodedAfi.SystemGeneratedIdentifier || entities
          .ExistingKeyOnlyProgram.SystemGeneratedIdentifier == local
          .HardcodedFci.SystemGeneratedIdentifier || entities
          .ExistingKeyOnlyProgram.SystemGeneratedIdentifier == local
          .HardcodedNa.SystemGeneratedIdentifier)
        {
          export.KeyOnly.SystemGeneratedIdentifier =
            entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier;
        }
      }
    }
  }

  private bool ReadPersonProgramProgram1()
  {
    entities.Existing.Populated = false;
    entities.ExistingKeyOnlyProgram.Populated = false;

    return Read("ReadPersonProgramProgram1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.SupportedPerson.Number);
        db.SetNullableDate(
          command, "discontinueDate", import.DebtDue.Date.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate",
          import.DateOfEmancipation.Date.GetValueOrDefault());
        db.SetInt32(
          command, "programId", local.HardcodedMai.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Existing.CspNumber = db.GetString(reader, 0);
        entities.Existing.EffectiveDate = db.GetDate(reader, 1);
        entities.Existing.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.Existing.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingKeyOnlyProgram.Code = db.GetString(reader, 5);
        entities.ExistingKeyOnlyProgram.InterstateIndicator =
          db.GetString(reader, 6);
        entities.Existing.Populated = true;
        entities.ExistingKeyOnlyProgram.Populated = true;
      });
  }

  private bool ReadPersonProgramProgram2()
  {
    entities.Existing.Populated = false;
    entities.ExistingKeyOnlyProgram.Populated = false;

    return Read("ReadPersonProgramProgram2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.SupportedPerson.Number);
        db.SetDate(
          command, "effectiveDate1", import.DebtDue.Date.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate2",
          import.DateOfEmancipation.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Existing.CspNumber = db.GetString(reader, 0);
        entities.Existing.EffectiveDate = db.GetDate(reader, 1);
        entities.Existing.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.Existing.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingKeyOnlyProgram.Code = db.GetString(reader, 5);
        entities.ExistingKeyOnlyProgram.InterstateIndicator =
          db.GetString(reader, 6);
        entities.Existing.Populated = true;
        entities.ExistingKeyOnlyProgram.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPersonProgramProgram3()
  {
    entities.Existing.Populated = false;
    entities.ExistingKeyOnlyProgram.Populated = false;

    return ReadEach("ReadPersonProgramProgram3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.SupportedPerson.Number);
        db.SetDate(
          command, "effectiveDate1", import.DebtDue.Date.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate2",
          import.DateOfEmancipation.Date.GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedIdentifier1",
          local.HardcodedAf.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier2",
          local.HardcodedAfi.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier3",
          local.HardcodedFc.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier4",
          local.HardcodedFci.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier5",
          local.HardcodedNa.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier6",
          local.HardcodedNai.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier7",
          local.HardcodedNc.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier8",
          local.HardcodedNf.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Existing.CspNumber = db.GetString(reader, 0);
        entities.Existing.EffectiveDate = db.GetDate(reader, 1);
        entities.Existing.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.Existing.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingKeyOnlyProgram.Code = db.GetString(reader, 5);
        entities.ExistingKeyOnlyProgram.InterstateIndicator =
          db.GetString(reader, 6);
        entities.Existing.Populated = true;
        entities.ExistingKeyOnlyProgram.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonProgramProgram4()
  {
    entities.Existing.Populated = false;
    entities.ExistingKeyOnlyProgram.Populated = false;

    return ReadEach("ReadPersonProgramProgram4",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.SupportedPerson.Number);
        db.SetDate(
          command, "effectiveDate1", import.DebtDue.Date.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate2",
          import.Collection.Date.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate3",
          import.DateOfEmancipation.Date.GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedIdentifier1",
          local.HardcodedAf.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier2",
          local.HardcodedAfi.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier3",
          local.HardcodedFc.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier4",
          local.HardcodedFci.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier5",
          local.HardcodedNa.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier6",
          local.HardcodedNai.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier7",
          local.HardcodedNc.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier8",
          local.HardcodedNf.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Existing.CspNumber = db.GetString(reader, 0);
        entities.Existing.EffectiveDate = db.GetDate(reader, 1);
        entities.Existing.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.Existing.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingKeyOnlyProgram.Code = db.GetString(reader, 5);
        entities.ExistingKeyOnlyProgram.InterstateIndicator =
          db.GetString(reader, 6);
        entities.Existing.Populated = true;
        entities.ExistingKeyOnlyProgram.Populated = true;

        return true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
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
    /// A value of SupportedPerson.
    /// </summary>
    [JsonPropertyName("supportedPerson")]
    public CsePerson SupportedPerson
    {
      get => supportedPerson ??= new();
      set => supportedPerson = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of KeyOnly.
    /// </summary>
    [JsonPropertyName("keyOnly")]
    public Program KeyOnly
    {
      get => keyOnly ??= new();
      set => keyOnly = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public DateWorkArea Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of DebtDue.
    /// </summary>
    [JsonPropertyName("debtDue")]
    public DateWorkArea DebtDue
    {
      get => debtDue ??= new();
      set => debtDue = value;
    }

    /// <summary>
    /// A value of DelMe.
    /// </summary>
    [JsonPropertyName("delMe")]
    public CsePerson DelMe
    {
      get => delMe ??= new();
      set => delMe = value;
    }

    /// <summary>
    /// A value of DateOfEmancipation.
    /// </summary>
    [JsonPropertyName("dateOfEmancipation")]
    public DateWorkArea DateOfEmancipation
    {
      get => dateOfEmancipation ??= new();
      set => dateOfEmancipation = value;
    }

    private CsePerson supportedPerson;
    private Obligation obligation;
    private DebtDetail debtDetail;
    private Program keyOnly;
    private DateWorkArea collection;
    private DateWorkArea debtDue;
    private CsePerson delMe;
    private DateWorkArea dateOfEmancipation;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of KeyOnly.
    /// </summary>
    [JsonPropertyName("keyOnly")]
    public Program KeyOnly
    {
      get => keyOnly ??= new();
      set => keyOnly = value;
    }

    /// <summary>
    /// A value of DprProgram.
    /// </summary>
    [JsonPropertyName("dprProgram")]
    public DprProgram DprProgram
    {
      get => dprProgram ??= new();
      set => dprProgram = value;
    }

    private Program keyOnly;
    private DprProgram dprProgram;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of HardcodedAf.
    /// </summary>
    [JsonPropertyName("hardcodedAf")]
    public Program HardcodedAf
    {
      get => hardcodedAf ??= new();
      set => hardcodedAf = value;
    }

    /// <summary>
    /// A value of HardcodedNa.
    /// </summary>
    [JsonPropertyName("hardcodedNa")]
    public Program HardcodedNa
    {
      get => hardcodedNa ??= new();
      set => hardcodedNa = value;
    }

    /// <summary>
    /// A value of HardcodedAfi.
    /// </summary>
    [JsonPropertyName("hardcodedAfi")]
    public Program HardcodedAfi
    {
      get => hardcodedAfi ??= new();
      set => hardcodedAfi = value;
    }

    /// <summary>
    /// A value of HardcodedNai.
    /// </summary>
    [JsonPropertyName("hardcodedNai")]
    public Program HardcodedNai
    {
      get => hardcodedNai ??= new();
      set => hardcodedNai = value;
    }

    /// <summary>
    /// A value of HardcodedFc.
    /// </summary>
    [JsonPropertyName("hardcodedFc")]
    public Program HardcodedFc
    {
      get => hardcodedFc ??= new();
      set => hardcodedFc = value;
    }

    /// <summary>
    /// A value of HardcodedFci.
    /// </summary>
    [JsonPropertyName("hardcodedFci")]
    public Program HardcodedFci
    {
      get => hardcodedFci ??= new();
      set => hardcodedFci = value;
    }

    /// <summary>
    /// A value of HardcodedNc.
    /// </summary>
    [JsonPropertyName("hardcodedNc")]
    public Program HardcodedNc
    {
      get => hardcodedNc ??= new();
      set => hardcodedNc = value;
    }

    /// <summary>
    /// A value of HardcodedNf.
    /// </summary>
    [JsonPropertyName("hardcodedNf")]
    public Program HardcodedNf
    {
      get => hardcodedNf ??= new();
      set => hardcodedNf = value;
    }

    /// <summary>
    /// A value of HardcodedMai.
    /// </summary>
    [JsonPropertyName("hardcodedMai")]
    public Program HardcodedMai
    {
      get => hardcodedMai ??= new();
      set => hardcodedMai = value;
    }

    private Program hardcodedAf;
    private Program hardcodedNa;
    private Program hardcodedAfi;
    private Program hardcodedNai;
    private Program hardcodedFc;
    private Program hardcodedFci;
    private Program hardcodedNc;
    private Program hardcodedNf;
    private Program hardcodedMai;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public PersonProgram Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    /// <summary>
    /// A value of ExistingKeyOnlyProgram.
    /// </summary>
    [JsonPropertyName("existingKeyOnlyProgram")]
    public Program ExistingKeyOnlyProgram
    {
      get => existingKeyOnlyProgram ??= new();
      set => existingKeyOnlyProgram = value;
    }

    /// <summary>
    /// A value of ExistingKeyOnlyCsePerson.
    /// </summary>
    [JsonPropertyName("existingKeyOnlyCsePerson")]
    public CsePerson ExistingKeyOnlyCsePerson
    {
      get => existingKeyOnlyCsePerson ??= new();
      set => existingKeyOnlyCsePerson = value;
    }

    /// <summary>
    /// A value of DeleteMe.
    /// </summary>
    [JsonPropertyName("deleteMe")]
    public Program DeleteMe
    {
      get => deleteMe ??= new();
      set => deleteMe = value;
    }

    private PersonProgram existing;
    private Program existingKeyOnlyProgram;
    private CsePerson existingKeyOnlyCsePerson;
    private Program deleteMe;
  }
#endregion
}
