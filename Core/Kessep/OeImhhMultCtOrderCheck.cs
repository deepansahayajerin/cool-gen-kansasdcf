// Program: OE_IMHH_MULT_CT_ORDER_CHECK, ID: 374455688, model: 746.
// Short name: SWE02696
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_IMHH_MULT_CT_ORDER_CHECK.
/// </summary>
[Serializable]
public partial class OeImhhMultCtOrderCheck: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_IMHH_MULT_CT_ORDER_CHECK program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeImhhMultCtOrderCheck(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeImhhMultCtOrderCheck.
  /// </summary>
  public OeImhhMultCtOrderCheck(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ******** MAINTENANCE LOG ********************
    // AUTHOR    	 DATE  	  CHG REQ#   DESCRIPTION
    // Mike Fangman     05/00    PRWORA     New AB to get the court order & 
    // check for multiple court orders for a person and date range.
    // ******** END MAINTENANCE LOG ****************
    local.MultCtOrderMsg.Text30 = "Mult ct ord during time frame";

    // Check for multiple court orders when finishing the creation of a detail 
    // line.
    // Set up the date range for the comparison in a DB2 date format from a 
    // YYYYMM numerical format.
    local.From.Date = IntToDate(import.From.YearMonth * 100 + 1);
    local.To.Date = IntToDate(import.To.YearMonth * 100 + 1);
    local.To.Date = AddMonths(local.To.Date, 1);
    local.To.Date = AddDays(local.To.Date, -1);

    // Read for one or more court orders during the time frame.
    foreach(var item in ReadLegalActionLegalActionPerson())
    {
      if (IsEmpty(export.LegalAction.StandardNumber))
      {
        export.LegalAction.StandardNumber = entities.LegalAction.StandardNumber;
      }
      else if (!Equal(entities.LegalAction.StandardNumber,
        export.LegalAction.StandardNumber))
      {
        export.MultCtOrderMsg.Text30 = local.MultCtOrderMsg.Text30;

        return;
      }
    }
  }

  private IEnumerable<bool> ReadLegalActionLegalActionPerson()
  {
    entities.LegalAction.Populated = false;
    entities.LegalActionPerson.Populated = false;

    return ReadEach("ReadLegalActionLegalActionPerson",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", import.CsePerson.Number);
        db.SetDate(command, "effectiveDt", local.To.Date.GetValueOrDefault());
        db.
          SetNullableDate(command, "endDt", local.From.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 2);
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 3);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 4);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 5);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 6);
        entities.LegalAction.Populated = true;
        entities.LegalActionPerson.Populated = true;

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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    public DateWorkArea From
    {
      get => from ??= new();
      set => from = value;
    }

    /// <summary>
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public DateWorkArea To
    {
      get => to ??= new();
      set => to = value;
    }

    private CsePerson csePerson;
    private DateWorkArea from;
    private DateWorkArea to;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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
    /// A value of MultCtOrderMsg.
    /// </summary>
    [JsonPropertyName("multCtOrderMsg")]
    public TextWorkArea MultCtOrderMsg
    {
      get => multCtOrderMsg ??= new();
      set => multCtOrderMsg = value;
    }

    private LegalAction legalAction;
    private TextWorkArea multCtOrderMsg;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    public DateWorkArea From
    {
      get => from ??= new();
      set => from = value;
    }

    /// <summary>
    /// A value of Year.
    /// </summary>
    [JsonPropertyName("year")]
    public NumericWorkSet Year
    {
      get => year ??= new();
      set => year = value;
    }

    /// <summary>
    /// A value of Month.
    /// </summary>
    [JsonPropertyName("month")]
    public NumericWorkSet Month
    {
      get => month ??= new();
      set => month = value;
    }

    /// <summary>
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public DateWorkArea To
    {
      get => to ??= new();
      set => to = value;
    }

    /// <summary>
    /// A value of MultCtOrderMsg.
    /// </summary>
    [JsonPropertyName("multCtOrderMsg")]
    public TextWorkArea MultCtOrderMsg
    {
      get => multCtOrderMsg ??= new();
      set => multCtOrderMsg = value;
    }

    private DateWorkArea from;
    private NumericWorkSet year;
    private NumericWorkSet month;
    private DateWorkArea to;
    private TextWorkArea multCtOrderMsg;
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

    private LegalAction legalAction;
    private LegalActionPerson legalActionPerson;
    private CsePerson csePerson;
  }
#endregion
}
