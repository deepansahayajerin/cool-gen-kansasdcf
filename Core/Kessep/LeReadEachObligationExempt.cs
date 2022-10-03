// Program: LE_READ_EACH_OBLIGATION_EXEMPT, ID: 372588310, model: 746.
// Short name: SWE00815
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
/// A program: LE_READ_EACH_OBLIGATION_EXEMPT.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block reads each Obligation Administrative Action Exemption for 
/// a specific CSE Person.
/// </para>
/// </summary>
[Serializable]
public partial class LeReadEachObligationExempt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_READ_EACH_OBLIGATION_EXEMPT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeReadEachObligationExempt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeReadEachObligationExempt.
  /// </summary>
  public LeReadEachObligationExempt(IContext context, Import import,
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
    // ****************************************************************
    // 11/16/98  P. Sharp      Removed check of imports from read each qualifier
    // and made if statements.  Removed action block si_read_cse_person. This
    // was already performed in the Pstep.
    export.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;
    export.AdministrativeAction.Type1 = import.AdministrativeAction.Type1;
    export.ObligationType.Code = import.Required.Code;
    local.Obligation.Count = 0;
    export.Export1.Index = -1;

    if (!IsEmpty(import.AdministrativeAction.Type1))
    {
      if (ReadAdministrativeAction())
      {
        MoveAdministrativeAction(entities.AdministrativeAction,
          export.AdministrativeAction);
      }
      else
      {
        ExitState = "ADMINISTRATIVE_ACTION_NF";

        return;
      }
    }

    if (!IsEmpty(import.Required.Code))
    {
      if (ReadObligationType())
      {
        MoveObligationType(entities.ObligationType, export.ObligationType);
      }
      else
      {
        ExitState = "FN0000_OBLIGATION_TYPE_NF";

        return;
      }
    }

    if (!IsEmpty(import.AdministrativeAction.Type1))
    {
      foreach(var item in ReadObligationAdmActionExemptionObligationObligationType())
        
      {
        if (!Lt(local.InitialisedToZeros.EffectiveDate,
          import.Starting.EffectiveDate))
        {
        }
        else if (Lt(entities.ObligationAdmActionExemption.EffectiveDate,
          import.Starting.EffectiveDate))
        {
        }
        else
        {
          continue;
        }

        if (IsEmpty(import.Required.Code))
        {
        }
        else if (Equal(entities.ObligationType.Code, import.Required.Code))
        {
        }
        else
        {
          continue;
        }

        ++export.Export1.Index;
        export.Export1.CheckSize();

        export.Export1.Update.DetailAdministrativeAction.Type1 =
          entities.AdministrativeAction.Type1;
        MoveObligation(entities.Obligation,
          export.Export1.Update.DetailObligation);
        export.Export1.Update.DetailObligationType.Assign(
          entities.ObligationType);
        export.Export1.Update.DetailObligationAdmActionExemption.Assign(
          entities.ObligationAdmActionExemption);

        // ***********************************************************
        // Move zeroes to End Date if it is equal to Maximum
        // Discontinue Date.
        // ***********************************************************
        if (Equal(export.Export1.Item.DetailObligationAdmActionExemption.
          EndDate, new DateTime(2099, 12, 31)))
        {
          export.Export1.Update.DetailObligationAdmActionExemption.EndDate =
            null;
        }

        // ************************************************************
        // Format the Obligation Adm Act Exemption Name for display
        // purposes.
        // ************************************************************
        local.ToBeFormatted.FirstName =
          entities.ObligationAdmActionExemption.FirstName;
        local.ToBeFormatted.LastName =
          entities.ObligationAdmActionExemption.LastName;
        local.ToBeFormatted.MiddleInitial =
          entities.ObligationAdmActionExemption.MiddleInitial ?? Spaces(1);
        UseSiFormatCsePersonName();
        export.Export1.Update.DetailCsePersonsWorkSet.FormattedName =
          local.ToBeFormatted.FormattedName;

        if (export.Export1.Index + 1 >= Export.ExportGroup.Capacity)
        {
          return;
        }
      }
    }
    else
    {
      foreach(var item in ReadObligationAdmActionExemptionAdministrativeAction())
        
      {
        if (!Lt(local.InitialisedToZeros.EffectiveDate,
          import.Starting.EffectiveDate))
        {
        }
        else if (Lt(entities.ObligationAdmActionExemption.EffectiveDate,
          import.Starting.EffectiveDate))
        {
        }
        else
        {
          continue;
        }

        if (IsEmpty(import.Required.Code))
        {
        }
        else if (Equal(entities.ObligationType.Code, import.Required.Code))
        {
        }
        else
        {
          continue;
        }

        ++export.Export1.Index;
        export.Export1.CheckSize();

        export.Export1.Update.DetailAdministrativeAction.Type1 =
          entities.AdministrativeAction.Type1;
        MoveObligation(entities.Obligation,
          export.Export1.Update.DetailObligation);
        export.Export1.Update.DetailObligationType.Assign(
          entities.ObligationType);
        export.Export1.Update.DetailObligationAdmActionExemption.Assign(
          entities.ObligationAdmActionExemption);

        // ***********************************************************
        // Move zeroes to End Date if it is equal to Maximum
        // Discontinue Date.
        // ***********************************************************
        if (Equal(export.Export1.Item.DetailObligationAdmActionExemption.
          EndDate, new DateTime(2099, 12, 31)))
        {
          export.Export1.Update.DetailObligationAdmActionExemption.EndDate =
            null;
        }

        // ************************************************************
        // Format the Obligation Adm Act Exemption Name for display
        // purposes.
        // ************************************************************
        local.ToBeFormatted.FirstName =
          entities.ObligationAdmActionExemption.FirstName;
        local.ToBeFormatted.LastName =
          entities.ObligationAdmActionExemption.LastName;
        local.ToBeFormatted.MiddleInitial =
          entities.ObligationAdmActionExemption.MiddleInitial ?? Spaces(1);
        UseSiFormatCsePersonName();
        export.Export1.Update.DetailCsePersonsWorkSet.FormattedName =
          local.ToBeFormatted.FormattedName;

        if (export.Export1.Index + 1 >= Export.ExportGroup.Capacity)
        {
          return;
        }
      }
    }
  }

  private static void MoveAdministrativeAction(AdministrativeAction source,
    AdministrativeAction target)
  {
    target.Type1 = source.Type1;
    target.Description = source.Description;
  }

  private static void MoveObligation(Obligation source, Obligation target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Description = source.Description;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.Code = source.Code;
    target.Name = source.Name;
  }

  private void UseSiFormatCsePersonName()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(local.ToBeFormatted);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    local.ToBeFormatted.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private bool ReadAdministrativeAction()
  {
    entities.AdministrativeAction.Populated = false;

    return Read("ReadAdministrativeAction",
      (db, command) =>
      {
        db.SetString(command, "type", import.AdministrativeAction.Type1);
      },
      (db, reader) =>
      {
        entities.AdministrativeAction.Type1 = db.GetString(reader, 0);
        entities.AdministrativeAction.Description = db.GetString(reader, 1);
        entities.AdministrativeAction.Populated = true;
      });
  }

  private IEnumerable<bool>
    ReadObligationAdmActionExemptionAdministrativeAction()
  {
    entities.ObligationType.Populated = false;
    entities.ObligationAdmActionExemption.Populated = false;
    entities.AdministrativeAction.Populated = false;
    entities.Obligation.Populated = false;

    return ReadEach("ReadObligationAdmActionExemptionAdministrativeAction",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.ObligationAdmActionExemption.OtyType = db.GetInt32(reader, 0);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationAdmActionExemption.AatType = db.GetString(reader, 1);
        entities.AdministrativeAction.Type1 = db.GetString(reader, 1);
        entities.ObligationAdmActionExemption.ObgGeneratedId =
          db.GetInt32(reader, 2);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.ObligationAdmActionExemption.CspNumber =
          db.GetString(reader, 3);
        entities.Obligation.CspNumber = db.GetString(reader, 3);
        entities.ObligationAdmActionExemption.CpaType = db.GetString(reader, 4);
        entities.Obligation.CpaType = db.GetString(reader, 4);
        entities.ObligationAdmActionExemption.EffectiveDate =
          db.GetDate(reader, 5);
        entities.ObligationAdmActionExemption.EndDate = db.GetDate(reader, 6);
        entities.ObligationAdmActionExemption.LastName =
          db.GetString(reader, 7);
        entities.ObligationAdmActionExemption.FirstName =
          db.GetString(reader, 8);
        entities.ObligationAdmActionExemption.MiddleInitial =
          db.GetNullableString(reader, 9);
        entities.ObligationAdmActionExemption.Reason = db.GetString(reader, 10);
        entities.ObligationAdmActionExemption.Description =
          db.GetNullableString(reader, 11);
        entities.AdministrativeAction.Description = db.GetString(reader, 12);
        entities.Obligation.Description = db.GetNullableString(reader, 13);
        entities.ObligationType.Code = db.GetString(reader, 14);
        entities.ObligationType.Name = db.GetString(reader, 15);
        entities.ObligationType.Populated = true;
        entities.ObligationAdmActionExemption.Populated = true;
        entities.AdministrativeAction.Populated = true;
        entities.Obligation.Populated = true;
        CheckValid<ObligationAdmActionExemption>("CpaType",
          entities.ObligationAdmActionExemption.CpaType);
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);

        return true;
      });
  }

  private IEnumerable<bool>
    ReadObligationAdmActionExemptionObligationObligationType()
  {
    entities.ObligationType.Populated = false;
    entities.ObligationAdmActionExemption.Populated = false;
    entities.Obligation.Populated = false;

    return ReadEach("ReadObligationAdmActionExemptionObligationObligationType",
      (db, command) =>
      {
        db.SetString(command, "aatType", entities.AdministrativeAction.Type1);
        db.SetString(command, "cspNumber", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.ObligationAdmActionExemption.OtyType = db.GetInt32(reader, 0);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationAdmActionExemption.AatType = db.GetString(reader, 1);
        entities.ObligationAdmActionExemption.ObgGeneratedId =
          db.GetInt32(reader, 2);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.ObligationAdmActionExemption.CspNumber =
          db.GetString(reader, 3);
        entities.Obligation.CspNumber = db.GetString(reader, 3);
        entities.ObligationAdmActionExemption.CpaType = db.GetString(reader, 4);
        entities.Obligation.CpaType = db.GetString(reader, 4);
        entities.ObligationAdmActionExemption.EffectiveDate =
          db.GetDate(reader, 5);
        entities.ObligationAdmActionExemption.EndDate = db.GetDate(reader, 6);
        entities.ObligationAdmActionExemption.LastName =
          db.GetString(reader, 7);
        entities.ObligationAdmActionExemption.FirstName =
          db.GetString(reader, 8);
        entities.ObligationAdmActionExemption.MiddleInitial =
          db.GetNullableString(reader, 9);
        entities.ObligationAdmActionExemption.Reason = db.GetString(reader, 10);
        entities.ObligationAdmActionExemption.Description =
          db.GetNullableString(reader, 11);
        entities.Obligation.Description = db.GetNullableString(reader, 12);
        entities.ObligationType.Code = db.GetString(reader, 13);
        entities.ObligationType.Name = db.GetString(reader, 14);
        entities.ObligationType.Populated = true;
        entities.ObligationAdmActionExemption.Populated = true;
        entities.Obligation.Populated = true;
        CheckValid<ObligationAdmActionExemption>("CpaType",
          entities.ObligationAdmActionExemption.CpaType);
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);

        return true;
      });
  }

  private bool ReadObligationType()
  {
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetString(command, "debtTypCd", import.Required.Code);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Name = db.GetString(reader, 2);
        entities.ObligationType.Populated = true;
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
    /// A value of Required.
    /// </summary>
    [JsonPropertyName("required")]
    public ObligationType Required
    {
      get => required ??= new();
      set => required = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public ObligationAdmActionExemption Starting
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

    /// <summary>
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
    }

    private ObligationType required;
    private ObligationAdmActionExemption starting;
    private CsePersonsWorkSet csePersonsWorkSet;
    private AdministrativeAction administrativeAction;
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
      /// A value of DetailObligationType.
      /// </summary>
      [JsonPropertyName("detailObligationType")]
      public ObligationType DetailObligationType
      {
        get => detailObligationType ??= new();
        set => detailObligationType = value;
      }

      /// <summary>
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of DetailAdministrativeAction.
      /// </summary>
      [JsonPropertyName("detailAdministrativeAction")]
      public AdministrativeAction DetailAdministrativeAction
      {
        get => detailAdministrativeAction ??= new();
        set => detailAdministrativeAction = value;
      }

      /// <summary>
      /// A value of DetailCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("detailCsePersonsWorkSet")]
      public CsePersonsWorkSet DetailCsePersonsWorkSet
      {
        get => detailCsePersonsWorkSet ??= new();
        set => detailCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DetailObligation.
      /// </summary>
      [JsonPropertyName("detailObligation")]
      public Obligation DetailObligation
      {
        get => detailObligation ??= new();
        set => detailObligation = value;
      }

      /// <summary>
      /// A value of DetailObligationAdmActionExemption.
      /// </summary>
      [JsonPropertyName("detailObligationAdmActionExemption")]
      public ObligationAdmActionExemption DetailObligationAdmActionExemption
      {
        get => detailObligationAdmActionExemption ??= new();
        set => detailObligationAdmActionExemption = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private ObligationType detailObligationType;
      private Common detailCommon;
      private AdministrativeAction detailAdministrativeAction;
      private CsePersonsWorkSet detailCsePersonsWorkSet;
      private Obligation detailObligation;
      private ObligationAdmActionExemption detailObligationAdmActionExemption;
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
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
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
    public Array<ExportGroup> Export1 =>
      export1 ??= new(ExportGroup.Capacity, 0);

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

    private ObligationType obligationType;
    private AdministrativeAction administrativeAction;
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
    public ObligationAdmActionExemption InitialisedToZeros
    {
      get => initialisedToZeros ??= new();
      set => initialisedToZeros = value;
    }

    /// <summary>
    /// A value of ToBeFormatted.
    /// </summary>
    [JsonPropertyName("toBeFormatted")]
    public CsePersonsWorkSet ToBeFormatted
    {
      get => toBeFormatted ??= new();
      set => toBeFormatted = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Common Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    private ObligationAdmActionExemption initialisedToZeros;
    private CsePersonsWorkSet toBeFormatted;
    private Common obligation;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ObligationAdmActionExemption.
    /// </summary>
    [JsonPropertyName("obligationAdmActionExemption")]
    public ObligationAdmActionExemption ObligationAdmActionExemption
    {
      get => obligationAdmActionExemption ??= new();
      set => obligationAdmActionExemption = value;
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

    private ObligationType obligationType;
    private ObligationAdmActionExemption obligationAdmActionExemption;
    private AdministrativeAction administrativeAction;
    private Obligation obligation;
    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
  }
#endregion
}
