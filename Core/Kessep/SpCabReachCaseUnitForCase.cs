// Program: SP_CAB_REACH_CASE_UNIT_FOR_CASE, ID: 372327083, model: 746.
// Short name: SWE01708
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
/// A program: SP_CAB_REACH_CASE_UNIT_FOR_CASE.
/// </para>
/// <para>
///   This action block will read each Case Unit for a case.
/// </para>
/// </summary>
[Serializable]
public partial class SpCabReachCaseUnitForCase: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_REACH_CASE_UNIT_FOR_CASE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabReachCaseUnitForCase(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabReachCaseUnitForCase.
  /// </summary>
  public SpCabReachCaseUnitForCase(IContext context, Import import,
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
    // ****************************************************
    // A.Kinney	04/29/97	Changed Current_Date
    // R.Grey		06/10/97	Add AR = Organization logic
    // 12/19/00  swsrchf I00109643 If no active Service Provider, display the 
    // last
    //                             Service Provider
    // 01/31/01  swsrchf I00111937 Do not display AP name, when the case is OPEN
    // and the
    //                             AP role has been end dated
    // ****************************************************
    local.Current.Date = Now().Date;
    export.Case1.Number = import.Case1.Number;

    if (ReadCase())
    {
      export.Case1.Number = import.Case1.Number;
      UseCabSetMaximumDiscontinueDate();

      // *** Problem report I00109643
      // *** 12/19/00 swsrchf
      local.OspFound.Flag = "N";

      if (ReadCaseAssignmentOfficeServiceProvider1())
      {
        if (ReadServiceProvider())
        {
          export.ServiceProvider.LastName = entities.ServiceProvider.LastName;
        }

        if (ReadOffice())
        {
          MoveOffice(entities.Office, export.Office);
        }

        // *** Problem report I00109643
        // *** 12/19/00 swsrchf
        local.OspFound.Flag = "Y";
      }

      // *** Problem report I00109643
      // *** 12/19/00 swsrchf
      // *** start
      if (AsChar(local.OspFound.Flag) == 'N')
      {
        if (ReadCaseAssignmentOfficeServiceProvider2())
        {
          if (ReadServiceProvider())
          {
            export.ServiceProvider.LastName = entities.ServiceProvider.LastName;
          }

          if (ReadOffice())
          {
            MoveOffice(entities.Office, export.Office);
          }
        }
      }

      // *** end
      // *** 12/19/00 swsrchf
      // *** Problem report I00109643
      export.Group.Index = 0;
      export.Group.Clear();

      foreach(var item in ReadCaseUnitCsePersonCsePersonCsePerson())
      {
        export.Group.Update.GrCaseUnit.Assign(entities.CaseUnit);
        export.Group.Update.GrNbr.CuNumber = entities.CaseUnit.CuNumber;
        export.Group.Update.GrApCsePerson.Number = entities.ApCsePerson.Number;
        export.Group.Update.GrArCsePerson.Number = entities.Ar.Number;
        export.Group.Update.GrChCsePerson.Number = entities.Ch.Number;
        export.Group.Update.GrCsunitSt1.Text1 =
          Substring(entities.CaseUnit.State, 1, 1);
        export.Group.Update.GrCsunitSt2.Text1 =
          Substring(entities.CaseUnit.State, 2, 1);
        export.Group.Update.GrCsunitSt3.Text1 =
          Substring(entities.CaseUnit.State, 3, 1);
        export.Group.Update.GrCsunitSt4.Text1 =
          Substring(entities.CaseUnit.State, 4, 1);
        export.Group.Update.GrCsunitSt5.Text1 =
          Substring(entities.CaseUnit.State, 5, 1);

        if (Equal(entities.CaseUnit.ClosureDate, local.MaxDate.Date))
        {
          export.Group.Update.GrCaseUnit.ClosureDate =
            local.InitializedDate.Date;
        }

        // *** Problem report I00111937
        // *** 01/31/01 swsrchf
        // *** start
        local.ApInactive.Flag = "Y";

        if (ReadCaseRole())
        {
          if (AsChar(entities.Case1.Status) == 'O' && !
            Equal(entities.ApCaseRole.EndDate, local.MaxDate.Date))
          {
            goto Read;
          }

          local.CsePersonsWorkSet.Number = entities.ApCsePerson.Number;
          UseSiReadCsePerson();
          export.Group.Update.GrApCsePersonsWorkSet.FormattedName =
            local.CsePersonsWorkSet.FormattedName;
          local.ApInactive.Flag = "N";
        }

Read:

        if (AsChar(local.ApInactive.Flag) == 'Y')
        {
          export.Group.Update.GrApCsePerson.Number = "";
          export.Group.Update.GrApCsePersonsWorkSet.FormattedName = "";
        }

        // *** end
        // *** 01/31/01 swsrchf
        // *** Problem report I00111937
        local.CsePersonsWorkSet.Number = entities.Ar.Number;

        if (CharAt(local.CsePersonsWorkSet.Number, 10) == 'O')
        {
          // ***********************************************
          // If the AR is an Organization do not read ADABAS
          // ***********************************************
          export.Group.Update.GrArCsePersonsWorkSet.FormattedName =
            entities.Ar.OrganizationName ?? Spaces(33);
        }
        else
        {
          UseSiReadCsePerson();
          export.Group.Update.GrArCsePersonsWorkSet.FormattedName =
            local.CsePersonsWorkSet.FormattedName;
        }

        local.CsePersonsWorkSet.Number = entities.Ch.Number;
        UseSiReadCsePerson();
        export.Group.Update.GrChCsePersonsWorkSet.FormattedName =
          local.CsePersonsWorkSet.FormattedName;
        export.Group.Next();
      }

      if (export.Group.IsEmpty)
      {
        ExitState = "ZD_ACO_NI0000_NO_DATA_TO_DISP_2";
      }
      else
      {
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
      }
    }
    else
    {
      ExitState = "CASE_NF";
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.TypeCode = source.TypeCode;
    target.Name = source.Name;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.MaxDate.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.MaxDate.Date = useExport.DateWorkArea.Date;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.ErrOnAdabasUnavailable.Flag = local.ErrOnAdabas.Flag;
    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.Ae.Flag = useExport.Ae.Flag;
    local.AbendData.Assign(useExport.AbendData);
    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
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
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseAssignmentOfficeServiceProvider1()
  {
    entities.CaseAssignment.Populated = false;
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadCaseAssignmentOfficeServiceProvider1",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNo", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 0);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 1);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 3);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 3);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 4);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 4);
        entities.CaseAssignment.OspCode = db.GetString(reader, 5);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 5);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 6);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 6);
        entities.CaseAssignment.CasNo = db.GetString(reader, 7);
        entities.CaseAssignment.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadCaseAssignmentOfficeServiceProvider2()
  {
    entities.CaseAssignment.Populated = false;
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadCaseAssignmentOfficeServiceProvider2",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNo", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 0);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 1);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 3);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 3);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 4);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 4);
        entities.CaseAssignment.OspCode = db.GetString(reader, 5);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 5);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 6);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 6);
        entities.CaseAssignment.CasNo = db.GetString(reader, 7);
        entities.CaseAssignment.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadCaseRole()
  {
    entities.ApCaseRole.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ApCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ApCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ApCaseRole.Type1 = db.GetString(reader, 2);
        entities.ApCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ApCaseRole.EndDate = db.GetNullableDate(reader, 4);
        entities.ApCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ApCaseRole.Type1);
      });
  }

  private IEnumerable<bool> ReadCaseUnitCsePersonCsePersonCsePerson()
  {
    return ReadEach("ReadCaseUnitCsePersonCsePersonCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNo", import.Case1.Number);
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.State = db.GetString(reader, 1);
        entities.CaseUnit.StartDate = db.GetDate(reader, 2);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 3);
        entities.CaseUnit.ClosureReasonCode = db.GetNullableString(reader, 4);
        entities.CaseUnit.CasNo = db.GetString(reader, 5);
        entities.CaseUnit.CspNoAr = db.GetNullableString(reader, 6);
        entities.Ar.Number = db.GetString(reader, 6);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 7);
        entities.ApCsePerson.Number = db.GetString(reader, 7);
        entities.CaseUnit.CspNoChild = db.GetNullableString(reader, 8);
        entities.Ch.Number = db.GetString(reader, 8);
        entities.Ch.Type1 = db.GetString(reader, 9);
        entities.Ch.NameMiddle = db.GetNullableString(reader, 10);
        entities.Ch.NameMaiden = db.GetNullableString(reader, 11);
        entities.Ar.Type1 = db.GetString(reader, 12);
        entities.Ar.NameMiddle = db.GetNullableString(reader, 13);
        entities.Ar.NameMaiden = db.GetNullableString(reader, 14);
        entities.Ar.OrganizationName = db.GetNullableString(reader, 15);
        entities.ApCsePerson.Type1 = db.GetString(reader, 16);
        entities.ApCsePerson.NameMiddle = db.GetNullableString(reader, 17);
        entities.ApCsePerson.NameMaiden = db.GetNullableString(reader, 18);
        entities.Ar.Populated = true;
        entities.Ch.Populated = true;
        entities.ApCsePerson.Populated = true;
        entities.CaseUnit.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Ch.Type1);
        CheckValid<CsePerson>("Type1", entities.Ar.Type1);
        CheckValid<CsePerson>("Type1", entities.ApCsePerson.Type1);

        return true;
      });
  }

  private bool ReadOffice()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.Office.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(
          command, "officeId", entities.OfficeServiceProvider.OffGeneratedId);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.TypeCode = db.GetString(reader, 1);
        entities.Office.Name = db.GetString(reader, 2);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 3);
        entities.Office.Populated = true;
      });
  }

  private bool ReadServiceProvider()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId",
          entities.OfficeServiceProvider.SpdGeneratedId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.LastName = db.GetString(reader, 2);
        entities.ServiceProvider.Populated = true;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private Case1 case1;
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
      /// A value of GrCommon.
      /// </summary>
      [JsonPropertyName("grCommon")]
      public Common GrCommon
      {
        get => grCommon ??= new();
        set => grCommon = value;
      }

      /// <summary>
      /// A value of GrNbr.
      /// </summary>
      [JsonPropertyName("grNbr")]
      public CaseUnit GrNbr
      {
        get => grNbr ??= new();
        set => grNbr = value;
      }

      /// <summary>
      /// A value of GrCsunitSt1.
      /// </summary>
      [JsonPropertyName("grCsunitSt1")]
      public WorkArea GrCsunitSt1
      {
        get => grCsunitSt1 ??= new();
        set => grCsunitSt1 = value;
      }

      /// <summary>
      /// A value of GrCsunitSt2.
      /// </summary>
      [JsonPropertyName("grCsunitSt2")]
      public WorkArea GrCsunitSt2
      {
        get => grCsunitSt2 ??= new();
        set => grCsunitSt2 = value;
      }

      /// <summary>
      /// A value of GrCsunitSt3.
      /// </summary>
      [JsonPropertyName("grCsunitSt3")]
      public WorkArea GrCsunitSt3
      {
        get => grCsunitSt3 ??= new();
        set => grCsunitSt3 = value;
      }

      /// <summary>
      /// A value of GrCsunitSt4.
      /// </summary>
      [JsonPropertyName("grCsunitSt4")]
      public WorkArea GrCsunitSt4
      {
        get => grCsunitSt4 ??= new();
        set => grCsunitSt4 = value;
      }

      /// <summary>
      /// A value of GrCsunitSt5.
      /// </summary>
      [JsonPropertyName("grCsunitSt5")]
      public WorkArea GrCsunitSt5
      {
        get => grCsunitSt5 ??= new();
        set => grCsunitSt5 = value;
      }

      /// <summary>
      /// A value of GrArCsePerson.
      /// </summary>
      [JsonPropertyName("grArCsePerson")]
      public CsePerson GrArCsePerson
      {
        get => grArCsePerson ??= new();
        set => grArCsePerson = value;
      }

      /// <summary>
      /// A value of GrCaseUnit.
      /// </summary>
      [JsonPropertyName("grCaseUnit")]
      public CaseUnit GrCaseUnit
      {
        get => grCaseUnit ??= new();
        set => grCaseUnit = value;
      }

      /// <summary>
      /// A value of GrApCsePerson.
      /// </summary>
      [JsonPropertyName("grApCsePerson")]
      public CsePerson GrApCsePerson
      {
        get => grApCsePerson ??= new();
        set => grApCsePerson = value;
      }

      /// <summary>
      /// A value of GrChCsePerson.
      /// </summary>
      [JsonPropertyName("grChCsePerson")]
      public CsePerson GrChCsePerson
      {
        get => grChCsePerson ??= new();
        set => grChCsePerson = value;
      }

      /// <summary>
      /// A value of GrChCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("grChCsePersonsWorkSet")]
      public CsePersonsWorkSet GrChCsePersonsWorkSet
      {
        get => grChCsePersonsWorkSet ??= new();
        set => grChCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of GrArCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("grArCsePersonsWorkSet")]
      public CsePersonsWorkSet GrArCsePersonsWorkSet
      {
        get => grArCsePersonsWorkSet ??= new();
        set => grArCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of GrApCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("grApCsePersonsWorkSet")]
      public CsePersonsWorkSet GrApCsePersonsWorkSet
      {
        get => grApCsePersonsWorkSet ??= new();
        set => grApCsePersonsWorkSet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common grCommon;
      private CaseUnit grNbr;
      private WorkArea grCsunitSt1;
      private WorkArea grCsunitSt2;
      private WorkArea grCsunitSt3;
      private WorkArea grCsunitSt4;
      private WorkArea grCsunitSt5;
      private CsePerson grArCsePerson;
      private CaseUnit grCaseUnit;
      private CsePerson grApCsePerson;
      private CsePerson grChCsePerson;
      private CsePersonsWorkSet grChCsePersonsWorkSet;
      private CsePersonsWorkSet grArCsePersonsWorkSet;
      private CsePersonsWorkSet grApCsePersonsWorkSet;
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

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

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

    private Case1 case1;
    private Office office;
    private ServiceProvider serviceProvider;
    private Array<GroupGroup> group;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ApInactive.
    /// </summary>
    [JsonPropertyName("apInactive")]
    public Common ApInactive
    {
      get => apInactive ??= new();
      set => apInactive = value;
    }

    /// <summary>
    /// A value of OspFound.
    /// </summary>
    [JsonPropertyName("ospFound")]
    public Common OspFound
    {
      get => ospFound ??= new();
      set => ospFound = value;
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
    /// A value of InitializedDate.
    /// </summary>
    [JsonPropertyName("initializedDate")]
    public DateWorkArea InitializedDate
    {
      get => initializedDate ??= new();
      set => initializedDate = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    /// <summary>
    /// A value of Ae.
    /// </summary>
    [JsonPropertyName("ae")]
    public Common Ae
    {
      get => ae ??= new();
      set => ae = value;
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
    /// A value of ErrOnAdabas.
    /// </summary>
    [JsonPropertyName("errOnAdabas")]
    public Common ErrOnAdabas
    {
      get => errOnAdabas ??= new();
      set => errOnAdabas = value;
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

    private Common apInactive;
    private Common ospFound;
    private DateWorkArea current;
    private DateWorkArea initializedDate;
    private DateWorkArea maxDate;
    private Common ae;
    private AbendData abendData;
    private Common errOnAdabas;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ApCaseRole.
    /// </summary>
    [JsonPropertyName("apCaseRole")]
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
    }

    /// <summary>
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of Ch.
    /// </summary>
    [JsonPropertyName("ch")]
    public CsePerson Ch
    {
      get => ch ??= new();
      set => ch = value;
    }

    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
    }

    /// <summary>
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
    }

    private CaseRole apCaseRole;
    private CaseAssignment caseAssignment;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private Office office;
    private Case1 case1;
    private CsePerson ar;
    private CsePerson ch;
    private CsePerson apCsePerson;
    private CaseUnit caseUnit;
  }
#endregion
}
