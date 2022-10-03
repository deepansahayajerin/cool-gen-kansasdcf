// Program: OE_GTSC_ASSOC_LEG_ACT_WITH_GTEST, ID: 371797740, model: 746.
// Short name: SWE00916
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_GTSC_ASSOC_LEG_ACT_WITH_GTEST.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This action block associates / disassociates the legal action from 
/// genetic_test
/// </para>
/// </summary>
[Serializable]
public partial class OeGtscAssocLegActWithGtest: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_GTSC_ASSOC_LEG_ACT_WITH_GTEST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeGtscAssocLegActWithGtest(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeGtscAssocLegActWithGtest.
  /// </summary>
  public OeGtscAssocLegActWithGtest(IContext context, Import import,
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
    // ******** SPECIAL MAINTENANCE ********************
    // AUTHOR  DATE  		DESCRIPTION
    // R. Jean	07/09/99	Singleton reads changed to select only
    // ******* END MAINTENANCE LOG ****************
    if (import.LegalAction.Identifier != 0)
    {
      if (!ReadLegalAction1())
      {
        ExitState = "LEGAL_ACTION_NF";

        return;
      }
    }

    if (!ReadGeneticTest())
    {
      ExitState = "GENETIC_TEST_NF";

      return;
    }

    if (ReadLegalAction2())
    {
      if (entities.ExistingNewPatEstab.Identifier != entities
        .ExistingPrevPatEstab.Identifier)
      {
        if (import.LegalAction.Identifier == 0)
        {
          DisassociateGeneticTest();
        }
        else
        {
          AssociateGeneticTest();
        }
      }
    }
    else if (import.LegalAction.Identifier != 0)
    {
      AssociateGeneticTest();
    }
  }

  private void AssociateGeneticTest()
  {
    var lgaIdentifier = entities.ExistingNewPatEstab.Identifier;

    entities.Scheduled.Populated = false;
    Update("AssociateGeneticTest",
      (db, command) =>
      {
        db.SetNullableInt32(command, "lgaIdentifier", lgaIdentifier);
        db.SetInt32(command, "testNumber", entities.Scheduled.TestNumber);
      });

    entities.Scheduled.LgaIdentifier = lgaIdentifier;
    entities.Scheduled.Populated = true;
  }

  private void DisassociateGeneticTest()
  {
    entities.Scheduled.Populated = false;
    Update("DisassociateGeneticTest",
      (db, command) =>
      {
        db.SetInt32(command, "testNumber", entities.Scheduled.TestNumber);
      });

    entities.Scheduled.LgaIdentifier = null;
    entities.Scheduled.Populated = true;
  }

  private bool ReadGeneticTest()
  {
    entities.Scheduled.Populated = false;

    return Read("ReadGeneticTest",
      (db, command) =>
      {
        db.SetInt32(command, "testNumber", import.Scheduled.TestNumber);
      },
      (db, reader) =>
      {
        entities.Scheduled.TestNumber = db.GetInt32(reader, 0);
        entities.Scheduled.LabCaseNo = db.GetNullableString(reader, 1);
        entities.Scheduled.TestType = db.GetNullableString(reader, 2);
        entities.Scheduled.ActualTestDate = db.GetNullableDate(reader, 3);
        entities.Scheduled.TestResultReceivedDate =
          db.GetNullableDate(reader, 4);
        entities.Scheduled.PaternityExclusionInd =
          db.GetNullableString(reader, 5);
        entities.Scheduled.PaternityProbability =
          db.GetNullableDecimal(reader, 6);
        entities.Scheduled.NoticeOfContestReceivedInd =
          db.GetNullableString(reader, 7);
        entities.Scheduled.StartDateOfContest = db.GetNullableDate(reader, 8);
        entities.Scheduled.EndDateOfContest = db.GetNullableDate(reader, 9);
        entities.Scheduled.CreatedBy = db.GetString(reader, 10);
        entities.Scheduled.CreatedTimestamp = db.GetDateTime(reader, 11);
        entities.Scheduled.LastUpdatedBy = db.GetString(reader, 12);
        entities.Scheduled.LastUpdatedTimestamp = db.GetDateTime(reader, 13);
        entities.Scheduled.LgaIdentifier = db.GetNullableInt32(reader, 14);
        entities.Scheduled.Populated = true;
      });
  }

  private bool ReadLegalAction1()
  {
    entities.ExistingNewPatEstab.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingNewPatEstab.Identifier = db.GetInt32(reader, 0);
        entities.ExistingNewPatEstab.CourtCaseNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingNewPatEstab.Populated = true;
      });
  }

  private bool ReadLegalAction2()
  {
    System.Diagnostics.Debug.Assert(entities.Scheduled.Populated);
    entities.ExistingPrevPatEstab.Populated = false;

    return Read("ReadLegalAction2",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.Scheduled.LgaIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingPrevPatEstab.Identifier = db.GetInt32(reader, 0);
        entities.ExistingPrevPatEstab.CourtCaseNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingPrevPatEstab.Populated = true;
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
    /// A value of Scheduled.
    /// </summary>
    [JsonPropertyName("scheduled")]
    public GeneticTest Scheduled
    {
      get => scheduled ??= new();
      set => scheduled = value;
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

    private GeneticTest scheduled;
    private LegalAction legalAction;
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
    /// A value of ExistingNewPatEstab.
    /// </summary>
    [JsonPropertyName("existingNewPatEstab")]
    public LegalAction ExistingNewPatEstab
    {
      get => existingNewPatEstab ??= new();
      set => existingNewPatEstab = value;
    }

    /// <summary>
    /// A value of ExistingPrevPatEstab.
    /// </summary>
    [JsonPropertyName("existingPrevPatEstab")]
    public LegalAction ExistingPrevPatEstab
    {
      get => existingPrevPatEstab ??= new();
      set => existingPrevPatEstab = value;
    }

    /// <summary>
    /// A value of Scheduled.
    /// </summary>
    [JsonPropertyName("scheduled")]
    public GeneticTest Scheduled
    {
      get => scheduled ??= new();
      set => scheduled = value;
    }

    private LegalAction existingNewPatEstab;
    private LegalAction existingPrevPatEstab;
    private GeneticTest scheduled;
  }
#endregion
}
