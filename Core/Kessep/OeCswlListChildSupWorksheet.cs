// Program: OE_CSWL_LIST_CHILD_SUP_WORKSHEET, ID: 371911907, model: 746.
// Short name: SWE00900
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
/// A program: OE_CSWL_LIST_CHILD_SUP_WORKSHEET.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// WRITTEN BY:SID
/// </para>
/// </summary>
[Serializable]
public partial class OeCswlListChildSupWorksheet: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_CSWL_LIST_CHILD_SUP_WORKSHEET program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeCswlListChildSupWorksheet(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeCswlListChildSupWorksheet.
  /// </summary>
  public OeCswlListChildSupWorksheet(IContext context, Import import,
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
    // ---------------------------------------------
    // Date      Author          Description
    // 02/27/95  Sid             Initial Creation
    // 03/05/08  M. Fan          CQ3073 (PR328961) Added 'created_timestamp' for
    //                           case person support worksheet entity view in 
    // group_export view
    //                           and entity actions.
    // ---------------------------------------------
    export.Case1.Number = import.Case1.Number;
    export.CsePerson.Number = import.CsePerson.Number;

    if (IsEmpty(import.CsePerson.Number))
    {
      ExitState = "CSE_PERSON_NO_REQUIRED";

      return;
    }

    if (ReadCsePerson())
    {
      export.CsePerson.Number = entities.CsePerson.Number;
      export.Name.Number = export.CsePerson.Number;
      UseSiReadCsePerson();
    }
    else
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    if (!IsEmpty(import.Case1.Number))
    {
      if (ReadCase())
      {
        if (!ReadCaseRole())
        {
          ExitState = "OE0000_CASE_MEMBER_NE";

          return;
        }
      }
      else
      {
        ExitState = "CASE_NF";

        return;
      }
    }

    if (IsEmpty(import.Case1.Number))
    {
      export.Export1.Index = 0;
      export.Export1.Clear();

      foreach(var item in ReadCaseRoleCaseCsePersonSupportWorksheetChildSupportWorksheet())
        
      {
        if (ReadLegalAction())
        {
          MoveLegalAction(entities.LegalAction,
            export.Export1.Update.DetailLegalAction);
        }

        export.Export1.Update.DetailCase.Number = entities.Case1.Number;
        export.Export1.Update.DetailCsePersonSupportWorksheet.Assign(
          entities.CsePersonSupportWorksheet);
        export.Export1.Update.DetailChildSupportWorksheet.Assign(
          entities.ChildSupportWorksheet);
        export.Export1.Update.Date.EffectiveDate =
          Date(entities.CsePersonSupportWorksheet.LastUpdatedTimestamp);
        export.Export1.Next();
      }
    }
    else
    {
      export.Export1.Index = 0;
      export.Export1.Clear();

      foreach(var item in ReadCaseRoleCsePersonSupportWorksheetChildSupportWorksheet())
        
      {
        if (ReadLegalAction())
        {
          MoveLegalAction(entities.LegalAction,
            export.Export1.Update.DetailLegalAction);
        }

        export.Export1.Update.DetailCase.Number = entities.Case1.Number;
        export.Export1.Update.DetailCsePersonSupportWorksheet.Assign(
          entities.CsePersonSupportWorksheet);
        export.Export1.Update.DetailChildSupportWorksheet.Assign(
          entities.ChildSupportWorksheet);
        export.Export1.Update.Date.EffectiveDate =
          Date(entities.CsePersonSupportWorksheet.LastUpdatedTimestamp);
        export.Export1.Next();
      }
    }

    if (export.Export1.IsEmpty)
    {
      ExitState = "CSE_PERSON_SUPPORT_WORKSHEET_NF";
    }
    else
    {
      ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
    }
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.Name.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.Name.Assign(useExport.CsePersonsWorkSet);
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseRole()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private IEnumerable<bool>
    ReadCaseRoleCaseCsePersonSupportWorksheetChildSupportWorksheet()
  {
    return ReadEach(
      "ReadCaseRoleCaseCsePersonSupportWorksheetChildSupportWorksheet",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CsePersonSupportWorksheet.CasNumber = db.GetString(reader, 0);
        entities.CsePersonSupportWorksheet.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CsePersonSupportWorksheet.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CsePersonSupportWorksheet.CroType = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CsePersonSupportWorksheet.CroIdentifier =
          db.GetInt32(reader, 3);
        entities.CsePersonSupportWorksheet.Identifer = db.GetInt32(reader, 4);
        entities.CsePersonSupportWorksheet.NetAdjParentalChildSuppAmt =
          db.GetNullableDecimal(reader, 5);
        entities.CsePersonSupportWorksheet.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.CsePersonSupportWorksheet.LastUpdatedBy =
          db.GetString(reader, 7);
        entities.CsePersonSupportWorksheet.LastUpdatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.CsePersonSupportWorksheet.CswIdentifier =
          db.GetInt64(reader, 9);
        entities.ChildSupportWorksheet.Identifier = db.GetInt64(reader, 9);
        entities.CsePersonSupportWorksheet.CssGuidelineYr =
          db.GetInt32(reader, 10);
        entities.ChildSupportWorksheet.CsGuidelineYear =
          db.GetInt32(reader, 10);
        entities.ChildSupportWorksheet.LgaIdentifier =
          db.GetNullableInt32(reader, 11);
        entities.ChildSupportWorksheet.Status = db.GetString(reader, 12);
        entities.ChildSupportWorksheet.Populated = true;
        entities.CsePersonSupportWorksheet.Populated = true;
        entities.CaseRole.Populated = true;
        entities.Case1.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePersonSupportWorksheet>("CroType",
          entities.CsePersonSupportWorksheet.CroType);

        return true;
      });
  }

  private IEnumerable<bool>
    ReadCaseRoleCsePersonSupportWorksheetChildSupportWorksheet()
  {
    return ReadEach(
      "ReadCaseRoleCsePersonSupportWorksheetChildSupportWorksheet",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CsePersonSupportWorksheet.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CsePersonSupportWorksheet.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CsePersonSupportWorksheet.CroType = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CsePersonSupportWorksheet.CroIdentifier =
          db.GetInt32(reader, 3);
        entities.CsePersonSupportWorksheet.Identifer = db.GetInt32(reader, 4);
        entities.CsePersonSupportWorksheet.NetAdjParentalChildSuppAmt =
          db.GetNullableDecimal(reader, 5);
        entities.CsePersonSupportWorksheet.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.CsePersonSupportWorksheet.LastUpdatedBy =
          db.GetString(reader, 7);
        entities.CsePersonSupportWorksheet.LastUpdatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.CsePersonSupportWorksheet.CswIdentifier =
          db.GetInt64(reader, 9);
        entities.ChildSupportWorksheet.Identifier = db.GetInt64(reader, 9);
        entities.CsePersonSupportWorksheet.CssGuidelineYr =
          db.GetInt32(reader, 10);
        entities.ChildSupportWorksheet.CsGuidelineYear =
          db.GetInt32(reader, 10);
        entities.ChildSupportWorksheet.LgaIdentifier =
          db.GetNullableInt32(reader, 11);
        entities.ChildSupportWorksheet.Status = db.GetString(reader, 12);
        entities.ChildSupportWorksheet.Populated = true;
        entities.CsePersonSupportWorksheet.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePersonSupportWorksheet>("CroType",
          entities.CsePersonSupportWorksheet.CroType);

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadLegalAction()
  {
    System.Diagnostics.Debug.Assert(entities.ChildSupportWorksheet.Populated);
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.ChildSupportWorksheet.LgaIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.Populated = true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private CsePerson csePerson;
    private Case1 case1;
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
      /// A value of Date.
      /// </summary>
      [JsonPropertyName("date")]
      public Code Date
      {
        get => date ??= new();
        set => date = value;
      }

      /// <summary>
      /// A value of DetailScrollingAttributes.
      /// </summary>
      [JsonPropertyName("detailScrollingAttributes")]
      public ScrollingAttributes DetailScrollingAttributes
      {
        get => detailScrollingAttributes ??= new();
        set => detailScrollingAttributes = value;
      }

      /// <summary>
      /// A value of DetailCase.
      /// </summary>
      [JsonPropertyName("detailCase")]
      public Case1 DetailCase
      {
        get => detailCase ??= new();
        set => detailCase = value;
      }

      /// <summary>
      /// A value of DetailLegalAction.
      /// </summary>
      [JsonPropertyName("detailLegalAction")]
      public LegalAction DetailLegalAction
      {
        get => detailLegalAction ??= new();
        set => detailLegalAction = value;
      }

      /// <summary>
      /// A value of DetailCsePersonSupportWorksheet.
      /// </summary>
      [JsonPropertyName("detailCsePersonSupportWorksheet")]
      public CsePersonSupportWorksheet DetailCsePersonSupportWorksheet
      {
        get => detailCsePersonSupportWorksheet ??= new();
        set => detailCsePersonSupportWorksheet = value;
      }

      /// <summary>
      /// A value of DetailChildSupportWorksheet.
      /// </summary>
      [JsonPropertyName("detailChildSupportWorksheet")]
      public ChildSupportWorksheet DetailChildSupportWorksheet
      {
        get => detailChildSupportWorksheet ??= new();
        set => detailChildSupportWorksheet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Code date;
      private ScrollingAttributes detailScrollingAttributes;
      private Case1 detailCase;
      private LegalAction detailLegalAction;
      private CsePersonSupportWorksheet detailCsePersonSupportWorksheet;
      private ChildSupportWorksheet detailChildSupportWorksheet;
    }

    /// <summary>
    /// A value of Name.
    /// </summary>
    [JsonPropertyName("name")]
    public CsePersonsWorkSet Name
    {
      get => name ??= new();
      set => name = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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

    private CsePersonsWorkSet name;
    private CsePerson csePerson;
    private Case1 case1;
    private Array<ExportGroup> export1;
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
    /// A value of ChildSupportWorksheet.
    /// </summary>
    [JsonPropertyName("childSupportWorksheet")]
    public ChildSupportWorksheet ChildSupportWorksheet
    {
      get => childSupportWorksheet ??= new();
      set => childSupportWorksheet = value;
    }

    /// <summary>
    /// A value of CsePersonSupportWorksheet.
    /// </summary>
    [JsonPropertyName("csePersonSupportWorksheet")]
    public CsePersonSupportWorksheet CsePersonSupportWorksheet
    {
      get => csePersonSupportWorksheet ??= new();
      set => csePersonSupportWorksheet = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
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

    private LegalAction legalAction;
    private ChildSupportWorksheet childSupportWorksheet;
    private CsePersonSupportWorksheet csePersonSupportWorksheet;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private Case1 case1;
  }
#endregion
}
