// Program: LE_POST_READ_POSITION_STATEMENT, ID: 372603746, model: 746.
// Short name: SWE00810
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_POST_READ_POSITION_STATEMENT.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block reads all Position Statements for an Administrative 
/// Appeal.
/// </para>
/// </summary>
[Serializable]
public partial class LePostReadPositionStatement: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_POST_READ_POSITION_STATEMENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LePostReadPositionStatement(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LePostReadPositionStatement.
  /// </summary>
  public LePostReadPositionStatement(IContext context, Import import,
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
    MoveAdministrativeAppeal1(import.AdministrativeAppeal,
      export.AdministrativeAppeal);
    MoveCsePersonsWorkSet(import.CsePersonsWorkSet, export.CsePersonsWorkSet);

    if (ReadCsePerson1())
    {
      local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
      UseSiReadCsePerson();

      switch(AsChar(local.ReturnCode.Flag))
      {
        case '0':
          // *********************************************
          // Read was successful
          // *********************************************
          break;
        case '1':
          ExitState = "ADABAS_READ_UNSUCCESSFUL";

          return;
        case '2':
          ExitState = "ADABAS_UPDATE_UNSUCCESSFUL";

          return;
        default:
          break;
      }
    }
    else
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    foreach(var item in ReadAdministrativeAppeal2())
    {
      ++local.Common.Count;

      if (local.Common.Count > 1)
      {
        ExitState = "LE0000_MULTIPLE_APPEALS_EXIT";

        return;
      }

      MoveAdministrativeAppeal2(entities.AdministrativeAppeal,
        export.AdministrativeAppeal);
    }

    if (local.Common.Count < 1)
    {
      ExitState = "LE0000_ADMINISTRATIVE_APPEAL_NF";

      return;
    }

    if (ReadAdministrativeAppeal1())
    {
      MoveAdministrativeAppeal2(entities.AdministrativeAppeal,
        export.AdministrativeAppeal);

      if (ReadAdministrativeAction1())
      {
        MoveAdministrativeAction(entities.AdministrativeAction,
          export.AdministrativeAction);
      }
      else if (ReadAdministrativeActCertification())
      {
        if (ReadAdministrativeAction2())
        {
          MoveAdministrativeAction(entities.AdministrativeAction,
            export.AdministrativeAction);
        }
        else
        {
          ExitState = "ADMINISTRATIVE_ACTION_NF";
        }
      }

      export.Export1.Index = 0;
      export.Export1.Clear();

      foreach(var item in ReadPositionStatement())
      {
        MovePositionStatement(entities.PositionStatement,
          export.Export1.Update.PositionStatement);
        export.Export1.Next();
      }

      if (!ReadCsePerson2())
      {
        ExitState = "CSE_PERSON_NF";
      }
    }
    else
    {
      ExitState = "LE0000_ADMINISTRATIVE_APPEAL_NF";
    }
  }

  private static void MoveAdministrativeAction(AdministrativeAction source,
    AdministrativeAction target)
  {
    target.Type1 = source.Type1;
    target.Description = source.Description;
  }

  private static void MoveAdministrativeAppeal1(AdministrativeAppeal source,
    AdministrativeAppeal target)
  {
    target.Identifier = source.Identifier;
    target.Number = source.Number;
  }

  private static void MoveAdministrativeAppeal2(AdministrativeAppeal source,
    AdministrativeAppeal target)
  {
    target.Identifier = source.Identifier;
    target.Number = source.Number;
    target.Type1 = source.Type1;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MovePositionStatement(PositionStatement source,
    PositionStatement target)
  {
    target.Number = source.Number;
    target.Explanation = source.Explanation;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.ReturnCode.Flag = useExport.Ae.Flag;
    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private bool ReadAdministrativeActCertification()
  {
    System.Diagnostics.Debug.Assert(entities.AdministrativeAppeal.Populated);
    entities.AdministrativeActCertification.Populated = false;

    return Read("ReadAdministrativeActCertification",
      (db, command) =>
      {
        db.SetString(
          command, "tanfCode", entities.AdministrativeAppeal.AacTanfCode ?? ""
          );
        db.SetDate(
          command, "takenDt",
          entities.AdministrativeAppeal.AacRTakenDate.GetValueOrDefault());
        db.SetString(
          command, "cpaType", entities.AdministrativeAppeal.CpaRType ?? "");
        db.SetString(
          command, "type", entities.AdministrativeAppeal.AacRType ?? "");
        db.SetString(
          command, "cspNumber", entities.AdministrativeAppeal.CspRNumber ?? ""
          );
      },
      (db, reader) =>
      {
        entities.AdministrativeActCertification.CpaType =
          db.GetString(reader, 0);
        entities.AdministrativeActCertification.CspNumber =
          db.GetString(reader, 1);
        entities.AdministrativeActCertification.Type1 = db.GetString(reader, 2);
        entities.AdministrativeActCertification.TakenDate =
          db.GetDate(reader, 3);
        entities.AdministrativeActCertification.TanfCode =
          db.GetString(reader, 4);
        entities.AdministrativeActCertification.Populated = true;
        CheckValid<AdministrativeActCertification>("CpaType",
          entities.AdministrativeActCertification.CpaType);
        CheckValid<AdministrativeActCertification>("Type1",
          entities.AdministrativeActCertification.Type1);
      });
  }

  private bool ReadAdministrativeAction1()
  {
    System.Diagnostics.Debug.Assert(entities.AdministrativeAppeal.Populated);
    entities.AdministrativeAction.Populated = false;

    return Read("ReadAdministrativeAction1",
      (db, command) =>
      {
        db.SetString(
          command, "type", entities.AdministrativeAppeal.AatType ?? "");
      },
      (db, reader) =>
      {
        entities.AdministrativeAction.Type1 = db.GetString(reader, 0);
        entities.AdministrativeAction.Description = db.GetString(reader, 1);
        entities.AdministrativeAction.Populated = true;
      });
  }

  private bool ReadAdministrativeAction2()
  {
    entities.AdministrativeAction.Populated = false;

    return Read("ReadAdministrativeAction2",
      (db, command) =>
      {
        db.SetString(
          command, "type", entities.AdministrativeActCertification.Type1);
      },
      (db, reader) =>
      {
        entities.AdministrativeAction.Type1 = db.GetString(reader, 0);
        entities.AdministrativeAction.Description = db.GetString(reader, 1);
        entities.AdministrativeAction.Populated = true;
      });
  }

  private bool ReadAdministrativeAppeal1()
  {
    entities.AdministrativeAppeal.Populated = false;

    return Read("ReadAdministrativeAppeal1",
      (db, command) =>
      {
        db.SetInt32(
          command, "adminAppealId", export.AdministrativeAppeal.Identifier);
      },
      (db, reader) =>
      {
        entities.AdministrativeAppeal.Identifier = db.GetInt32(reader, 0);
        entities.AdministrativeAppeal.Number = db.GetNullableString(reader, 1);
        entities.AdministrativeAppeal.Type1 = db.GetString(reader, 2);
        entities.AdministrativeAppeal.CreatedTstamp = db.GetDateTime(reader, 3);
        entities.AdministrativeAppeal.CspQNumber =
          db.GetNullableString(reader, 4);
        entities.AdministrativeAppeal.CpaRType =
          db.GetNullableString(reader, 5);
        entities.AdministrativeAppeal.CspRNumber =
          db.GetNullableString(reader, 6);
        entities.AdministrativeAppeal.AacRType =
          db.GetNullableString(reader, 7);
        entities.AdministrativeAppeal.AacRTakenDate =
          db.GetNullableDate(reader, 8);
        entities.AdministrativeAppeal.AatType = db.GetNullableString(reader, 9);
        entities.AdministrativeAppeal.ObgGeneratedId =
          db.GetNullableInt32(reader, 10);
        entities.AdministrativeAppeal.CspNumber =
          db.GetNullableString(reader, 11);
        entities.AdministrativeAppeal.CpaType =
          db.GetNullableString(reader, 12);
        entities.AdministrativeAppeal.OaaTakenDate =
          db.GetNullableDate(reader, 13);
        entities.AdministrativeAppeal.OtyId = db.GetNullableInt32(reader, 14);
        entities.AdministrativeAppeal.AacTanfCode =
          db.GetNullableString(reader, 15);
        entities.AdministrativeAppeal.Populated = true;
        CheckValid<AdministrativeAppeal>("CpaRType",
          entities.AdministrativeAppeal.CpaRType);
        CheckValid<AdministrativeAppeal>("AacRType",
          entities.AdministrativeAppeal.AacRType);
        CheckValid<AdministrativeAppeal>("CpaType",
          entities.AdministrativeAppeal.CpaType);
      });
  }

  private IEnumerable<bool> ReadAdministrativeAppeal2()
  {
    entities.AdministrativeAppeal.Populated = false;

    return ReadEach("ReadAdministrativeAppeal2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "number", import.AdministrativeAppeal.Number ?? "");
        db.SetNullableString(command, "cspQNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.AdministrativeAppeal.Identifier = db.GetInt32(reader, 0);
        entities.AdministrativeAppeal.Number = db.GetNullableString(reader, 1);
        entities.AdministrativeAppeal.Type1 = db.GetString(reader, 2);
        entities.AdministrativeAppeal.CreatedTstamp = db.GetDateTime(reader, 3);
        entities.AdministrativeAppeal.CspQNumber =
          db.GetNullableString(reader, 4);
        entities.AdministrativeAppeal.CpaRType =
          db.GetNullableString(reader, 5);
        entities.AdministrativeAppeal.CspRNumber =
          db.GetNullableString(reader, 6);
        entities.AdministrativeAppeal.AacRType =
          db.GetNullableString(reader, 7);
        entities.AdministrativeAppeal.AacRTakenDate =
          db.GetNullableDate(reader, 8);
        entities.AdministrativeAppeal.AatType = db.GetNullableString(reader, 9);
        entities.AdministrativeAppeal.ObgGeneratedId =
          db.GetNullableInt32(reader, 10);
        entities.AdministrativeAppeal.CspNumber =
          db.GetNullableString(reader, 11);
        entities.AdministrativeAppeal.CpaType =
          db.GetNullableString(reader, 12);
        entities.AdministrativeAppeal.OaaTakenDate =
          db.GetNullableDate(reader, 13);
        entities.AdministrativeAppeal.OtyId = db.GetNullableInt32(reader, 14);
        entities.AdministrativeAppeal.AacTanfCode =
          db.GetNullableString(reader, 15);
        entities.AdministrativeAppeal.Populated = true;
        CheckValid<AdministrativeAppeal>("CpaRType",
          entities.AdministrativeAppeal.CpaRType);
        CheckValid<AdministrativeAppeal>("AacRType",
          entities.AdministrativeAppeal.AacRType);
        CheckValid<AdministrativeAppeal>("CpaType",
          entities.AdministrativeAppeal.CpaType);

        return true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Occupation = db.GetNullableString(reader, 2);
        entities.CsePerson.AeCaseNumber = db.GetNullableString(reader, 3);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 4);
        entities.CsePerson.IllegalAlienIndicator =
          db.GetNullableString(reader, 5);
        entities.CsePerson.CurrentSpouseMi = db.GetNullableString(reader, 6);
        entities.CsePerson.CurrentSpouseFirstName =
          db.GetNullableString(reader, 7);
        entities.CsePerson.BirthPlaceState = db.GetNullableString(reader, 8);
        entities.CsePerson.EmergencyPhone = db.GetNullableInt32(reader, 9);
        entities.CsePerson.NameMiddle = db.GetNullableString(reader, 10);
        entities.CsePerson.NameMaiden = db.GetNullableString(reader, 11);
        entities.CsePerson.HomePhone = db.GetNullableInt32(reader, 12);
        entities.CsePerson.OtherNumber = db.GetNullableInt32(reader, 13);
        entities.CsePerson.BirthPlaceCity = db.GetNullableString(reader, 14);
        entities.CsePerson.CurrentMaritalStatus =
          db.GetNullableString(reader, 15);
        entities.CsePerson.CurrentSpouseLastName =
          db.GetNullableString(reader, 16);
        entities.CsePerson.Race = db.GetNullableString(reader, 17);
        entities.CsePerson.HairColor = db.GetNullableString(reader, 18);
        entities.CsePerson.EyeColor = db.GetNullableString(reader, 19);
        entities.CsePerson.Weight = db.GetNullableInt32(reader, 20);
        entities.CsePerson.HeightFt = db.GetNullableInt32(reader, 21);
        entities.CsePerson.HeightIn = db.GetNullableInt32(reader, 22);
        entities.CsePerson.OtherIdInfo = db.GetNullableString(reader, 23);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.AdministrativeAppeal.Populated);
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(
          command, "numb", entities.AdministrativeAppeal.CspQNumber ?? "");
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Occupation = db.GetNullableString(reader, 2);
        entities.CsePerson.AeCaseNumber = db.GetNullableString(reader, 3);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 4);
        entities.CsePerson.IllegalAlienIndicator =
          db.GetNullableString(reader, 5);
        entities.CsePerson.CurrentSpouseMi = db.GetNullableString(reader, 6);
        entities.CsePerson.CurrentSpouseFirstName =
          db.GetNullableString(reader, 7);
        entities.CsePerson.BirthPlaceState = db.GetNullableString(reader, 8);
        entities.CsePerson.EmergencyPhone = db.GetNullableInt32(reader, 9);
        entities.CsePerson.NameMiddle = db.GetNullableString(reader, 10);
        entities.CsePerson.NameMaiden = db.GetNullableString(reader, 11);
        entities.CsePerson.HomePhone = db.GetNullableInt32(reader, 12);
        entities.CsePerson.OtherNumber = db.GetNullableInt32(reader, 13);
        entities.CsePerson.BirthPlaceCity = db.GetNullableString(reader, 14);
        entities.CsePerson.CurrentMaritalStatus =
          db.GetNullableString(reader, 15);
        entities.CsePerson.CurrentSpouseLastName =
          db.GetNullableString(reader, 16);
        entities.CsePerson.Race = db.GetNullableString(reader, 17);
        entities.CsePerson.HairColor = db.GetNullableString(reader, 18);
        entities.CsePerson.EyeColor = db.GetNullableString(reader, 19);
        entities.CsePerson.Weight = db.GetNullableInt32(reader, 20);
        entities.CsePerson.HeightFt = db.GetNullableInt32(reader, 21);
        entities.CsePerson.HeightIn = db.GetNullableInt32(reader, 22);
        entities.CsePerson.OtherIdInfo = db.GetNullableString(reader, 23);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadPositionStatement()
  {
    return ReadEach("ReadPositionStatement",
      (db, command) =>
      {
        db.SetInt32(
          command, "aapIdentifier", entities.AdministrativeAppeal.Identifier);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.PositionStatement.AapIdentifier = db.GetInt32(reader, 0);
        entities.PositionStatement.Number = db.GetInt32(reader, 1);
        entities.PositionStatement.Explanation = db.GetString(reader, 2);
        entities.PositionStatement.Populated = true;

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
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private AdministrativeAppeal administrativeAppeal;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of PositionStatement.
      /// </summary>
      [JsonPropertyName("positionStatement")]
      public PositionStatement PositionStatement
      {
        get => positionStatement ??= new();
        set => positionStatement = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Common common;
      private PositionStatement positionStatement;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
    }

    /// <summary>
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 => export1 ??= new(ExportGroup.Capacity);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private AdministrativeAction administrativeAction;
    private AdministrativeAppeal administrativeAppeal;
    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of ReturnCode.
    /// </summary>
    [JsonPropertyName("returnCode")]
    public Common ReturnCode
    {
      get => returnCode ??= new();
      set => returnCode = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private Common common;
    private Common returnCode;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of AdministrativeActCertification.
    /// </summary>
    [JsonPropertyName("administrativeActCertification")]
    public AdministrativeActCertification AdministrativeActCertification
    {
      get => administrativeActCertification ??= new();
      set => administrativeActCertification = value;
    }

    /// <summary>
    /// A value of ObligationAdministrativeAction.
    /// </summary>
    [JsonPropertyName("obligationAdministrativeAction")]
    public ObligationAdministrativeAction ObligationAdministrativeAction
    {
      get => obligationAdministrativeAction ??= new();
      set => obligationAdministrativeAction = value;
    }

    /// <summary>
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
    }

    /// <summary>
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
    }

    /// <summary>
    /// A value of PositionStatement.
    /// </summary>
    [JsonPropertyName("positionStatement")]
    public PositionStatement PositionStatement
    {
      get => positionStatement ??= new();
      set => positionStatement = value;
    }

    private CsePerson csePerson;
    private AdministrativeActCertification administrativeActCertification;
    private ObligationAdministrativeAction obligationAdministrativeAction;
    private AdministrativeAction administrativeAction;
    private AdministrativeAppeal administrativeAppeal;
    private PositionStatement positionStatement;
  }
#endregion
}
