// Program: LE_CAB_RELATED_CSE_CASES_F_LACT, ID: 371998948, model: 746.
// Short name: SWE01669
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
/// A program: LE_CAB_RELATED_CSE_CASES_F_LACT.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This common action block returns related cse cases for a given legal action
/// </para>
/// </summary>
[Serializable]
public partial class LeCabRelatedCseCasesFLact: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_CAB_RELATED_CSE_CASES_F_LACT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeCabRelatedCseCasesFLact(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeCabRelatedCseCasesFLact.
  /// </summary>
  public LeCabRelatedCseCasesFLact(IContext context, Import import,
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
    // 12/21/1998    P. Sharp      Removed read of legal action and made 
    // identifier part of the qualifier of the read each of case.  Removed the
    // active case role flag from read each and removed import view. Not used in
    // this action block
    // 11/17/1999    Anand Katuri    PR# H00078496  Read Case_Role information 
    // without Start or End Dates
    local.Current.Date = Now().Date;
    local.Previous.Number = "";
    export.RelatedCseCases.Index = -1;

    // PR# H00078496: Comment out the Read Each and introduce a Read Each to 
    // read Case_Role information without Start or End Dates. - Anand
    foreach(var item in ReadCase())
    {
      if (export.RelatedCseCases.Index + 1 >= Export
        .RelatedCseCasesGroup.Capacity)
      {
        return;
      }

      if (Equal(entities.Case1.Number, local.Previous.Number))
      {
        // --- already moved to export
        continue;
      }

      ++export.RelatedCseCases.Index;
      export.RelatedCseCases.CheckSize();

      export.RelatedCseCases.Update.DetailRelated.Number =
        entities.Case1.Number;
      local.Previous.Number = entities.Case1.Number;
    }
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetInt32(command, "lgaId", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;

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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    private LegalAction legalAction;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A RelatedCseCasesGroup group.</summary>
    [Serializable]
    public class RelatedCseCasesGroup
    {
      /// <summary>
      /// A value of DetailRelated.
      /// </summary>
      [JsonPropertyName("detailRelated")]
      public Case1 DetailRelated
      {
        get => detailRelated ??= new();
        set => detailRelated = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Case1 detailRelated;
    }

    /// <summary>
    /// Gets a value of RelatedCseCases.
    /// </summary>
    [JsonIgnore]
    public Array<RelatedCseCasesGroup> RelatedCseCases =>
      relatedCseCases ??= new(RelatedCseCasesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of RelatedCseCases for json serialization.
    /// </summary>
    [JsonPropertyName("relatedCseCases")]
    [Computed]
    public IList<RelatedCseCasesGroup> RelatedCseCases_Json
    {
      get => relatedCseCases;
      set => RelatedCseCases.Assign(value);
    }

    private Array<RelatedCseCasesGroup> relatedCseCases;
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
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Case1 Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    private DateWorkArea current;
    private Case1 previous;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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

    private CaseRole caseRole;
    private LegalActionCaseRole legalActionCaseRole;
    private Case1 case1;
    private LegalAction legalAction;
  }
#endregion
}
