// Program: FN_CHECK_FOR_SECONDARY_OBLIG, ID: 372279906, model: 746.
// Short name: SWE02319
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CHECK_FOR_SECONDARY_OBLIG.
/// </summary>
[Serializable]
public partial class FnCheckForSecondaryOblig: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CHECK_FOR_SECONDARY_OBLIG program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCheckForSecondaryOblig(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCheckForSecondaryOblig.
  /// </summary>
  public FnCheckForSecondaryOblig(IContext context, Import import, Export export)
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
    // : Check to see if the CRD Court Order is in support of any Secondary 
    // Obligations with a zero balance due.
    if (!ReadObligation3())
    {
      return;
    }

    if (ReadObligationRln1())
    {
      if (!ReadObligation2())
      {
        ExitState = "FN0000_OBLIGATION_NF_RB";

        return;
      }
    }
    else if (ReadObligationRln2())
    {
      if (!ReadObligation1())
      {
        ExitState = "FN0000_OBLIGATION_NF_RB";

        return;
      }
    }
    else
    {
      ExitState = "FN0000_OBLIG_RLN_NF_RB";

      return;
    }

    if (ReadLegalAction())
    {
      export.Group.Index = 0;
      export.Group.CheckSize();

      export.Group.Update.Collection.Amount =
        import.AmtToDistribute.TotalCurrency;
      export.Group.Update.Collection.CourtOrderAppliedTo =
        entities.ExistingPrimaryLegalAction.StandardNumber;
    }
    else
    {
      ExitState = "LEGAL_ACTION_NF_RB";
    }
  }

  private bool ReadLegalAction()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingPrimaryObligation.Populated);
    entities.ExistingPrimaryLegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.ExistingPrimaryObligation.LgaId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingPrimaryLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingPrimaryLegalAction.StandardNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingPrimaryLegalAction.Populated = true;
      });
  }

  private bool ReadObligation1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligationRln.Populated);
    entities.ExistingPrimaryObligation.Populated = false;

    return Read("ReadObligation1",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtyGeneratedId", entities.ExistingObligationRln.OtyFirstId);
          
        db.SetInt32(
          command, "obId", entities.ExistingObligationRln.ObgFGeneratedId);
        db.SetString(
          command, "cspNumber", entities.ExistingObligationRln.CspFNumber);
        db.
          SetString(command, "cpaType", entities.ExistingObligationRln.CpaFType);
          
      },
      (db, reader) =>
      {
        entities.ExistingPrimaryObligation.CpaType = db.GetString(reader, 0);
        entities.ExistingPrimaryObligation.CspNumber = db.GetString(reader, 1);
        entities.ExistingPrimaryObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingPrimaryObligation.DtyGeneratedId =
          db.GetInt32(reader, 3);
        entities.ExistingPrimaryObligation.LgaId =
          db.GetNullableInt32(reader, 4);
        entities.ExistingPrimaryObligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 5);
        entities.ExistingPrimaryObligation.Populated = true;
        CheckValid<Obligation>("CpaType",
          entities.ExistingPrimaryObligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.ExistingPrimaryObligation.PrimarySecondaryCode);
      });
  }

  private bool ReadObligation2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligationRln.Populated);
    entities.ExistingPrimaryObligation.Populated = false;

    return Read("ReadObligation2",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtyGeneratedId",
          entities.ExistingObligationRln.OtySecondId);
        db.SetInt32(
          command, "obId", entities.ExistingObligationRln.ObgGeneratedId);
        db.SetString(
          command, "cspNumber", entities.ExistingObligationRln.CspNumber);
        db.
          SetString(command, "cpaType", entities.ExistingObligationRln.CpaType);
          
      },
      (db, reader) =>
      {
        entities.ExistingPrimaryObligation.CpaType = db.GetString(reader, 0);
        entities.ExistingPrimaryObligation.CspNumber = db.GetString(reader, 1);
        entities.ExistingPrimaryObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingPrimaryObligation.DtyGeneratedId =
          db.GetInt32(reader, 3);
        entities.ExistingPrimaryObligation.LgaId =
          db.GetNullableInt32(reader, 4);
        entities.ExistingPrimaryObligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 5);
        entities.ExistingPrimaryObligation.Populated = true;
        CheckValid<Obligation>("CpaType",
          entities.ExistingPrimaryObligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.ExistingPrimaryObligation.PrimarySecondaryCode);
      });
  }

  private bool ReadObligation3()
  {
    entities.ExistingSecondaryObligation.Populated = false;

    return Read("ReadObligation3",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", import.Persistant.ObligorPersonNumber ?? "");
        db.SetNullableString(
          command, "standardNo", import.Persistant.CourtOrderNumber ?? "");
        db.SetNullableString(
          command, "primSecCd",
          import.HardcodedSecondary.PrimarySecondaryCode ?? "");
        db.SetNullableDate(
          command, "retiredDt", local.NullDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingSecondaryObligation.CpaType = db.GetString(reader, 0);
        entities.ExistingSecondaryObligation.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingSecondaryObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingSecondaryObligation.DtyGeneratedId =
          db.GetInt32(reader, 3);
        entities.ExistingSecondaryObligation.LgaId =
          db.GetNullableInt32(reader, 4);
        entities.ExistingSecondaryObligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 5);
        entities.ExistingSecondaryObligation.Populated = true;
        CheckValid<Obligation>("CpaType",
          entities.ExistingSecondaryObligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.ExistingSecondaryObligation.PrimarySecondaryCode);
      });
  }

  private bool ReadObligationRln1()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingSecondaryObligation.Populated);
    entities.ExistingObligationRln.Populated = false;

    return Read("ReadObligationRln1",
      (db, command) =>
      {
        db.SetInt32(
          command, "orrGeneratedId",
          import.HardcodedPriSec.SequentialGeneratedIdentifier);
        db.SetInt32(
          command, "otyFirstId",
          entities.ExistingSecondaryObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgFGeneratedId",
          entities.ExistingSecondaryObligation.SystemGeneratedIdentifier);
        db.SetString(
          command, "cspFNumber",
          entities.ExistingSecondaryObligation.CspNumber);
        db.SetString(
          command, "cpaFType", entities.ExistingSecondaryObligation.CpaType);
      },
      (db, reader) =>
      {
        entities.ExistingObligationRln.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingObligationRln.CspNumber = db.GetString(reader, 1);
        entities.ExistingObligationRln.CpaType = db.GetString(reader, 2);
        entities.ExistingObligationRln.ObgFGeneratedId = db.GetInt32(reader, 3);
        entities.ExistingObligationRln.CspFNumber = db.GetString(reader, 4);
        entities.ExistingObligationRln.CpaFType = db.GetString(reader, 5);
        entities.ExistingObligationRln.OrrGeneratedId = db.GetInt32(reader, 6);
        entities.ExistingObligationRln.CreatedBy = db.GetString(reader, 7);
        entities.ExistingObligationRln.OtySecondId = db.GetInt32(reader, 8);
        entities.ExistingObligationRln.OtyFirstId = db.GetInt32(reader, 9);
        entities.ExistingObligationRln.Populated = true;
        CheckValid<ObligationRln>("CpaType",
          entities.ExistingObligationRln.CpaType);
        CheckValid<ObligationRln>("CpaFType",
          entities.ExistingObligationRln.CpaFType);
      });
  }

  private bool ReadObligationRln2()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingSecondaryObligation.Populated);
    entities.ExistingObligationRln.Populated = false;

    return Read("ReadObligationRln2",
      (db, command) =>
      {
        db.SetInt32(
          command, "orrGeneratedId",
          import.HardcodedPriSec.SequentialGeneratedIdentifier);
        db.SetInt32(
          command, "otySecondId",
          entities.ExistingSecondaryObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ExistingSecondaryObligation.SystemGeneratedIdentifier);
        db.SetString(
          command, "cspNumber", entities.ExistingSecondaryObligation.CspNumber);
          
        db.SetString(
          command, "cpaType", entities.ExistingSecondaryObligation.CpaType);
      },
      (db, reader) =>
      {
        entities.ExistingObligationRln.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingObligationRln.CspNumber = db.GetString(reader, 1);
        entities.ExistingObligationRln.CpaType = db.GetString(reader, 2);
        entities.ExistingObligationRln.ObgFGeneratedId = db.GetInt32(reader, 3);
        entities.ExistingObligationRln.CspFNumber = db.GetString(reader, 4);
        entities.ExistingObligationRln.CpaFType = db.GetString(reader, 5);
        entities.ExistingObligationRln.OrrGeneratedId = db.GetInt32(reader, 6);
        entities.ExistingObligationRln.CreatedBy = db.GetString(reader, 7);
        entities.ExistingObligationRln.OtySecondId = db.GetInt32(reader, 8);
        entities.ExistingObligationRln.OtyFirstId = db.GetInt32(reader, 9);
        entities.ExistingObligationRln.Populated = true;
        CheckValid<ObligationRln>("CpaType",
          entities.ExistingObligationRln.CpaType);
        CheckValid<ObligationRln>("CpaFType",
          entities.ExistingObligationRln.CpaFType);
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
    /// A value of Persistant.
    /// </summary>
    [JsonPropertyName("persistant")]
    public CashReceiptDetail Persistant
    {
      get => persistant ??= new();
      set => persistant = value;
    }

    /// <summary>
    /// A value of AmtToDistribute.
    /// </summary>
    [JsonPropertyName("amtToDistribute")]
    public Common AmtToDistribute
    {
      get => amtToDistribute ??= new();
      set => amtToDistribute = value;
    }

    /// <summary>
    /// A value of HardcodedPriSec.
    /// </summary>
    [JsonPropertyName("hardcodedPriSec")]
    public ObligationRlnRsn HardcodedPriSec
    {
      get => hardcodedPriSec ??= new();
      set => hardcodedPriSec = value;
    }

    /// <summary>
    /// A value of HardcodedSecondary.
    /// </summary>
    [JsonPropertyName("hardcodedSecondary")]
    public Obligation HardcodedSecondary
    {
      get => hardcodedSecondary ??= new();
      set => hardcodedSecondary = value;
    }

    private CashReceiptDetail persistant;
    private Common amtToDistribute;
    private ObligationRlnRsn hardcodedPriSec;
    private Obligation hardcodedSecondary;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of Collection.
      /// </summary>
      [JsonPropertyName("collection")]
      public Collection Collection
      {
        get => collection ??= new();
        set => collection = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1000;

      private Collection collection;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    private Array<GroupGroup> group;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public DateWorkArea NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
    }

    private DateWorkArea nullDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
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
    /// A value of ExistingObligor1.
    /// </summary>
    [JsonPropertyName("existingObligor1")]
    public CsePerson ExistingObligor1
    {
      get => existingObligor1 ??= new();
      set => existingObligor1 = value;
    }

    /// <summary>
    /// A value of ExistingObligor2.
    /// </summary>
    [JsonPropertyName("existingObligor2")]
    public CsePersonAccount ExistingObligor2
    {
      get => existingObligor2 ??= new();
      set => existingObligor2 = value;
    }

    /// <summary>
    /// A value of ExistingPrimaryObligation.
    /// </summary>
    [JsonPropertyName("existingPrimaryObligation")]
    public Obligation ExistingPrimaryObligation
    {
      get => existingPrimaryObligation ??= new();
      set => existingPrimaryObligation = value;
    }

    /// <summary>
    /// A value of ExistingPrimaryLegalAction.
    /// </summary>
    [JsonPropertyName("existingPrimaryLegalAction")]
    public LegalAction ExistingPrimaryLegalAction
    {
      get => existingPrimaryLegalAction ??= new();
      set => existingPrimaryLegalAction = value;
    }

    /// <summary>
    /// A value of ExistingSecondaryObligation.
    /// </summary>
    [JsonPropertyName("existingSecondaryObligation")]
    public Obligation ExistingSecondaryObligation
    {
      get => existingSecondaryObligation ??= new();
      set => existingSecondaryObligation = value;
    }

    /// <summary>
    /// A value of ExistingSecondaryLegalAction.
    /// </summary>
    [JsonPropertyName("existingSecondaryLegalAction")]
    public LegalAction ExistingSecondaryLegalAction
    {
      get => existingSecondaryLegalAction ??= new();
      set => existingSecondaryLegalAction = value;
    }

    /// <summary>
    /// A value of ExistingObligationRlnRsn.
    /// </summary>
    [JsonPropertyName("existingObligationRlnRsn")]
    public ObligationRlnRsn ExistingObligationRlnRsn
    {
      get => existingObligationRlnRsn ??= new();
      set => existingObligationRlnRsn = value;
    }

    /// <summary>
    /// A value of ExistingObligationRln.
    /// </summary>
    [JsonPropertyName("existingObligationRln")]
    public ObligationRln ExistingObligationRln
    {
      get => existingObligationRln ??= new();
      set => existingObligationRln = value;
    }

    private ObligationTransaction debt;
    private DebtDetail debtDetail;
    private CsePerson existingObligor1;
    private CsePersonAccount existingObligor2;
    private Obligation existingPrimaryObligation;
    private LegalAction existingPrimaryLegalAction;
    private Obligation existingSecondaryObligation;
    private LegalAction existingSecondaryLegalAction;
    private ObligationRlnRsn existingObligationRlnRsn;
    private ObligationRln existingObligationRln;
  }
#endregion
}
