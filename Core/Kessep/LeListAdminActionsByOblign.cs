// Program: LE_LIST_ADMIN_ACTIONS_BY_OBLIGN, ID: 372599882, model: 746.
// Short name: SWE00795
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
/// A program: LE_LIST_ADMIN_ACTIONS_BY_OBLIGN.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block reads and returns all administrative actions for a given 
/// obligor
/// </para>
/// </summary>
[Serializable]
public partial class LeListAdminActionsByOblign: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LIST_ADMIN_ACTIONS_BY_OBLIGN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeListAdminActionsByOblign(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeListAdminActionsByOblign.
  /// </summary>
  public LeListAdminActionsByOblign(IContext context, Import import,
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
    // *******************************************************************
    // Date		Developer	Request #
    // Description
    // ????		Initial development
    // 10/01/98	P. Sharp      Phase II assesment changes.
    // Moved read of cse person inside the when not found of cse
    // person account. Trying to eliminate unnecessary I/O.
    // *******************************************************************
    export.Obligation.SystemGeneratedIdentifier =
      import.Obligation.SystemGeneratedIdentifier;

    if (!IsEmpty(import.Required.Type1))
    {
      if (ReadAdministrativeAction())
      {
        if (!IsEmpty(import.ListOptAutoManualActs.OneChar))
        {
          if (AsChar(entities.AdministrativeAction.Indicator) != AsChar
            (import.ListOptAutoManualActs.OneChar))
          {
            ExitState = "LE0000_LIST_NOT_MATCH_TYPE";

            return;
          }
        }
      }
      else
      {
        ExitState = "LE0000_INVALID_ADMIN_ACTION_TYPE";

        return;
      }
    }

    if (!ReadCsePersonAccount())
    {
      if (ReadCsePerson())
      {
        ExitState = "FN0000_CSE_PERSON_ACCOUNT_NF";

        return;
      }
      else
      {
        ExitState = "CSE_PERSON_NF";

        return;
      }
    }

    if (ReadObligation())
    {
      export.Obligation.Assign(entities.ExistingObligation);

      if (ReadObligationType())
      {
        MoveObligationType(entities.ExistingObligationType,
          export.ObligationType);
      }
    }
    else
    {
      ExitState = "FN0000_OBLIGATION_NF";

      return;
    }

    export.AdminActions.Index = -1;
    UseLeCabGetAdmActForOblign();
    local.AdminActions.Index = 0;

    for(var limit = export.TotalNoOfEntries.Count; local.AdminActions.Index < limit
      ; ++local.AdminActions.Index)
    {
      if (!local.AdminActions.CheckSize())
      {
        break;
      }

      export.AdminActions.Index = local.AdminActions.Index;
      export.AdminActions.CheckSize();

      export.AdminActions.Update.DetailObligor.Number = import.CsePerson.Number;
      export.AdminActions.Update.DetailObligation.SystemGeneratedIdentifier =
        entities.ExistingObligation.SystemGeneratedIdentifier;
      export.AdminActions.Update.DetailCertified.SelectChar =
        local.AdminActions.Item.DetailCertified.SelectChar;
      export.AdminActions.Update.DetailAdministrativeAction.Type1 =
        local.AdminActions.Item.DetailAdministrativeAction.Type1;

      if (AsChar(local.AdminActions.Item.DetailCertified.SelectChar) == 'Y')
      {
        export.AdminActions.Update.DetailAdministrativeActCertification.Assign(
          local.AdminActions.Item.DetailAdministrativeActCertification);
        MoveAdministrativeActCertification(local.AdminActions.Item.
          DetailFederalDebtSetoff,
          export.AdminActions.Update.DetailFederalDebtSetoff);
      }
      else
      {
        export.AdminActions.Update.DetailObligationAdministrativeAction.
          TakenDate =
            local.AdminActions.Item.DetailObligationAdministrativeAction.
            TakenDate;

        // ---------------------------------------------
        // The following statement is needed since the same screen field is used
        // for both entity types administration_act_certification and
        // obligation_administrative_action.
        // ---------------------------------------------
        export.AdminActions.Update.DetailAdministrativeActCertification.
          TakenDate =
            local.AdminActions.Item.DetailObligationAdministrativeAction.
            TakenDate;
      }
    }

    local.AdminActions.CheckIndex();
  }

  private static void MoveAdministrativeActCertification(
    AdministrativeActCertification source,
    AdministrativeActCertification target)
  {
    target.AdcAmount = source.AdcAmount;
    target.NonAdcAmount = source.NonAdcAmount;
  }

  private static void MoveExport1ToAdminActions(LeCabGetAdmActForOblign.Export.
    ExportGroup source, Local.AdminActionsGroup target)
  {
    target.DetailCertified.SelectChar = source.DetailCertfiable.SelectChar;
    target.DetailAdministrativeActCertification.Assign(
      source.DetailAdministrativeActCertification);
    MoveAdministrativeActCertification(source.DetailFederalDebtSetoff,
      target.DetailFederalDebtSetoff);
    target.DetailObligationAdministrativeAction.TakenDate =
      source.DetailObligationAdministrativeAction.TakenDate;
    target.DetailAdministrativeAction.Type1 =
      source.DetailAdministrativeAction.Type1;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.Code = source.Code;
    target.Name = source.Name;
  }

  private void UseLeCabGetAdmActForOblign()
  {
    var useImport = new LeCabGetAdmActForOblign.Import();
    var useExport = new LeCabGetAdmActForOblign.Export();

    useImport.ListOptManualAutoActs.OneChar =
      import.ListOptAutoManualActs.OneChar;
    useImport.StartDate.Date = import.StartDate.Date;
    useImport.Required.Type1 = import.Required.Type1;
    useImport.Obligor.Number = import.CsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      entities.ExistingObligation.SystemGeneratedIdentifier;

    Call(LeCabGetAdmActForOblign.Execute, useImport, useExport);

    useExport.Export1.CopyTo(local.AdminActions, MoveExport1ToAdminActions);
    export.TotalNoOfEntries.Count = useExport.TotalNoOfEntries.Count;
  }

  private bool ReadAdministrativeAction()
  {
    entities.AdministrativeAction.Populated = false;

    return Read("ReadAdministrativeAction",
      (db, command) =>
      {
        db.SetString(command, "type", import.Required.Type1);
      },
      (db, reader) =>
      {
        entities.AdministrativeAction.Type1 = db.GetString(reader, 0);
        entities.AdministrativeAction.Indicator = db.GetString(reader, 1);
        entities.AdministrativeAction.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.ExistingObligorCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingObligorCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingObligorCsePerson.Populated = true;
      });
  }

  private bool ReadCsePersonAccount()
  {
    entities.ExistingObligorCsePersonAccount.Populated = false;

    return Read("ReadCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingObligorCsePersonAccount.CspNumber =
          db.GetString(reader, 0);
        entities.ExistingObligorCsePersonAccount.Type1 =
          db.GetString(reader, 1);
        entities.ExistingObligorCsePersonAccount.Populated = true;
        CheckValid<CsePersonAccount>("Type1",
          entities.ExistingObligorCsePersonAccount.Type1);
      });
  }

  private bool ReadObligation()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingObligorCsePersonAccount.Populated);
    entities.ExistingObligation.Populated = false;

    return Read("ReadObligation",
      (db, command) =>
      {
        db.SetString(
          command, "cpaType", entities.ExistingObligorCsePersonAccount.Type1);
        db.SetString(
          command, "cspNumber",
          entities.ExistingObligorCsePersonAccount.CspNumber);
        db.
          SetInt32(command, "obId", import.Obligation.SystemGeneratedIdentifier);
          
      },
      (db, reader) =>
      {
        entities.ExistingObligation.CpaType = db.GetString(reader, 0);
        entities.ExistingObligation.CspNumber = db.GetString(reader, 1);
        entities.ExistingObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ExistingObligation.Description =
          db.GetNullableString(reader, 4);
        entities.ExistingObligation.AsOfDtNadArrBal =
          db.GetNullableDecimal(reader, 5);
        entities.ExistingObligation.AsOfDtAdcArrBal =
          db.GetNullableDecimal(reader, 6);
        entities.ExistingObligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.ExistingObligation.CpaType);
      });
  }

  private bool ReadObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);
    entities.ExistingObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId", entities.ExistingObligation.DtyGeneratedId);
      },
      (db, reader) =>
      {
        entities.ExistingObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingObligationType.Code = db.GetString(reader, 1);
        entities.ExistingObligationType.Name = db.GetString(reader, 2);
        entities.ExistingObligationType.Populated = true;
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
    /// A value of ListOptAutoManualActs.
    /// </summary>
    [JsonPropertyName("listOptAutoManualActs")]
    public Standard ListOptAutoManualActs
    {
      get => listOptAutoManualActs ??= new();
      set => listOptAutoManualActs = value;
    }

    /// <summary>
    /// A value of Required.
    /// </summary>
    [JsonPropertyName("required")]
    public AdministrativeAction Required
    {
      get => required ??= new();
      set => required = value;
    }

    /// <summary>
    /// A value of StartDate.
    /// </summary>
    [JsonPropertyName("startDate")]
    public DateWorkArea StartDate
    {
      get => startDate ??= new();
      set => startDate = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private Standard listOptAutoManualActs;
    private AdministrativeAction required;
    private DateWorkArea startDate;
    private Obligation obligation;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A AdminActionsGroup group.</summary>
    [Serializable]
    public class AdminActionsGroup
    {
      /// <summary>
      /// A value of DetailCertified.
      /// </summary>
      [JsonPropertyName("detailCertified")]
      public Common DetailCertified
      {
        get => detailCertified ??= new();
        set => detailCertified = value;
      }

      /// <summary>
      /// A value of DetailObligor.
      /// </summary>
      [JsonPropertyName("detailObligor")]
      public CsePerson DetailObligor
      {
        get => detailObligor ??= new();
        set => detailObligor = value;
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
      /// A value of DetailAdministrativeAction.
      /// </summary>
      [JsonPropertyName("detailAdministrativeAction")]
      public AdministrativeAction DetailAdministrativeAction
      {
        get => detailAdministrativeAction ??= new();
        set => detailAdministrativeAction = value;
      }

      /// <summary>
      /// A value of DetailAdministrativeActCertification.
      /// </summary>
      [JsonPropertyName("detailAdministrativeActCertification")]
      public AdministrativeActCertification DetailAdministrativeActCertification
      {
        get => detailAdministrativeActCertification ??= new();
        set => detailAdministrativeActCertification = value;
      }

      /// <summary>
      /// A value of DetailFederalDebtSetoff.
      /// </summary>
      [JsonPropertyName("detailFederalDebtSetoff")]
      public AdministrativeActCertification DetailFederalDebtSetoff
      {
        get => detailFederalDebtSetoff ??= new();
        set => detailFederalDebtSetoff = value;
      }

      /// <summary>
      /// A value of DetailObligationAdministrativeAction.
      /// </summary>
      [JsonPropertyName("detailObligationAdministrativeAction")]
      public ObligationAdministrativeAction DetailObligationAdministrativeAction
      {
        get => detailObligationAdministrativeAction ??= new();
        set => detailObligationAdministrativeAction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common detailCertified;
      private CsePerson detailObligor;
      private Obligation detailObligation;
      private AdministrativeAction detailAdministrativeAction;
      private AdministrativeActCertification detailAdministrativeActCertification;
        
      private AdministrativeActCertification detailFederalDebtSetoff;
      private ObligationAdministrativeAction detailObligationAdministrativeAction;
        
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
    /// A value of TotalNoOfEntries.
    /// </summary>
    [JsonPropertyName("totalNoOfEntries")]
    public Common TotalNoOfEntries
    {
      get => totalNoOfEntries ??= new();
      set => totalNoOfEntries = value;
    }

    /// <summary>
    /// Gets a value of AdminActions.
    /// </summary>
    [JsonIgnore]
    public Array<AdminActionsGroup> AdminActions => adminActions ??= new(
      AdminActionsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of AdminActions for json serialization.
    /// </summary>
    [JsonPropertyName("adminActions")]
    [Computed]
    public IList<AdminActionsGroup> AdminActions_Json
    {
      get => adminActions;
      set => AdminActions.Assign(value);
    }

    private ObligationType obligationType;
    private Obligation obligation;
    private Common totalNoOfEntries;
    private Array<AdminActionsGroup> adminActions;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A AdminActionsGroup group.</summary>
    [Serializable]
    public class AdminActionsGroup
    {
      /// <summary>
      /// A value of DetailCertified.
      /// </summary>
      [JsonPropertyName("detailCertified")]
      public Common DetailCertified
      {
        get => detailCertified ??= new();
        set => detailCertified = value;
      }

      /// <summary>
      /// A value of DetailAdministrativeActCertification.
      /// </summary>
      [JsonPropertyName("detailAdministrativeActCertification")]
      public AdministrativeActCertification DetailAdministrativeActCertification
      {
        get => detailAdministrativeActCertification ??= new();
        set => detailAdministrativeActCertification = value;
      }

      /// <summary>
      /// A value of DetailFederalDebtSetoff.
      /// </summary>
      [JsonPropertyName("detailFederalDebtSetoff")]
      public AdministrativeActCertification DetailFederalDebtSetoff
      {
        get => detailFederalDebtSetoff ??= new();
        set => detailFederalDebtSetoff = value;
      }

      /// <summary>
      /// A value of DetailObligationAdministrativeAction.
      /// </summary>
      [JsonPropertyName("detailObligationAdministrativeAction")]
      public ObligationAdministrativeAction DetailObligationAdministrativeAction
      {
        get => detailObligationAdministrativeAction ??= new();
        set => detailObligationAdministrativeAction = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common detailCertified;
      private AdministrativeActCertification detailAdministrativeActCertification;
        
      private AdministrativeActCertification detailFederalDebtSetoff;
      private ObligationAdministrativeAction detailObligationAdministrativeAction;
        
      private AdministrativeAction detailAdministrativeAction;
    }

    /// <summary>
    /// Gets a value of AdminActions.
    /// </summary>
    [JsonIgnore]
    public Array<AdminActionsGroup> AdminActions => adminActions ??= new(
      AdminActionsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of AdminActions for json serialization.
    /// </summary>
    [JsonPropertyName("adminActions")]
    [Computed]
    public IList<AdminActionsGroup> AdminActions_Json
    {
      get => adminActions;
      set => AdminActions.Assign(value);
    }

    private Array<AdminActionsGroup> adminActions;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingObligationType.
    /// </summary>
    [JsonPropertyName("existingObligationType")]
    public ObligationType ExistingObligationType
    {
      get => existingObligationType ??= new();
      set => existingObligationType = value;
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
    /// A value of ExistingObligorCsePersonAccount.
    /// </summary>
    [JsonPropertyName("existingObligorCsePersonAccount")]
    public CsePersonAccount ExistingObligorCsePersonAccount
    {
      get => existingObligorCsePersonAccount ??= new();
      set => existingObligorCsePersonAccount = value;
    }

    /// <summary>
    /// A value of ExistingObligorCsePerson.
    /// </summary>
    [JsonPropertyName("existingObligorCsePerson")]
    public CsePerson ExistingObligorCsePerson
    {
      get => existingObligorCsePerson ??= new();
      set => existingObligorCsePerson = value;
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

    private ObligationType existingObligationType;
    private Obligation existingObligation;
    private CsePersonAccount existingObligorCsePersonAccount;
    private CsePerson existingObligorCsePerson;
    private AdministrativeAction administrativeAction;
  }
#endregion
}
