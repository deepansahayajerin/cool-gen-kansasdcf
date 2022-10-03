// Program: SP_CAB_IND_SUPPORT_AND_JUGMNT, ID: 372645316, model: 746.
// Short name: SWE01883
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
/// A program: SP_CAB_IND_SUPPORT_AND_JUGMNT.
/// </para>
/// <para>
///   Mainly this cab exists in order to set flags for the Case Review 
/// Establishment screen, CRES.  Should these codes change then all that would
/// need to be changed and gened is this cab.
/// RVW 1/8/97
/// </para>
/// </summary>
[Serializable]
public partial class SpCabIndSupportAndJugmnt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_IND_SUPPORT_AND_JUGMNT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabIndSupportAndJugmnt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabIndSupportAndJugmnt.
  /// </summary>
  public SpCabIndSupportAndJugmnt(IContext context, Import import, Export export)
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
    // ********************************************
    // *  04/30/97 R. Grey		Change Current Date
    // ********************************************
    local.Current.Date = Now().Date;

    if (ReadLaDetNonfinancial())
    {
      if (ReadLegalActionPerson2())
      {
        if (Equal(entities.LaDetNonfinancial.NonFinOblgType, "HIC"))
        {
          export.MedicalSupportChild.Flag = "Y";
        }

        if (Equal(entities.LaDetNonfinancial.NonFinOblgType, "UM"))
        {
          export.JudgementChild.Flag = "Y";
        }
      }
    }

    foreach(var item in ReadLegalActionDetailLegalAction())
    {
      if (ReadLegalActionPerson1())
      {
        if (ReadObligationType())
        {
          switch(TrimEnd(entities.ObligationType.Code))
          {
            case "CRCH":
              export.JudgementChild.Flag = "Y";

              break;
            case "718B":
              export.JudgementChild.Flag = "Y";

              break;
            case "MS":
              export.MedicalSupportChild.Flag = "Y";

              break;
            case "MC":
              export.MedicalSupportChild.Flag = "Y";

              break;
            case "MJ":
              export.JudgementChild.Flag = "Y";

              break;
            case "CS":
              export.CurrentSupportChild.Flag = "Y";

              break;
            case "AJ":
              export.JudgementChild.Flag = "Y";

              break;
            default:
              break;
          }
        }
      }
    }
  }

  private bool ReadLaDetNonfinancial()
  {
    entities.LaDetNonfinancial.Populated = false;

    return Read("ReadLaDetNonfinancial",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", import.Child.Number);
        db.SetNullableDate(
          command, "endDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LaDetNonfinancial.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LaDetNonfinancial.Number = db.GetInt32(reader, 1);
        entities.LaDetNonfinancial.EndDate = db.GetNullableDate(reader, 2);
        entities.LaDetNonfinancial.EffectiveDate = db.GetDate(reader, 3);
        entities.LaDetNonfinancial.NonFinOblgType =
          db.GetNullableString(reader, 4);
        entities.LaDetNonfinancial.DetailType = db.GetString(reader, 5);
        entities.LaDetNonfinancial.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LaDetNonfinancial.DetailType);
      });
  }

  private IEnumerable<bool> ReadLegalActionDetailLegalAction()
  {
    entities.LegalActionDetail.Populated = false;
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalActionDetailLegalAction",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
        db.SetNullableString(command, "cspNumber", import.Child.Number);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail.JudgementAmount =
          db.GetNullableDecimal(reader, 4);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 5);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 6);
        entities.LegalActionDetail.Populated = true;
        entities.LegalAction.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);

        return true;
      });
  }

  private bool ReadLegalActionPerson1()
  {
    entities.LegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.SetNullableString(command, "cspNumber", import.Ap.Number);
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.LegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionPerson.Role = db.GetString(reader, 4);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 5);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 6);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 7);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 8);
        entities.LegalActionPerson.Populated = true;
      });
  }

  private bool ReadLegalActionPerson2()
  {
    System.Diagnostics.Debug.Assert(entities.LaDetNonfinancial.Populated);
    entities.LegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.LaDetNonfinancial.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LaDetNonfinancial.LgaIdentifier);
        db.SetNullableString(command, "cspNumber", import.Ap.Number);
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.LegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionPerson.Role = db.GetString(reader, 4);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 5);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 6);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 7);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 8);
        entities.LegalActionPerson.Populated = true;
      });
  }

  private bool ReadObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId",
          entities.LegalActionDetail.OtyId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.EffectiveDt = db.GetDate(reader, 2);
        entities.ObligationType.DiscontinueDt = db.GetNullableDate(reader, 3);
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CsePerson Child
    {
      get => child ??= new();
      set => child = value;
    }

    private CsePersonsWorkSet ap;
    private CsePerson child;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of JudgementChild.
    /// </summary>
    [JsonPropertyName("judgementChild")]
    public Common JudgementChild
    {
      get => judgementChild ??= new();
      set => judgementChild = value;
    }

    /// <summary>
    /// A value of CurrentSupportChild.
    /// </summary>
    [JsonPropertyName("currentSupportChild")]
    public Common CurrentSupportChild
    {
      get => currentSupportChild ??= new();
      set => currentSupportChild = value;
    }

    /// <summary>
    /// A value of MedicalSupportChild.
    /// </summary>
    [JsonPropertyName("medicalSupportChild")]
    public Common MedicalSupportChild
    {
      get => medicalSupportChild ??= new();
      set => medicalSupportChild = value;
    }

    private Common judgementChild;
    private Common currentSupportChild;
    private Common medicalSupportChild;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private DateWorkArea current;
    private Common common;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of LaDetNonfinancial.
    /// </summary>
    [JsonPropertyName("laDetNonfinancial")]
    public LegalActionDetail LaDetNonfinancial
    {
      get => laDetNonfinancial ??= new();
      set => laDetNonfinancial = value;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
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

    private CsePerson csePerson;
    private CsePerson ap;
    private LegalActionDetail laDetNonfinancial;
    private ObligationType obligationType;
    private LegalActionDetail legalActionDetail;
    private LegalActionPerson legalActionPerson;
    private LegalAction legalAction;
  }
#endregion
}
