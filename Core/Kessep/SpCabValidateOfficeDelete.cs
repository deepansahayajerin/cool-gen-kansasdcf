// Program: SP_CAB_VALIDATE_OFFICE_DELETE, ID: 371782949, model: 746.
// Short name: SWE00095
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_CAB_VALIDATE_OFFICE_DELETE.
/// </para>
/// <para>
/// RESP: SRVPLAN
/// </para>
/// </summary>
[Serializable]
public partial class SpCabValidateOfficeDelete: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_VALIDATE_OFFICE_DELETE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabValidateOfficeDelete(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabValidateOfficeDelete.
  /// </summary>
  public SpCabValidateOfficeDelete(IContext context, Import import,
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
    if (!ReadOffice())
    {
      ExitState = "FN0000_OFFICE_NF";

      return;
    }

    if (ReadCase())
    {
      ExitState = "CANNOT_DEL_REL_TO_CASE";

      return;
    }

    if (ReadOfficeAssignmentPlan())
    {
      ExitState = "CANNOT_DEL_REL_TO_OFF_ASSGN_PLN";

      return;
    }

    if (ReadOfficeServiceProvider())
    {
      ExitState = "CANNOT_DEL_REL_TO_OFC_SERV_PROV";

      return;
    }

    if (ReadOfficeCaseloadAssignment())
    {
      ExitState = "CANNOT_DEL_REL_TO_OFC_CL_ASGNMNT";

      return;
    }

    if (ReadCountyService())
    {
      ExitState = "CANNOT_DEL_REL_TO_COUNTY_SERVICE";
    }
  }

  private bool ReadCase()
  {
    entities.ExistingCase.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "offGeneratedId", entities.ExistingOffice.SystemGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.ExistingCase.OffGeneratedId = db.GetNullableInt32(reader, 0);
        entities.ExistingCase.Number = db.GetString(reader, 1);
        entities.ExistingCase.Populated = true;
      });
  }

  private bool ReadCountyService()
  {
    entities.ExistingCountyService.Populated = false;

    return Read("ReadCountyService",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "offGeneratedId", entities.ExistingOffice.SystemGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.ExistingCountyService.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCountyService.OffGeneratedId =
          db.GetNullableInt32(reader, 1);
        entities.ExistingCountyService.Populated = true;
      });
  }

  private bool ReadOffice()
  {
    entities.ExistingOffice.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(command, "officeId", import.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.ExistingOffice.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingOffice.OffOffice = db.GetNullableInt32(reader, 1);
        entities.ExistingOffice.Populated = true;
      });
  }

  private bool ReadOfficeAssignmentPlan()
  {
    entities.ExistingOfficeAssignmentPlan.Populated = false;

    return Read("ReadOfficeAssignmentPlan",
      (db, command) =>
      {
        db.SetInt32(
          command, "offGeneratedId", entities.ExistingOffice.SystemGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.ExistingOfficeAssignmentPlan.OffGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingOfficeAssignmentPlan.EffectiveDate =
          db.GetDate(reader, 1);
        entities.ExistingOfficeAssignmentPlan.AssignmentType =
          db.GetString(reader, 2);
        entities.ExistingOfficeAssignmentPlan.Populated = true;
      });
  }

  private bool ReadOfficeCaseloadAssignment()
  {
    entities.ExistingOfficeCaseloadAssignment.Populated = false;

    return Read("ReadOfficeCaseloadAssignment",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "offGeneratedId", entities.ExistingOffice.SystemGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.ExistingOfficeCaseloadAssignment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingOfficeCaseloadAssignment.OffGeneratedId =
          db.GetNullableInt32(reader, 1);
        entities.ExistingOfficeCaseloadAssignment.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider()
  {
    entities.ExistingOfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "offGeneratedId", entities.ExistingOffice.SystemGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.ExistingOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 1);
        entities.ExistingOfficeServiceProvider.RoleCode =
          db.GetString(reader, 2);
        entities.ExistingOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 3);
        entities.ExistingOfficeServiceProvider.Populated = true;
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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of ListState.
      /// </summary>
      [JsonPropertyName("listState")]
      public Common ListState
      {
        get => listState ??= new();
        set => listState = value;
      }

      /// <summary>
      /// A value of ListAddressType.
      /// </summary>
      [JsonPropertyName("listAddressType")]
      public Common ListAddressType
      {
        get => listAddressType ??= new();
        set => listAddressType = value;
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

      /// <summary>
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public OfficeAddress Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>
      /// A value of OfficeAddress.
      /// </summary>
      [JsonPropertyName("officeAddress")]
      public OfficeAddress OfficeAddress
      {
        get => officeAddress ??= new();
        set => officeAddress = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Common listState;
      private Common listAddressType;
      private Common common;
      private OfficeAddress hidden;
      private OfficeAddress officeAddress;
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

    private Office office;
    private Array<GroupGroup> group;
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
      /// A value of ListState.
      /// </summary>
      [JsonPropertyName("listState")]
      public Common ListState
      {
        get => listState ??= new();
        set => listState = value;
      }

      /// <summary>
      /// A value of ListAddressType.
      /// </summary>
      [JsonPropertyName("listAddressType")]
      public Common ListAddressType
      {
        get => listAddressType ??= new();
        set => listAddressType = value;
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

      /// <summary>
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public OfficeAddress Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>
      /// A value of OfficeAddress.
      /// </summary>
      [JsonPropertyName("officeAddress")]
      public OfficeAddress OfficeAddress
      {
        get => officeAddress ??= new();
        set => officeAddress = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Common listState;
      private Common listAddressType;
      private Common common;
      private OfficeAddress hidden;
      private OfficeAddress officeAddress;
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

    private Array<GroupGroup> group;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingOffice.
    /// </summary>
    [JsonPropertyName("existingOffice")]
    public Office ExistingOffice
    {
      get => existingOffice ??= new();
      set => existingOffice = value;
    }

    /// <summary>
    /// A value of ExistingCase.
    /// </summary>
    [JsonPropertyName("existingCase")]
    public Case1 ExistingCase
    {
      get => existingCase ??= new();
      set => existingCase = value;
    }

    /// <summary>
    /// A value of ExistingOfficeAssignmentPlan.
    /// </summary>
    [JsonPropertyName("existingOfficeAssignmentPlan")]
    public OfficeAssignmentPlan ExistingOfficeAssignmentPlan
    {
      get => existingOfficeAssignmentPlan ??= new();
      set => existingOfficeAssignmentPlan = value;
    }

    /// <summary>
    /// A value of ExistingOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("existingOfficeServiceProvider")]
    public OfficeServiceProvider ExistingOfficeServiceProvider
    {
      get => existingOfficeServiceProvider ??= new();
      set => existingOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingFips.
    /// </summary>
    [JsonPropertyName("existingFips")]
    public Fips ExistingFips
    {
      get => existingFips ??= new();
      set => existingFips = value;
    }

    /// <summary>
    /// A value of ExistingOfficeCaseloadAssignment.
    /// </summary>
    [JsonPropertyName("existingOfficeCaseloadAssignment")]
    public OfficeCaseloadAssignment ExistingOfficeCaseloadAssignment
    {
      get => existingOfficeCaseloadAssignment ??= new();
      set => existingOfficeCaseloadAssignment = value;
    }

    /// <summary>
    /// A value of ExistingCountyService.
    /// </summary>
    [JsonPropertyName("existingCountyService")]
    public CountyService ExistingCountyService
    {
      get => existingCountyService ??= new();
      set => existingCountyService = value;
    }

    /// <summary>
    /// A value of ExistingCseOrganization.
    /// </summary>
    [JsonPropertyName("existingCseOrganization")]
    public CseOrganization ExistingCseOrganization
    {
      get => existingCseOrganization ??= new();
      set => existingCseOrganization = value;
    }

    private Office existingOffice;
    private Case1 existingCase;
    private OfficeAssignmentPlan existingOfficeAssignmentPlan;
    private OfficeServiceProvider existingOfficeServiceProvider;
    private Fips existingFips;
    private OfficeCaseloadAssignment existingOfficeCaseloadAssignment;
    private CountyService existingCountyService;
    private CseOrganization existingCseOrganization;
  }
#endregion
}
