// Program: SI_RMAD_ALL_ROLES, ID: 373475611, model: 746.
// Short name: SWE00664
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_RMAD_ALL_ROLES.
/// </summary>
[Serializable]
public partial class SiRmadAllRoles: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_RMAD_ALL_ROLES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiRmadAllRoles(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiRmadAllRoles.
  /// </summary>
  public SiRmadAllRoles(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.DateWorkArea.Date = Now().Date;
    local.ErrOnAdabasUnavailable.Flag = "Y";
    export.Export1.Index = -1;

    foreach(var item in ReadCase())
    {
      if (Equal(entities.Case1.Number, local.Case1.Number))
      {
        continue;
      }
      else
      {
        local.Case1.Number = entities.Case1.Number;
      }

      UseSiReadOfficeOspHeader();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      foreach(var item1 in ReadCaseRoleCsePerson())
      {
        local.CsePersonsWorkSet.Number = entities.AllCaseCsePerson.Number;

        if (CharAt(local.CsePersonsWorkSet.Number, 10) == 'O')
        {
          local.CsePersonsWorkSet.FormattedName =
            entities.AllCaseCsePerson.OrganizationName ?? Spaces(33);
        }
        else
        {
          UseCabReadAdabasPerson();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          UseSiFormatCsePersonName();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }

        ++export.Export1.Index;
        export.Export1.CheckSize();

        if (export.Export1.Index >= 100)
        {
          ExitState = "SI_RMAD_MORE_THAN_100_ROWS";

          return;
        }

        export.Export1.Update.CaseRole.Assign(entities.AllCaseCaseRole);
        MoveCsePersonsWorkSet(local.CsePersonsWorkSet,
          export.Export1.Update.CsePersonsWorkSet);
        export.Export1.Update.Office.Name = local.Office.Name;
        MoveServiceProvider(local.ServiceProvider,
          export.Export1.Update.ServiceProvider);
        MoveCase1(entities.Case1, export.Export1.Update.Case1);
        export.Export1.Update.RowNumber.Count = export.Export1.Index + 1;
      }
    }
  }

  private static void MoveCase1(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.Status = source.Status;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveServiceProvider(ServiceProvider source,
    ServiceProvider target)
  {
    target.UserId = source.UserId;
    target.LastName = source.LastName;
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

  private void UseSiFormatCsePersonName()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    local.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseSiReadOfficeOspHeader()
  {
    var useImport = new SiReadOfficeOspHeader.Import();
    var useExport = new SiReadOfficeOspHeader.Export();

    useImport.Case1.Number = entities.Case1.Number;

    Call(SiReadOfficeOspHeader.Execute, useImport, useExport);

    local.Office.Name = useExport.Office.Name;
    local.ServiceProvider.LastName = useExport.ServiceProvider.LastName;
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRoleCsePerson()
  {
    entities.AllCaseCsePerson.Populated = false;
    entities.AllCaseCaseRole.Populated = false;

    return ReadEach("ReadCaseRoleCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.AllCaseCaseRole.CasNumber = db.GetString(reader, 0);
        entities.AllCaseCaseRole.CspNumber = db.GetString(reader, 1);
        entities.AllCaseCsePerson.Number = db.GetString(reader, 1);
        entities.AllCaseCaseRole.Type1 = db.GetString(reader, 2);
        entities.AllCaseCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.AllCaseCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.AllCaseCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.AllCaseCsePerson.Type1 = db.GetString(reader, 6);
        entities.AllCaseCsePerson.OrganizationName =
          db.GetNullableString(reader, 7);
        entities.AllCaseCsePerson.Populated = true;
        entities.AllCaseCaseRole.Populated = true;

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

    private CsePerson csePerson;
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
      /// A value of RowIndicator.
      /// </summary>
      [JsonPropertyName("rowIndicator")]
      public Common RowIndicator
      {
        get => rowIndicator ??= new();
        set => rowIndicator = value;
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
      /// A value of CsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("csePersonsWorkSet")]
      public CsePersonsWorkSet CsePersonsWorkSet
      {
        get => csePersonsWorkSet ??= new();
        set => csePersonsWorkSet = value;
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
      /// A value of RowNumber.
      /// </summary>
      [JsonPropertyName("rowNumber")]
      public Common RowNumber
      {
        get => rowNumber ??= new();
        set => rowNumber = value;
      }

      /// <summary>
      /// A value of RowOperation.
      /// </summary>
      [JsonPropertyName("rowOperation")]
      public Standard RowOperation
      {
        get => rowOperation ??= new();
        set => rowOperation = value;
      }

      /// <summary>
      /// A value of Office.
      /// </summary>
      [JsonPropertyName("office")]
      public Office Office
      {
        get => office ??= new();
        set => office = value;
      }

      /// <summary>
      /// A value of ServiceProvider.
      /// </summary>
      [JsonPropertyName("serviceProvider")]
      public ServiceProvider ServiceProvider
      {
        get => serviceProvider ??= new();
        set => serviceProvider = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common rowIndicator;
      private CaseRole caseRole;
      private CsePersonsWorkSet csePersonsWorkSet;
      private Case1 case1;
      private Common rowNumber;
      private Standard rowOperation;
      private Office office;
      private ServiceProvider serviceProvider;
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

    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    private Array<ExportGroup> export1;
    private AbendData abendData;
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
    /// A value of ErrOnAdabasUnavailable.
    /// </summary>
    [JsonPropertyName("errOnAdabasUnavailable")]
    public Common ErrOnAdabasUnavailable
    {
      get => errOnAdabasUnavailable ??= new();
      set => errOnAdabasUnavailable = value;
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

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    private Case1 case1;
    private Common errOnAdabasUnavailable;
    private AbendData abendData;
    private DateWorkArea dateWorkArea;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Office office;
    private ServiceProvider serviceProvider;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of AllCaseCsePerson.
    /// </summary>
    [JsonPropertyName("allCaseCsePerson")]
    public CsePerson AllCaseCsePerson
    {
      get => allCaseCsePerson ??= new();
      set => allCaseCsePerson = value;
    }

    /// <summary>
    /// A value of AllCaseCaseRole.
    /// </summary>
    [JsonPropertyName("allCaseCaseRole")]
    public CaseRole AllCaseCaseRole
    {
      get => allCaseCaseRole ??= new();
      set => allCaseCaseRole = value;
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

    private CsePerson allCaseCsePerson;
    private CaseRole allCaseCaseRole;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private Case1 case1;
  }
#endregion
}
