// Program: LE_DISP_ADMIN_APPEALS_BY_CSE_PER, ID: 372581244, model: 746.
// Short name: SWE00764
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
/// A program: LE_DISP_ADMIN_APPEALS_BY_CSE_PER.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block reads all Administrative Appeals for a selected CSE 
/// Person.
/// </para>
/// </summary>
[Serializable]
public partial class LeDispAdminAppealsByCsePer: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_DISP_ADMIN_APPEALS_BY_CSE_PER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeDispAdminAppealsByCsePer(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeDispAdminAppealsByCsePer.
  /// </summary>
  public LeDispAdminAppealsByCsePer(IContext context, Import import,
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
    UseSiReadCsePerson();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    export.Export1.Index = 0;
    export.Export1.Clear();

    foreach(var item in ReadAdministrativeAppeal())
    {
      MoveAdministrativeAppeal(entities.AdministrativeAppeal,
        export.Export1.Update.AdministrativeAppeal);

      if (ReadAdministrativeActionObligationAdministrativeAction())
      {
        export.Export1.Update.AdministrativeAction.Type1 =
          entities.AdministrativeAction.Type1;
        export.Export1.Update.DateWorkArea.Date =
          entities.ObligationAdministrativeAction.TakenDate;
      }
      else if (ReadAdministrativeActCertification())
      {
        export.Export1.Update.AdministrativeAction.Type1 =
          entities.AdministrativeActCertification.Type1;
        export.Export1.Update.DateWorkArea.Date =
          entities.AdministrativeActCertification.TakenDate;
      }

      export.Export1.Next();
    }
  }

  private static void MoveAdministrativeAppeal(AdministrativeAppeal source,
    AdministrativeAppeal target)
  {
    target.Identifier = source.Identifier;
    target.Number = source.Number;
    target.Type1 = source.Type1;
    target.ReceivedDate = source.ReceivedDate;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

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

  private bool ReadAdministrativeActionObligationAdministrativeAction()
  {
    System.Diagnostics.Debug.Assert(entities.AdministrativeAppeal.Populated);
    entities.ObligationAdministrativeAction.Populated = false;
    entities.AdministrativeAction.Populated = false;

    return Read("ReadAdministrativeActionObligationAdministrativeAction",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "otyId",
          entities.AdministrativeAppeal.OtyId.GetValueOrDefault());
        db.SetNullableString(
          command, "aatType", entities.AdministrativeAppeal.AatType ?? "");
        db.SetNullableInt32(
          command, "obgGeneratedId",
          entities.AdministrativeAppeal.ObgGeneratedId.GetValueOrDefault());
        db.SetNullableString(
          command, "cspNumber", entities.AdministrativeAppeal.CspNumber ?? "");
        db.SetNullableString(
          command, "cpaType", entities.AdministrativeAppeal.CpaType ?? "");
        db.SetNullableDate(
          command, "oaaTakenDate",
          entities.AdministrativeAppeal.OaaTakenDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.AdministrativeAction.Type1 = db.GetString(reader, 0);
        entities.ObligationAdministrativeAction.OtyType =
          db.GetInt32(reader, 1);
        entities.ObligationAdministrativeAction.AatType =
          db.GetString(reader, 2);
        entities.ObligationAdministrativeAction.ObgGeneratedId =
          db.GetInt32(reader, 3);
        entities.ObligationAdministrativeAction.CspNumber =
          db.GetString(reader, 4);
        entities.ObligationAdministrativeAction.CpaType =
          db.GetString(reader, 5);
        entities.ObligationAdministrativeAction.TakenDate =
          db.GetDate(reader, 6);
        entities.ObligationAdministrativeAction.Populated = true;
        entities.AdministrativeAction.Populated = true;
        CheckValid<ObligationAdministrativeAction>("CpaType",
          entities.ObligationAdministrativeAction.CpaType);
      });
  }

  private IEnumerable<bool> ReadAdministrativeAppeal()
  {
    return ReadEach("ReadAdministrativeAppeal",
      (db, command) =>
      {
        db.SetNullableString(command, "cspQNumber", entities.CsePerson.Number);
        db.SetDate(
          command, "receivedDate1",
          import.Starting.ReceivedDate.GetValueOrDefault());
        db.SetDate(
          command, "receivedDate2",
          local.InitialisedToZeros.ReceivedDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.AdministrativeAppeal.Identifier = db.GetInt32(reader, 0);
        entities.AdministrativeAppeal.Number = db.GetNullableString(reader, 1);
        entities.AdministrativeAppeal.Type1 = db.GetString(reader, 2);
        entities.AdministrativeAppeal.RequestDate = db.GetDate(reader, 3);
        entities.AdministrativeAppeal.ReceivedDate = db.GetDate(reader, 4);
        entities.AdministrativeAppeal.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.AdministrativeAppeal.CspQNumber =
          db.GetNullableString(reader, 6);
        entities.AdministrativeAppeal.CpaRType =
          db.GetNullableString(reader, 7);
        entities.AdministrativeAppeal.CspRNumber =
          db.GetNullableString(reader, 8);
        entities.AdministrativeAppeal.AacRType =
          db.GetNullableString(reader, 9);
        entities.AdministrativeAppeal.AacRTakenDate =
          db.GetNullableDate(reader, 10);
        entities.AdministrativeAppeal.AatType =
          db.GetNullableString(reader, 11);
        entities.AdministrativeAppeal.ObgGeneratedId =
          db.GetNullableInt32(reader, 12);
        entities.AdministrativeAppeal.CspNumber =
          db.GetNullableString(reader, 13);
        entities.AdministrativeAppeal.CpaType =
          db.GetNullableString(reader, 14);
        entities.AdministrativeAppeal.OaaTakenDate =
          db.GetNullableDate(reader, 15);
        entities.AdministrativeAppeal.OtyId = db.GetNullableInt32(reader, 16);
        entities.AdministrativeAppeal.AacTanfCode =
          db.GetNullableString(reader, 17);
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

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
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
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public AdministrativeAppeal Starting
    {
      get => starting ??= new();
      set => starting = value;
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

    private AdministrativeAppeal starting;
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
      /// A value of DateWorkArea.
      /// </summary>
      [JsonPropertyName("dateWorkArea")]
      public DateWorkArea DateWorkArea
      {
        get => dateWorkArea ??= new();
        set => dateWorkArea = value;
      }

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
      /// A value of AdministrativeAppeal.
      /// </summary>
      [JsonPropertyName("administrativeAppeal")]
      public AdministrativeAppeal AdministrativeAppeal
      {
        get => administrativeAppeal ??= new();
        set => administrativeAppeal = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private DateWorkArea dateWorkArea;
      private Common common;
      private AdministrativeAppeal administrativeAppeal;
      private AdministrativeAction administrativeAction;
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
    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of InitialisedToZeros.
    /// </summary>
    [JsonPropertyName("initialisedToZeros")]
    public AdministrativeAppeal InitialisedToZeros
    {
      get => initialisedToZeros ??= new();
      set => initialisedToZeros = value;
    }

    private AdministrativeAppeal initialisedToZeros;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
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
    /// A value of AdministrativeActCertification.
    /// </summary>
    [JsonPropertyName("administrativeActCertification")]
    public AdministrativeActCertification AdministrativeActCertification
    {
      get => administrativeActCertification ??= new();
      set => administrativeActCertification = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private Obligation obligation;
    private CsePersonAccount csePersonAccount;
    private AdministrativeAppeal administrativeAppeal;
    private ObligationAdministrativeAction obligationAdministrativeAction;
    private AdministrativeActCertification administrativeActCertification;
    private AdministrativeAction administrativeAction;
    private CsePerson csePerson;
  }
#endregion
}
