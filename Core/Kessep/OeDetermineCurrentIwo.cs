// Program: OE_DETERMINE_CURRENT_IWO, ID: 371387712, model: 746.
// Short name: SWE03614
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_DETERMINE_CURRENT_IWO.
/// </summary>
[Serializable]
public partial class OeDetermineCurrentIwo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_DETERMINE_CURRENT_IWO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeDetermineCurrentIwo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeDetermineCurrentIwo.
  /// </summary>
  public OeDetermineCurrentIwo(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // **********************************************************************************************
    // Initial Code       Dwayne Dupree        10/15/2007
    // This is determining if payments were made as defined in the driver's 
    // license restriction process.
    // **********************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.WageWithholdingFound.Count = 0;
    local.CreatedDate.Date = new DateTime(2099, 12, 31);
    local.MultiPayor.Flag = "";

    if (ReadCsePerson())
    {
      local.MultiPayor.Flag = "Y";
    }

    foreach(var item in ReadLegalAction())
    {
      local.CreatedDate.Date = Date(entities.LegalAction.CreatedTstamp);

      if (Lt(local.CreatedDate.Date, import.Import30DayLetterSentDate.Date))
      {
        break;
      }

      if (Equal(entities.LegalAction.ActionTaken, "IWOMODO") || Equal
        (entities.LegalAction.ActionTaken, "IWONOTKM") || Equal
        (entities.LegalAction.ActionTaken, "IWONOTKS") || Equal
        (entities.LegalAction.ActionTaken, "ORDIWO2") || Equal
        (entities.LegalAction.ActionTaken, "IWO") || Equal
        (entities.LegalAction.ActionTaken, "IWOMODM"))
      {
        if (AsChar(local.MultiPayor.Flag) == 'Y')
        {
          if (!ReadLegalActionPerson2())
          {
            if (ReadLegalActionPerson1())
            {
              // The IWO was for a different obligor. Skip this legal action
              continue;
            }
            else
            {
              // no oblligor is assigned to this legal action
              goto Test;
            }
          }
        }

        ++local.WageWithholdingFound.Count;
        export.CreatedDate.Date = local.CreatedDate.Date;
      }

Test:
      ;
    }

    if (local.WageWithholdingFound.Count > 0)
    {
      // there is a wage withholdiing in place
      export.WageWithholdingFound.Flag = "Y";
    }
  }

  private bool ReadCsePerson()
  {
    entities.Multipayor.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
        db.SetNullableString(
          command, "standardNo", import.LegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Multipayor.Number = db.GetString(reader, 0);
        entities.Multipayor.Type1 = db.GetString(reader, 1);
        entities.Multipayor.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Multipayor.Type1);
      });
  }

  private IEnumerable<bool> ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", import.LegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 5);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 6);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 7);
        entities.LegalAction.Populated = true;

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
          command, "lgaRIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 3);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 4);
        entities.LegalActionPerson.Populated = true;
      });
  }

  private bool ReadLegalActionPerson2()
  {
    entities.LegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalAction.Identifier);
        db.SetNullableString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 3);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 4);
        entities.LegalActionPerson.Populated = true;
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
    /// A value of Import30DayLetterSentDate.
    /// </summary>
    [JsonPropertyName("import30DayLetterSentDate")]
    public DateWorkArea Import30DayLetterSentDate
    {
      get => import30DayLetterSentDate ??= new();
      set => import30DayLetterSentDate = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private DateWorkArea import30DayLetterSentDate;
    private LegalAction legalAction;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CreatedDate.
    /// </summary>
    [JsonPropertyName("createdDate")]
    public DateWorkArea CreatedDate
    {
      get => createdDate ??= new();
      set => createdDate = value;
    }

    /// <summary>
    /// A value of WageWithholdingFound.
    /// </summary>
    [JsonPropertyName("wageWithholdingFound")]
    public Common WageWithholdingFound
    {
      get => wageWithholdingFound ??= new();
      set => wageWithholdingFound = value;
    }

    private DateWorkArea createdDate;
    private Common wageWithholdingFound;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of MultiPayor.
    /// </summary>
    [JsonPropertyName("multiPayor")]
    public Common MultiPayor
    {
      get => multiPayor ??= new();
      set => multiPayor = value;
    }

    /// <summary>
    /// A value of WageWithholdingFound.
    /// </summary>
    [JsonPropertyName("wageWithholdingFound")]
    public Common WageWithholdingFound
    {
      get => wageWithholdingFound ??= new();
      set => wageWithholdingFound = value;
    }

    /// <summary>
    /// A value of CreatedDate.
    /// </summary>
    [JsonPropertyName("createdDate")]
    public DateWorkArea CreatedDate
    {
      get => createdDate ??= new();
      set => createdDate = value;
    }

    private Common multiPayor;
    private Common wageWithholdingFound;
    private DateWorkArea createdDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Multipayor.
    /// </summary>
    [JsonPropertyName("multipayor")]
    public CsePerson Multipayor
    {
      get => multipayor ??= new();
      set => multipayor = value;
    }

    /// <summary>
    /// A value of AbsentParent.
    /// </summary>
    [JsonPropertyName("absentParent")]
    public CaseRole AbsentParent
    {
      get => absentParent ??= new();
      set => absentParent = value;
    }

    /// <summary>
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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

    private CsePerson multipayor;
    private CaseRole absentParent;
    private LegalActionCaseRole legalActionCaseRole;
    private Tribunal tribunal;
    private LegalActionDetail legalActionDetail;
    private LegalActionPerson legalActionPerson;
    private CsePerson csePerson;
    private LegalAction legalAction;
  }
#endregion
}
