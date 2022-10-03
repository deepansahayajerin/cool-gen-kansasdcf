// Program: FN_GET_EACH_MANUAL_DIST_AUDIT, ID: 372039876, model: 746.
// Short name: SWE00470
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
/// A program: FN_GET_EACH_MANUAL_DIST_AUDIT.
/// </para>
/// <para>
/// RESP: FINANCE
/// </para>
/// </summary>
[Serializable]
public partial class FnGetEachManualDistAudit: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_GET_EACH_MANUAL_DIST_AUDIT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnGetEachManualDistAudit(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnGetEachManualDistAudit.
  /// </summary>
  public FnGetEachManualDistAudit(IContext context, Import import, Export export)
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
    // ************************************************************
    // 04/30/97	SHERAZ MALIK	CHANGE CURRENT_DATE
    // 12/15/98        G Sharp         1. Change sort from descending to 
    // ascending on both read each. 2. on 2nd read each change > to =>.
    // *************************************************************
    if (ReadObligation())
    {
      if (AsChar(import.ShowHistory.Flag) == 'Y')
      {
        export.Export1.Index = 0;
        export.Export1.Clear();

        foreach(var item in ReadManualDistributionAudit2())
        {
          export.Export1.Update.ManualDistributionAudit.Assign(
            entities.ExistingManualDistributionAudit);
          export.Export1.Next();
        }
      }
      else
      {
        export.Export1.Index = 0;
        export.Export1.Clear();

        foreach(var item in ReadManualDistributionAudit1())
        {
          export.Export1.Update.ManualDistributionAudit.Assign(
            entities.ExistingManualDistributionAudit);
          export.Export1.Next();
        }
      }
    }
    else
    {
      ExitState = "FN0000_OBLIGATION_NF";
    }
  }

  private IEnumerable<bool> ReadManualDistributionAudit1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);

    return ReadEach("ReadManualDistributionAudit1",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyType", entities.ExistingObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ExistingObligation.SystemGeneratedIdentifier);
        db.
          SetString(command, "cspNumber", entities.ExistingObligation.CspNumber);
          
        db.SetString(command, "cpaType", entities.ExistingObligation.CpaType);
        db.SetNullableDate(
          command, "discontinueDt", import.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ExistingManualDistributionAudit.OtyType =
          db.GetInt32(reader, 0);
        entities.ExistingManualDistributionAudit.ObgGeneratedId =
          db.GetInt32(reader, 1);
        entities.ExistingManualDistributionAudit.CspNumber =
          db.GetString(reader, 2);
        entities.ExistingManualDistributionAudit.CpaType =
          db.GetString(reader, 3);
        entities.ExistingManualDistributionAudit.EffectiveDt =
          db.GetDate(reader, 4);
        entities.ExistingManualDistributionAudit.DiscontinueDt =
          db.GetNullableDate(reader, 5);
        entities.ExistingManualDistributionAudit.CreatedBy =
          db.GetString(reader, 6);
        entities.ExistingManualDistributionAudit.CreatedTmst =
          db.GetDateTime(reader, 7);
        entities.ExistingManualDistributionAudit.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.ExistingManualDistributionAudit.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 9);
        entities.ExistingManualDistributionAudit.Instructions =
          db.GetNullableString(reader, 10);
        entities.ExistingManualDistributionAudit.Populated = true;
        CheckValid<ManualDistributionAudit>("CpaType",
          entities.ExistingManualDistributionAudit.CpaType);

        return true;
      });
  }

  private IEnumerable<bool> ReadManualDistributionAudit2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);

    return ReadEach("ReadManualDistributionAudit2",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyType", entities.ExistingObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ExistingObligation.SystemGeneratedIdentifier);
        db.
          SetString(command, "cspNumber", entities.ExistingObligation.CspNumber);
          
        db.SetString(command, "cpaType", entities.ExistingObligation.CpaType);
        db.SetDate(
          command, "effectiveDt",
          import.Starting.EffectiveDt.GetValueOrDefault());
        db.SetDate(
          command, "date", local.InitialisedToZeros.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ExistingManualDistributionAudit.OtyType =
          db.GetInt32(reader, 0);
        entities.ExistingManualDistributionAudit.ObgGeneratedId =
          db.GetInt32(reader, 1);
        entities.ExistingManualDistributionAudit.CspNumber =
          db.GetString(reader, 2);
        entities.ExistingManualDistributionAudit.CpaType =
          db.GetString(reader, 3);
        entities.ExistingManualDistributionAudit.EffectiveDt =
          db.GetDate(reader, 4);
        entities.ExistingManualDistributionAudit.DiscontinueDt =
          db.GetNullableDate(reader, 5);
        entities.ExistingManualDistributionAudit.CreatedBy =
          db.GetString(reader, 6);
        entities.ExistingManualDistributionAudit.CreatedTmst =
          db.GetDateTime(reader, 7);
        entities.ExistingManualDistributionAudit.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.ExistingManualDistributionAudit.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 9);
        entities.ExistingManualDistributionAudit.Instructions =
          db.GetNullableString(reader, 10);
        entities.ExistingManualDistributionAudit.Populated = true;
        CheckValid<ManualDistributionAudit>("CpaType",
          entities.ExistingManualDistributionAudit.CpaType);

        return true;
      });
  }

  private bool ReadObligation()
  {
    entities.ExistingObligation.Populated = false;

    return Read("ReadObligation",
      (db, command) =>
      {
        db.
          SetInt32(command, "obId", import.Obligation.SystemGeneratedIdentifier);
          
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetInt32(
          command, "dtyGeneratedId",
          import.ObligationType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingObligation.CpaType = db.GetString(reader, 0);
        entities.ExistingObligation.CspNumber = db.GetString(reader, 1);
        entities.ExistingObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ExistingObligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.ExistingObligation.CpaType);
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public ManualDistributionAudit Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of ShowHistory.
    /// </summary>
    [JsonPropertyName("showHistory")]
    public Common ShowHistory
    {
      get => showHistory ??= new();
      set => showHistory = value;
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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

    private DateWorkArea current;
    private ManualDistributionAudit starting;
    private Common showHistory;
    private CsePerson csePerson;
    private CsePersonAccount csePersonAccount;
    private ObligationType obligationType;
    private Obligation obligation;
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
      /// A value of ManualDistributionAudit.
      /// </summary>
      [JsonPropertyName("manualDistributionAudit")]
      public ManualDistributionAudit ManualDistributionAudit
      {
        get => manualDistributionAudit ??= new();
        set => manualDistributionAudit = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common common;
      private ManualDistributionAudit manualDistributionAudit;
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
    public DateWorkArea InitialisedToZeros
    {
      get => initialisedToZeros ??= new();
      set => initialisedToZeros = value;
    }

    /// <summary>
    /// A value of ZdelLocalCurrent.
    /// </summary>
    [JsonPropertyName("zdelLocalCurrent")]
    public DateWorkArea ZdelLocalCurrent
    {
      get => zdelLocalCurrent ??= new();
      set => zdelLocalCurrent = value;
    }

    private DateWorkArea initialisedToZeros;
    private DateWorkArea zdelLocalCurrent;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingManualDistributionAudit.
    /// </summary>
    [JsonPropertyName("existingManualDistributionAudit")]
    public ManualDistributionAudit ExistingManualDistributionAudit
    {
      get => existingManualDistributionAudit ??= new();
      set => existingManualDistributionAudit = value;
    }

    /// <summary>
    /// A value of ExistingObligation.
    /// </summary>
    [JsonPropertyName("existingObligation")]
    public Obligation ExistingObligation
    {
      get => existingObligation ??= new();
      set => existingObligation = value;
    }

    /// <summary>
    /// A value of ExistingObligor.
    /// </summary>
    [JsonPropertyName("existingObligor")]
    public CsePersonAccount ExistingObligor
    {
      get => existingObligor ??= new();
      set => existingObligor = value;
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

    /// <summary>
    /// A value of ExistingObligationType.
    /// </summary>
    [JsonPropertyName("existingObligationType")]
    public ObligationType ExistingObligationType
    {
      get => existingObligationType ??= new();
      set => existingObligationType = value;
    }

    private ManualDistributionAudit existingManualDistributionAudit;
    private Obligation existingObligation;
    private CsePersonAccount existingObligor;
    private CsePerson existingCsePerson;
    private ObligationType existingObligationType;
  }
#endregion
}
