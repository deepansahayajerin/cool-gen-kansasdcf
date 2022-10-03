// Program: FN_B621_BUILD_PGM_HST_FOR_CO, ID: 371353975, model: 746.
// Short name: SWE00123
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B621_BUILD_PGM_HST_FOR_CO.
/// </summary>
[Serializable]
public partial class FnB621BuildPgmHstForCo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B621_BUILD_PGM_HST_FOR_CO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB621BuildPgmHstForCo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB621BuildPgmHstForCo.
  /// </summary>
  public FnB621BuildPgmHstForCo(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.PgmHist.Index = 0;
    export.PgmHist.Clear();

    foreach(var item in ReadCsePerson())
    {
      local.DateOfEmancipation.Date = import.MaximumDiscontinue.Date;

      foreach(var item1 in ReadCaseRole())
      {
        if (!Equal(entities.ExistingCaseRole.DateOfEmancipation,
          local.Null1.Date))
        {
          local.DateOfEmancipation.Date =
            entities.ExistingCaseRole.DateOfEmancipation;

          break;
        }
      }

      export.PgmHist.Update.PgmHistSuppPrsn.Number =
        entities.ExistingSupportedKeyOnly.Number;

      export.PgmHist.Item.PgmHistDtl.Index = 0;
      export.PgmHist.Item.PgmHistDtl.Clear();

      foreach(var item1 in ReadProgramPersonProgram())
      {
        MovePersonProgram(entities.ExistingPersonProgram,
          export.PgmHist.Update.PgmHistDtl.Update.PgmHistDtlPersonProgram);

        for(import.OfPgms.Index = 0; import.OfPgms.Index < import.OfPgms.Count; ++
          import.OfPgms.Index)
        {
          if (entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier == import
            .OfPgms.Item.OfPgms1.SystemGeneratedIdentifier)
          {
            export.PgmHist.Update.PgmHistDtl.Update.PgmHistDtlProgram.Assign(
              import.OfPgms.Item.OfPgms1);
            export.PgmHist.Item.PgmHistDtl.Next();

            goto ReadEach;
          }
        }

        ExitState = "PROGRAM_NF";
        export.PgmHist.Next();
        export.PgmHist.Item.PgmHistDtl.Next();

        return;

ReadEach:

        export.PgmHist.Item.PgmHistDtl.Next();
      }

      export.PgmHist.Next();
    }
  }

  private static void MovePersonProgram(PersonProgram source,
    PersonProgram target)
  {
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private IEnumerable<bool> ReadCaseRole()
  {
    entities.ExistingCaseRole.Populated = false;

    return ReadEach("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", entities.ExistingSupportedKeyOnly.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ExistingCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ExistingCaseRole.Type1 = db.GetString(reader, 2);
        entities.ExistingCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ExistingCaseRole.DateOfEmancipation =
          db.GetNullableDate(reader, 4);
        entities.ExistingCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingCaseRole.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePerson()
  {
    return ReadEach("ReadCsePerson",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", import.LegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        if (export.PgmHist.IsFull)
        {
          return false;
        }

        entities.ExistingSupportedKeyOnly.Number = db.GetString(reader, 0);
        entities.ExistingSupportedKeyOnly.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadProgramPersonProgram()
  {
    return ReadEach("ReadProgramPersonProgram",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", entities.ExistingSupportedKeyOnly.Number);
        db.SetDate(
          command, "effectiveDate",
          local.DateOfEmancipation.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.PgmHist.Item.PgmHistDtl.IsFull)
        {
          return false;
        }

        entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingPersonProgram.PrgGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingPersonProgram.CspNumber = db.GetString(reader, 1);
        entities.ExistingPersonProgram.EffectiveDate = db.GetDate(reader, 2);
        entities.ExistingPersonProgram.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingPersonProgram.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.ExistingKeyOnlyProgram.Populated = true;
        entities.ExistingPersonProgram.Populated = true;

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
    /// <summary>A OfPgmsGroup group.</summary>
    [Serializable]
    public class OfPgmsGroup
    {
      /// <summary>
      /// A value of OfPgms1.
      /// </summary>
      [JsonPropertyName("ofPgms1")]
      public Program OfPgms1
      {
        get => ofPgms1 ??= new();
        set => ofPgms1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 200;

      private Program ofPgms1;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of MaximumDiscontinue.
    /// </summary>
    [JsonPropertyName("maximumDiscontinue")]
    public DateWorkArea MaximumDiscontinue
    {
      get => maximumDiscontinue ??= new();
      set => maximumDiscontinue = value;
    }

    /// <summary>
    /// Gets a value of OfPgms.
    /// </summary>
    [JsonIgnore]
    public Array<OfPgmsGroup> OfPgms => ofPgms ??= new(OfPgmsGroup.Capacity);

    /// <summary>
    /// Gets a value of OfPgms for json serialization.
    /// </summary>
    [JsonPropertyName("ofPgms")]
    [Computed]
    public IList<OfPgmsGroup> OfPgms_Json
    {
      get => ofPgms;
      set => OfPgms.Assign(value);
    }

    private LegalAction legalAction;
    private DateWorkArea maximumDiscontinue;
    private Array<OfPgmsGroup> ofPgms;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A PgmHistGroup group.</summary>
    [Serializable]
    public class PgmHistGroup
    {
      /// <summary>
      /// A value of PgmHistSuppPrsn.
      /// </summary>
      [JsonPropertyName("pgmHistSuppPrsn")]
      public CsePerson PgmHistSuppPrsn
      {
        get => pgmHistSuppPrsn ??= new();
        set => pgmHistSuppPrsn = value;
      }

      /// <summary>
      /// Gets a value of PgmHistDtl.
      /// </summary>
      [JsonIgnore]
      public Array<PgmHistDtlGroup> PgmHistDtl => pgmHistDtl ??= new(
        PgmHistDtlGroup.Capacity);

      /// <summary>
      /// Gets a value of PgmHistDtl for json serialization.
      /// </summary>
      [JsonPropertyName("pgmHistDtl")]
      [Computed]
      public IList<PgmHistDtlGroup> PgmHistDtl_Json
      {
        get => pgmHistDtl;
        set => PgmHistDtl.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 30;

      private CsePerson pgmHistSuppPrsn;
      private Array<PgmHistDtlGroup> pgmHistDtl;
    }

    /// <summary>A PgmHistDtlGroup group.</summary>
    [Serializable]
    public class PgmHistDtlGroup
    {
      /// <summary>
      /// A value of PgmHistDtlProgram.
      /// </summary>
      [JsonPropertyName("pgmHistDtlProgram")]
      public Program PgmHistDtlProgram
      {
        get => pgmHistDtlProgram ??= new();
        set => pgmHistDtlProgram = value;
      }

      /// <summary>
      /// A value of PgmHistDtlPersonProgram.
      /// </summary>
      [JsonPropertyName("pgmHistDtlPersonProgram")]
      public PersonProgram PgmHistDtlPersonProgram
      {
        get => pgmHistDtlPersonProgram ??= new();
        set => pgmHistDtlPersonProgram = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 500;

      private Program pgmHistDtlProgram;
      private PersonProgram pgmHistDtlPersonProgram;
    }

    /// <summary>
    /// Gets a value of PgmHist.
    /// </summary>
    [JsonIgnore]
    public Array<PgmHistGroup> PgmHist =>
      pgmHist ??= new(PgmHistGroup.Capacity);

    /// <summary>
    /// Gets a value of PgmHist for json serialization.
    /// </summary>
    [JsonPropertyName("pgmHist")]
    [Computed]
    public IList<PgmHistGroup> PgmHist_Json
    {
      get => pgmHist;
      set => PgmHist.Assign(value);
    }

    private Array<PgmHistGroup> pgmHist;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of DateOfEmancipation.
    /// </summary>
    [JsonPropertyName("dateOfEmancipation")]
    public DateWorkArea DateOfEmancipation
    {
      get => dateOfEmancipation ??= new();
      set => dateOfEmancipation = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    private DateWorkArea dateOfEmancipation;
    private DateWorkArea null1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of ExistingObligorKeyOnly.
    /// </summary>
    [JsonPropertyName("existingObligorKeyOnly")]
    public CsePerson ExistingObligorKeyOnly
    {
      get => existingObligorKeyOnly ??= new();
      set => existingObligorKeyOnly = value;
    }

    /// <summary>
    /// A value of ExistingKeyOnlyObligor.
    /// </summary>
    [JsonPropertyName("existingKeyOnlyObligor")]
    public CsePersonAccount ExistingKeyOnlyObligor
    {
      get => existingKeyOnlyObligor ??= new();
      set => existingKeyOnlyObligor = value;
    }

    /// <summary>
    /// A value of ExistingKeyOnlyObligation.
    /// </summary>
    [JsonPropertyName("existingKeyOnlyObligation")]
    public Obligation ExistingKeyOnlyObligation
    {
      get => existingKeyOnlyObligation ??= new();
      set => existingKeyOnlyObligation = value;
    }

    /// <summary>
    /// A value of ExistingKeyOnlyDebt.
    /// </summary>
    [JsonPropertyName("existingKeyOnlyDebt")]
    public ObligationTransaction ExistingKeyOnlyDebt
    {
      get => existingKeyOnlyDebt ??= new();
      set => existingKeyOnlyDebt = value;
    }

    /// <summary>
    /// A value of ExistingSupportedKeyOnly.
    /// </summary>
    [JsonPropertyName("existingSupportedKeyOnly")]
    public CsePerson ExistingSupportedKeyOnly
    {
      get => existingSupportedKeyOnly ??= new();
      set => existingSupportedKeyOnly = value;
    }

    /// <summary>
    /// A value of ExistingKeyOnlySupported.
    /// </summary>
    [JsonPropertyName("existingKeyOnlySupported")]
    public CsePersonAccount ExistingKeyOnlySupported
    {
      get => existingKeyOnlySupported ??= new();
      set => existingKeyOnlySupported = value;
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
    /// A value of ExistingPersonProgram.
    /// </summary>
    [JsonPropertyName("existingPersonProgram")]
    public PersonProgram ExistingPersonProgram
    {
      get => existingPersonProgram ??= new();
      set => existingPersonProgram = value;
    }

    /// <summary>
    /// A value of ExistingCaseRole.
    /// </summary>
    [JsonPropertyName("existingCaseRole")]
    public CaseRole ExistingCaseRole
    {
      get => existingCaseRole ??= new();
      set => existingCaseRole = value;
    }

    /// <summary>
    /// A value of DelMe.
    /// </summary>
    [JsonPropertyName("delMe")]
    public Program DelMe
    {
      get => delMe ??= new();
      set => delMe = value;
    }

    private LegalAction legalAction;
    private CsePerson existingObligorKeyOnly;
    private CsePersonAccount existingKeyOnlyObligor;
    private Obligation existingKeyOnlyObligation;
    private ObligationTransaction existingKeyOnlyDebt;
    private CsePerson existingSupportedKeyOnly;
    private CsePersonAccount existingKeyOnlySupported;
    private Program existingKeyOnlyProgram;
    private PersonProgram existingPersonProgram;
    private CaseRole existingCaseRole;
    private Program delMe;
  }
#endregion
}
