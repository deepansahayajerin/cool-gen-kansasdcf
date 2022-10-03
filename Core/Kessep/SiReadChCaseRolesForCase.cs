// Program: SI_READ_CH_CASE_ROLES_FOR_CASE, ID: 374382354, model: 746.
// Short name: SWE02866
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
/// A program: SI_READ_CH_CASE_ROLES_FOR_CASE.
/// </para>
/// <para>
/// RESP: SRVINIT		
/// This AB reads all of the case roles (current and past) and the person that 
/// case role is for.
/// </para>
/// </summary>
[Serializable]
public partial class SiReadChCaseRolesForCase: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_READ_CH_CASE_ROLES_FOR_CASE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiReadChCaseRolesForCase(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiReadChCaseRolesForCase.
  /// </summary>
  public SiReadChCaseRolesForCase(IContext context, Import import, Export export)
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
    // ---------------------------------------------------------------------------------------------
    // 			C H A N G E    L O G
    // ---------------------------------------------------------------------------------------------
    // ---------------------------------------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // --------  ----------	------------	
    // -----------------------------------------------------
    // 02-23-00  W.Campbell			Initial Development
    // 05-26-00  M.Lachowicz	PR96158		Change READ EACH.
    // 05-08-06  GVandy	WR230751	Add Hospital Paternity Establishment Indicator.
    // 05-10-17  GVandy	CQ48108		Changes to support IV-D PEP.
    // 					Include Paternity_Lock_ind in export group.
    // ---------------------------------------------------------------------------------------------
    local.ErrOnAdabasUnavailable.Flag = "Y";
    local.Current.Date = Now().Date;
    UseCabSetMaximumDiscontinueDate();

    if (ReadCase())
    {
      export.Case1.Assign(entities.Case1);
    }
    else
    {
      ExitState = "CASE_NF";

      return;
    }

    export.Export1.Index = -1;

    // 05/26/00 M.L Start
    foreach(var item in ReadCaseRoleCsePerson())
    {
      if (Equal(entities.CsePerson.Number, local.Previous.Number))
      {
        continue;
      }

      local.Previous.Number = entities.CsePerson.Number;

      ++export.Export1.Index;
      export.Export1.CheckSize();

      if (export.Export1.Index >= Export.ExportGroup.Capacity)
      {
        // 05/26/00 M.L Start
        // Marek L.
        // Pass Start_Date to calling Prad.
        // 05/26/00 M.L End
        export.PageKeyCsePerson.Number = entities.CsePerson.Number;

        break;
      }

      export.Export1.Update.DetailCsePerson.Assign(entities.CsePerson);

      if (CharAt(entities.CsePerson.Number, 10) != 'O')
      {
        // ---------------------------------------------
        // Read Name details from ADABAS
        // ---------------------------------------------
        local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
        UseCabReadAdabasPerson();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        export.Export1.Update.DetailCsePersonsWorkSet.Assign(
          local.CsePersonsWorkSet);

        if (Equal(export.Export1.Item.DetailCsePersonsWorkSet.Ssn, "000000000"))
        {
          export.Export1.Update.DetailCsePersonsWorkSet.Ssn = "";
        }

        UseSiFormatCsePersonName();
        export.Export1.Update.DetailCsePersonsWorkSet.FormattedName =
          local.CsePersonsWorkSet.FormattedName;
      }
      else
      {
        export.Export1.Update.DetailCsePersonsWorkSet.FormattedName =
          entities.CsePerson.OrganizationName ?? Spaces(33);
      }

      export.Export1.Update.DetailCaseRole.Assign(entities.CaseRole);

      if (Equal(entities.CaseRole.EndDate, local.DateWorkArea.Date))
      {
        export.Export1.Update.DetailCaseRole.EndDate = null;
      }
    }

    // 05/26/00 M.L End
    // ---------------------------------------------
    // Set the scrolling message based on what has
    // been retrieved.
    // ---------------------------------------------
    if (import.Standard.PageNumber == 1)
    {
      if (export.Export1.Index >= Export.ExportGroup.Capacity)
      {
        export.Standard.ScrollingMessage = "MORE +";
      }
      else
      {
        export.Standard.ScrollingMessage = "MORE";
      }
    }
    else if (import.Standard.PageNumber == 0)
    {
      export.Standard.ScrollingMessage = "MORE";
    }
    else if (export.Export1.Index >= Export.ExportGroup.Capacity)
    {
      export.Standard.ScrollingMessage = "MORE - +";
    }
    else
    {
      export.Standard.ScrollingMessage = "MORE -";
    }
  }

  private void UseCabReadAdabasPerson()
  {
    var useImport = new CabReadAdabasPerson.Import();
    var useExport = new CabReadAdabasPerson.Export();

    useImport.ErrOnAdabasUnavailable.Flag = local.ErrOnAdabasUnavailable.Flag;
    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(CabReadAdabasPerson.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    export.AbendData.Assign(useExport.AbendData);
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.DateWorkArea.Date = useExport.DateWorkArea.Date;
  }

  private void UseSiFormatCsePersonName()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    local.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
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
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 2);
        entities.Case1.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseRoleCsePerson()
  {
    entities.CsePerson.Populated = false;
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRoleCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "numb", import.PageKeyCsePerson.Number);
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
        entities.CsePerson.Type1 = db.GetString(reader, 6);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 7);
        entities.CsePerson.BornOutOfWedlock = db.GetNullableString(reader, 8);
        entities.CsePerson.CseToEstblPaternity =
          db.GetNullableString(reader, 9);
        entities.CsePerson.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 10);
        entities.CsePerson.DatePaternEstab = db.GetDate(reader, 11);
        entities.CsePerson.BirthCertFathersLastName =
          db.GetNullableString(reader, 12);
        entities.CsePerson.BirthCertFathersFirstName =
          db.GetNullableString(reader, 13);
        entities.CsePerson.BirthCertFathersMi =
          db.GetNullableString(reader, 14);
        entities.CsePerson.BirthCertificateSignature =
          db.GetNullableString(reader, 15);
        entities.CsePerson.HospitalPatEstInd = db.GetNullableString(reader, 16);
        entities.CsePerson.PaternityLockInd = db.GetNullableString(reader, 17);
        entities.CsePerson.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of PageKeyCsePerson.
    /// </summary>
    [JsonPropertyName("pageKeyCsePerson")]
    public CsePerson PageKeyCsePerson
    {
      get => pageKeyCsePerson ??= new();
      set => pageKeyCsePerson = value;
    }

    /// <summary>
    /// A value of PageKeyCaseRole.
    /// </summary>
    [JsonPropertyName("pageKeyCaseRole")]
    public CaseRole PageKeyCaseRole
    {
      get => pageKeyCaseRole ??= new();
      set => pageKeyCaseRole = value;
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

    private Standard standard;
    private CsePerson pageKeyCsePerson;
    private CaseRole pageKeyCaseRole;
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
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of DetailCsePerson.
      /// </summary>
      [JsonPropertyName("detailCsePerson")]
      public CsePerson DetailCsePerson
      {
        get => detailCsePerson ??= new();
        set => detailCsePerson = value;
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
      /// A value of DetailCaseRole.
      /// </summary>
      [JsonPropertyName("detailCaseRole")]
      public CaseRole DetailCaseRole
      {
        get => detailCaseRole ??= new();
        set => detailCaseRole = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private Common detailCommon;
      private CsePerson detailCsePerson;
      private CsePersonsWorkSet detailCsePersonsWorkSet;
      private CaseRole detailCaseRole;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    /// <summary>
    /// A value of PageKeyCsePerson.
    /// </summary>
    [JsonPropertyName("pageKeyCsePerson")]
    public CsePerson PageKeyCsePerson
    {
      get => pageKeyCsePerson ??= new();
      set => pageKeyCsePerson = value;
    }

    /// <summary>
    /// A value of PageKeyCaseRole.
    /// </summary>
    [JsonPropertyName("pageKeyCaseRole")]
    public CaseRole PageKeyCaseRole
    {
      get => pageKeyCaseRole ??= new();
      set => pageKeyCaseRole = value;
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

    private Case1 case1;
    private Standard standard;
    private AbendData abendData;
    private CsePerson pageKeyCsePerson;
    private CaseRole pageKeyCaseRole;
    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public CsePerson Previous
    {
      get => previous ??= new();
      set => previous = value;
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
    /// A value of ErrOnAdabasUnavailable.
    /// </summary>
    [JsonPropertyName("errOnAdabasUnavailable")]
    public Common ErrOnAdabasUnavailable
    {
      get => errOnAdabasUnavailable ??= new();
      set => errOnAdabasUnavailable = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
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

    private CsePerson previous;
    private DateWorkArea current;
    private Common errOnAdabasUnavailable;
    private DateWorkArea dateWorkArea;
    private CsePersonsWorkSet csePersonsWorkSet;
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

    private CsePerson csePerson;
    private CaseRole caseRole;
    private Case1 case1;
  }
#endregion
}
