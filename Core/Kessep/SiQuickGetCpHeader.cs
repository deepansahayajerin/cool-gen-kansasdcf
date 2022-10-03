// Program: SI_QUICK_GET_CP_HEADER, ID: 374537478, model: 746.
// Short name: SWE03100
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_QUICK_GET_CP_HEADER.
/// </para>
/// <para>
/// CP Header
/// </para>
/// </summary>
[Serializable]
public partial class SiQuickGetCpHeader: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_QUICK_GET_CP_HEADER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiQuickGetCpHeader(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiQuickGetCpHeader.
  /// </summary>
  public SiQuickGetCpHeader(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // Date		Developer	WR#		Description
    // ---------------------------------------------------------------------------------------
    // 06/18/2009	JHuss		211		Initial development.
    local.Current.Date = Now().Date;

    // ****************************************************************************
    // Interstate case numbers can have up to 15 characters.  Kansas case 
    // numbers
    // only have 10 characters.  If the case number sent for querying has any
    // characters in the first 5 locations then respond with a Case Not Found
    // message.  Also respond with a Case Not Found message if the case number
    // contains any non-numeric characters.
    // ****************************************************************************
    local.Leading.Text5 = Substring(import.QuickInQuery.CaseId, 1, 5);

    //--->Commented this code for testing purpose. The case id has only four zeros.
    //if (!IsEmpty(local.Leading.Text5) && !Equal(local.Leading.Text5, "00000"))
    //{
    //  ExitState = "CASE_NF";

    //  return;
    //}
    //<---Commented this code for testing purpose. The case id has only four zeros.

    local.Case1.Number = Substring(import.QuickInQuery.CaseId, 6, 10);
    UseCabZeroFillNumber();

    if (IsExitState("CASE_NUMBER_NOT_NUMERIC"))
    {
      ExitState = "CASE_NF";

      return;
    }

    if (ReadCase())
    {
      local.Case1.Assign(entities.Case1);
    }
    else
    {
      ExitState = "CASE_NF";

      return;
    }

    export.Case1.Number = local.Case1.Number;

    // ****************************************************************************
    // If family violence is found for any case participant on the case
    // then no data will be returned.  This includes all active and
    // inactive case participants.  This is true for open or closed cases.
    // ****************************************************************************
    if (ReadCaseRoleCsePerson2())
    {
      ExitState = "SC0000_DATA_NOT_DISPLAY_FOR_FV";

      return;
    }

    // ****************************************************************************
    // If the case is open then use the current date for querying.
    // Otherwise, choose the date prior to the day the case was closed.
    // ****************************************************************************
    if (AsChar(local.Case1.Status) == 'O')
    {
      local.CaseRoleDate.Date = local.Current.Date;
    }
    else
    {
      local.CaseRoleDate.Date = AddDays(local.Case1.StatusDate, -1);
    }

    // ****************************************************************************
    // Multiple active APs are possible.  Choose the AP that's active and
    // was added to the case first.
    // ****************************************************************************
    if (ReadCaseRoleCsePerson3())
    {
      export.QuickCpHeader.NcpPersonNumber = entities.CsePerson.Number;

      if (ReadCsePersonDetail())
      {
        export.QuickCpHeader.NcpFirstName = entities.CsePersonDetail.FirstName;
        export.QuickCpHeader.NcpMiddleInitial =
          entities.CsePersonDetail.MiddleInitial ?? Spaces(1);
        export.QuickCpHeader.NcpLastName = entities.CsePersonDetail.LastName;
      }
    }

    local.Temp.FirstName = "";
    local.Temp.MiddleInitial = "";
    local.Temp.LastName = "";

    if (ReadCaseRoleCsePerson1())
    {
      export.QuickCpHeader.CpTypeCode = entities.CsePerson.Type1;
      export.QuickCpHeader.CpPersonNumber = entities.CsePerson.Number;

      if (AsChar(entities.CsePerson.Type1) == 'O')
      {
        export.QuickCpHeader.CpOrganizationName =
          entities.CsePerson.OrganizationName ?? Spaces(33);
      }
      else if (ReadCsePersonDetail())
      {
        export.QuickCpHeader.CpFirstName = entities.CsePersonDetail.FirstName;
        export.QuickCpHeader.CpMiddleInitial =
          entities.CsePersonDetail.MiddleInitial ?? Spaces(1);
        export.QuickCpHeader.CpLastName = entities.CsePersonDetail.LastName;
      }
    }
  }

  private void UseCabZeroFillNumber()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.Case1.Number = local.Case1.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    local.Case1.Number = useImport.Case1.Number;
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", local.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseRoleCsePerson1()
  {
    entities.CaseRole.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadCaseRoleCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", local.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.CaseRoleDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.CsePerson.Type1 = db.GetString(reader, 7);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 8);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 9);
        entities.CaseRole.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCaseRoleCsePerson2()
  {
    entities.CaseRole.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadCaseRoleCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", local.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.CsePerson.Type1 = db.GetString(reader, 7);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 8);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 9);
        entities.CaseRole.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCaseRoleCsePerson3()
  {
    entities.CaseRole.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadCaseRoleCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", local.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.CaseRoleDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.CsePerson.Type1 = db.GetString(reader, 7);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 8);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 9);
        entities.CaseRole.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePersonDetail()
  {
    entities.CsePersonDetail.Populated = false;

    return Read("ReadCsePersonDetail",
      (db, command) =>
      {
        db.SetString(command, "personNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonDetail.PersonNumber = db.GetString(reader, 0);
        entities.CsePersonDetail.FirstName = db.GetString(reader, 1);
        entities.CsePersonDetail.LastName = db.GetString(reader, 2);
        entities.CsePersonDetail.MiddleInitial =
          db.GetNullableString(reader, 3);
        entities.CsePersonDetail.Populated = true;
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
    /// A value of QuickInQuery.
    /// </summary>
    [JsonPropertyName("quickInQuery")]
    public QuickInQuery QuickInQuery
    {
      get => quickInQuery ??= new();
      set => quickInQuery = value;
    }

    private QuickInQuery quickInQuery;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of QuickCpHeader.
    /// </summary>
    [JsonPropertyName("quickCpHeader")]
    public QuickCpHeader QuickCpHeader
    {
      get => quickCpHeader ??= new();
      set => quickCpHeader = value;
    }

    private Case1 case1;
    private QuickCpHeader quickCpHeader;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of CaseRoleDate.
    /// </summary>
    [JsonPropertyName("caseRoleDate")]
    public DateWorkArea CaseRoleDate
    {
      get => caseRoleDate ??= new();
      set => caseRoleDate = value;
    }

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
    /// A value of Input.
    /// </summary>
    [JsonPropertyName("input")]
    public CsePersonsWorkSet Input
    {
      get => input ??= new();
      set => input = value;
    }

    /// <summary>
    /// A value of Leading.
    /// </summary>
    [JsonPropertyName("leading")]
    public WorkArea Leading
    {
      get => leading ??= new();
      set => leading = value;
    }

    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public CsePersonsWorkSet Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    private Case1 case1;
    private DateWorkArea caseRoleDate;
    private DateWorkArea current;
    private CsePersonsWorkSet input;
    private WorkArea leading;
    private CsePersonsWorkSet temp;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
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
    /// A value of CsePersonDetail.
    /// </summary>
    [JsonPropertyName("csePersonDetail")]
    public CsePersonDetail CsePersonDetail
    {
      get => csePersonDetail ??= new();
      set => csePersonDetail = value;
    }

    private Case1 case1;
    private CaseRole caseRole;
    private CsePerson csePerson;
    private CsePersonDetail csePersonDetail;
  }
#endregion
}
