// Program: SI_IAPI_DISPLAY_CSENET_DATA, ID: 372856406, model: 746.
// Short name: SWE02862
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_IAPI_DISPLAY_CSENET_DATA.
/// </summary>
[Serializable]
public partial class SiIapiDisplayCsenetData: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_IAPI_DISPLAY_CSENET_DATA program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiIapiDisplayCsenetData(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiIapiDisplayCsenetData.
  /// </summary>
  public SiIapiDisplayCsenetData(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------
    // Date	  Developer Name	Description
    // 07-26-99  C.Scroggins DPMSD	Initial Development
    // ------------------------------------------------------------
    // ****************************************************************************
    // This PRAD displays the data on IAPI. Originally, all of the code was in 
    // the prad, but when I made some additional changes I put all of the code
    // in this CAB.
    // ****************************************************************************
    export.InterstateCase.Assign(import.InterstateCase);
    export.MonitoredActivity.SystemGeneratedIdentifier =
      import.MonitoredActivity.SystemGeneratedIdentifier;
    export.MonitoredActivityAssignment.SystemGeneratedIdentifier =
      import.MonitoredActivityAssignment.SystemGeneratedIdentifier;
    export.HiddenApid.Number = import.HiddenApid.Number;
    export.Infrastructure.ReferenceDate = import.Infrastructure.ReferenceDate;
    local.ApAdded.Flag = "N";
    export.NameGroupList.Index = -1;
    local.Infrastructure.DenormDate = export.InterstateCase.TransactionDate;
    local.Infrastructure.DenormNumeric12 =
      export.InterstateCase.TransSerialNumber;

    if (!import.NameGroupList.IsEmpty)
    {
      for(import.NameGroupList.Index = 0; import.NameGroupList.Index < import
        .NameGroupList.Count; ++import.NameGroupList.Index)
      {
        if (!import.NameGroupList.CheckSize())
        {
          break;
        }

        ++export.NameGroupList.Index;
        export.NameGroupList.CheckSize();

        export.NameGroupList.Update.N.Assign(import.NameGroupList.Item.N);
        export.NameGroupList.Update.NdetailCase.Type1 =
          import.NameGroupList.Item.NdetailCase.Type1;

        if (Equal(export.NameGroupList.Item.NdetailCase.Type1, "AP") || Equal
          (export.NameGroupList.Item.NdetailCase.Type1, "ap"))
        {
          local.ApAdded.Flag = "Y";
        }

        export.NameGroupList.Update.NdetailFamily.Type1 =
          import.NameGroupList.Item.NdetailFamily.Type1;
        export.NameGroupList.Update.NdetailFamily.Type1 =
          ToUpper(export.NameGroupList.Item.NdetailFamily.Type1);
        export.NameGroupList.Update.N.FormattedName =
          ToUpper(export.NameGroupList.Item.N.FormattedName);
        export.NameGroupList.Update.N.LastName =
          ToUpper(export.NameGroupList.Item.N.LastName);
        export.NameGroupList.Update.N.FirstName =
          ToUpper(export.NameGroupList.Item.N.FirstName);
        export.NameGroupList.Update.N.MiddleInitial =
          ToUpper(export.NameGroupList.Item.N.MiddleInitial);
      }

      import.NameGroupList.CheckIndex();
    }

    local.DateWorkArea.Date = Now().Date;
    UseSiReadCsenetApIdData();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.CsePersonsWorkSet.FirstName =
      ToUpper(export.InterstateApIdentification.NameFirst);
    local.CsePersonsWorkSet.LastName =
      ToUpper(export.InterstateApIdentification.NameLast);
    local.CsePersonsWorkSet.MiddleInitial =
      ToUpper(Substring(export.InterstateApIdentification.MiddleName, 12, 1, 1));
      
    UseSiFormatCsePersonName();
    export.Ap.Text37 = ToUpper(local.CsePersonsWorkSet.FormattedName);

    // *********************************************
    // If this is the first time thru add the AP,
    // otherwise don't add again.
    // *********************************************
    if (export.NameGroupList.Index + 1 < Export.NameGroupListGroup.Capacity && AsChar
      (local.ApAdded.Flag) == 'N')
    {
      if (!export.NameGroupList.IsEmpty)
      {
        export.NameGroupList.Index = export.NameGroupList.Count - 1;
        export.NameGroupList.CheckSize();

        if (Equal(ToUpper(export.NameGroupList.Item.N.LastName),
          ToUpper(export.HiddenApid.LastName)) && Equal
          (ToUpper(export.NameGroupList.Item.N.FirstName),
          ToUpper(export.HiddenApid.FirstName)) && Equal
          (export.NameGroupList.Item.N.Dob, export.HiddenApid.Dob))
        {
          return;
        }
      }

      export.HiddenApid.FirstName =
        ToUpper(export.InterstateApIdentification.NameFirst);
      export.HiddenApid.LastName =
        ToUpper(export.InterstateApIdentification.NameLast);
      export.HiddenApid.MiddleInitial =
        ToUpper(
          Substring(export.InterstateApIdentification.MiddleName, 12, 1, 1));

      export.NameGroupList.Index = export.NameGroupList.Count;
      export.NameGroupList.CheckSize();

      export.NameGroupList.Update.N.FormattedName =
        ToUpper(local.CsePersonsWorkSet.FormattedName);
      export.NameGroupList.Update.N.FirstName =
        ToUpper(export.InterstateApIdentification.NameFirst);
      export.NameGroupList.Update.N.LastName =
        ToUpper(export.InterstateApIdentification.NameLast);
      export.NameGroupList.Update.N.MiddleInitial =
        ToUpper(
          Substring(export.InterstateApIdentification.MiddleName, 12, 1, 1));
      export.NameGroupList.Update.N.Dob =
        export.InterstateApIdentification.DateOfBirth;
      export.NameGroupList.Update.N.Ssn =
        export.InterstateApIdentification.Ssn ?? Spaces(9);
      export.NameGroupList.Update.N.Sex =
        ToUpper(export.InterstateApIdentification.Sex);
      export.NameGroupList.Update.NdetailCase.Type1 = "AP";
    }

    if (Equal(export.InterstateApIdentification.Ssn, "000000000"))
    {
      export.InterstateApIdentification.Ssn = "";
    }

    if (Equal(export.InterstateApIdentification.AliasSsn1, "000000000"))
    {
      export.InterstateApIdentification.AliasSsn1 = "";
    }

    if (Equal(export.InterstateApIdentification.AliasSsn2, "000000000"))
    {
      export.InterstateApIdentification.AliasSsn2 = "";
    }

    if (ReadInfrastructure())
    {
      export.Infrastructure.ReferenceDate =
        entities.Infrastructure.ReferenceDate;

      if (ReadMonitoredActivity())
      {
        export.MonitoredActivity.StartDate =
          entities.MonitoredActivity.StartDate;
      }
    }
  }

  private static void MoveInterstateCase(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.TransactionDate = source.TransactionDate;
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

  private void UseSiReadCsenetApIdData()
  {
    var useImport = new SiReadCsenetApIdData.Import();
    var useExport = new SiReadCsenetApIdData.Export();

    MoveInterstateCase(import.InterstateCase, useImport.InterstateCase);

    Call(SiReadCsenetApIdData.Execute, useImport, useExport);

    export.InterstateApIdentification.Assign(
      useExport.InterstateApIdentification);
  }

  private bool ReadInfrastructure()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "denormDate",
          local.Infrastructure.DenormDate.GetValueOrDefault());
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Infrastructure.DenormNumeric12.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.EventId = db.GetInt32(reader, 1);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 2);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 3);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 4);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 5);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 6);
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadMonitoredActivity()
  {
    entities.MonitoredActivity.Populated = false;

    return Read("ReadMonitoredActivity",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infSysGenId",
          entities.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MonitoredActivity.Name = db.GetString(reader, 1);
        entities.MonitoredActivity.ActivityControlNumber =
          db.GetInt32(reader, 2);
        entities.MonitoredActivity.TypeCode = db.GetNullableString(reader, 3);
        entities.MonitoredActivity.FedNonComplianceDate =
          db.GetNullableDate(reader, 4);
        entities.MonitoredActivity.FedNearNonComplDate =
          db.GetNullableDate(reader, 5);
        entities.MonitoredActivity.OtherNonComplianceDate =
          db.GetNullableDate(reader, 6);
        entities.MonitoredActivity.OtherNearNonComplDate =
          db.GetNullableDate(reader, 7);
        entities.MonitoredActivity.StartDate = db.GetDate(reader, 8);
        entities.MonitoredActivity.ClosureDate = db.GetNullableDate(reader, 9);
        entities.MonitoredActivity.ClosureReasonCode =
          db.GetNullableString(reader, 10);
        entities.MonitoredActivity.CaseUnitClosedInd = db.GetString(reader, 11);
        entities.MonitoredActivity.CreatedBy = db.GetString(reader, 12);
        entities.MonitoredActivity.CreatedTimestamp =
          db.GetDateTime(reader, 13);
        entities.MonitoredActivity.LastUpdatedBy =
          db.GetNullableString(reader, 14);
        entities.MonitoredActivity.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 15);
        entities.MonitoredActivity.InfSysGenId =
          db.GetNullableInt32(reader, 16);
        entities.MonitoredActivity.Populated = true;
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
    /// <summary>A NameGroupListGroup group.</summary>
    [Serializable]
    public class NameGroupListGroup
    {
      /// <summary>
      /// A value of N.
      /// </summary>
      [JsonPropertyName("n")]
      public CsePersonsWorkSet N
      {
        get => n ??= new();
        set => n = value;
      }

      /// <summary>
      /// A value of NdetailCase.
      /// </summary>
      [JsonPropertyName("ndetailCase")]
      public CaseRole NdetailCase
      {
        get => ndetailCase ??= new();
        set => ndetailCase = value;
      }

      /// <summary>
      /// A value of NdetailFamily.
      /// </summary>
      [JsonPropertyName("ndetailFamily")]
      public CaseRole NdetailFamily
      {
        get => ndetailFamily ??= new();
        set => ndetailFamily = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private CsePersonsWorkSet n;
      private CaseRole ndetailCase;
      private CaseRole ndetailFamily;
    }

    /// <summary>
    /// A value of HiddenApid.
    /// </summary>
    [JsonPropertyName("hiddenApid")]
    public CsePersonsWorkSet HiddenApid
    {
      get => hiddenApid ??= new();
      set => hiddenApid = value;
    }

    /// <summary>
    /// Gets a value of NameGroupList.
    /// </summary>
    [JsonIgnore]
    public Array<NameGroupListGroup> NameGroupList => nameGroupList ??= new(
      NameGroupListGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of NameGroupList for json serialization.
    /// </summary>
    [JsonPropertyName("nameGroupList")]
    [Computed]
    public IList<NameGroupListGroup> NameGroupList_Json
    {
      get => nameGroupList;
      set => NameGroupList.Assign(value);
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of MonitoredActivity.
    /// </summary>
    [JsonPropertyName("monitoredActivity")]
    public MonitoredActivity MonitoredActivity
    {
      get => monitoredActivity ??= new();
      set => monitoredActivity = value;
    }

    /// <summary>
    /// A value of MonitoredActivityAssignment.
    /// </summary>
    [JsonPropertyName("monitoredActivityAssignment")]
    public MonitoredActivityAssignment MonitoredActivityAssignment
    {
      get => monitoredActivityAssignment ??= new();
      set => monitoredActivityAssignment = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private CsePersonsWorkSet hiddenApid;
    private Array<NameGroupListGroup> nameGroupList;
    private InterstateCase interstateCase;
    private MonitoredActivity monitoredActivity;
    private MonitoredActivityAssignment monitoredActivityAssignment;
    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A NameGroupListGroup group.</summary>
    [Serializable]
    public class NameGroupListGroup
    {
      /// <summary>
      /// A value of N.
      /// </summary>
      [JsonPropertyName("n")]
      public CsePersonsWorkSet N
      {
        get => n ??= new();
        set => n = value;
      }

      /// <summary>
      /// A value of NdetailCase.
      /// </summary>
      [JsonPropertyName("ndetailCase")]
      public CaseRole NdetailCase
      {
        get => ndetailCase ??= new();
        set => ndetailCase = value;
      }

      /// <summary>
      /// A value of NdetailFamily.
      /// </summary>
      [JsonPropertyName("ndetailFamily")]
      public CaseRole NdetailFamily
      {
        get => ndetailFamily ??= new();
        set => ndetailFamily = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private CsePersonsWorkSet n;
      private CaseRole ndetailCase;
      private CaseRole ndetailFamily;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of InterstateApIdentification.
    /// </summary>
    [JsonPropertyName("interstateApIdentification")]
    public InterstateApIdentification InterstateApIdentification
    {
      get => interstateApIdentification ??= new();
      set => interstateApIdentification = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public WorkArea Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// Gets a value of NameGroupList.
    /// </summary>
    [JsonIgnore]
    public Array<NameGroupListGroup> NameGroupList => nameGroupList ??= new(
      NameGroupListGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of NameGroupList for json serialization.
    /// </summary>
    [JsonPropertyName("nameGroupList")]
    [Computed]
    public IList<NameGroupListGroup> NameGroupList_Json
    {
      get => nameGroupList;
      set => NameGroupList.Assign(value);
    }

    /// <summary>
    /// A value of HiddenApid.
    /// </summary>
    [JsonPropertyName("hiddenApid")]
    public CsePersonsWorkSet HiddenApid
    {
      get => hiddenApid ??= new();
      set => hiddenApid = value;
    }

    /// <summary>
    /// A value of MonitoredActivity.
    /// </summary>
    [JsonPropertyName("monitoredActivity")]
    public MonitoredActivity MonitoredActivity
    {
      get => monitoredActivity ??= new();
      set => monitoredActivity = value;
    }

    /// <summary>
    /// A value of MonitoredActivityAssignment.
    /// </summary>
    [JsonPropertyName("monitoredActivityAssignment")]
    public MonitoredActivityAssignment MonitoredActivityAssignment
    {
      get => monitoredActivityAssignment ??= new();
      set => monitoredActivityAssignment = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private InterstateCase interstateCase;
    private InterstateApIdentification interstateApIdentification;
    private WorkArea ap;
    private Array<NameGroupListGroup> nameGroupList;
    private CsePersonsWorkSet hiddenApid;
    private MonitoredActivity monitoredActivity;
    private MonitoredActivityAssignment monitoredActivityAssignment;
    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
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
    /// A value of ApAdded.
    /// </summary>
    [JsonPropertyName("apAdded")]
    public Common ApAdded
    {
      get => apAdded ??= new();
      set => apAdded = value;
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

    private Infrastructure infrastructure;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common apAdded;
    private DateWorkArea dateWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of EventDetail.
    /// </summary>
    [JsonPropertyName("eventDetail")]
    public EventDetail EventDetail
    {
      get => eventDetail ??= new();
      set => eventDetail = value;
    }

    /// <summary>
    /// A value of Event1.
    /// </summary>
    [JsonPropertyName("event1")]
    public Event1 Event1
    {
      get => event1 ??= new();
      set => event1 = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of MonitoredActivity.
    /// </summary>
    [JsonPropertyName("monitoredActivity")]
    public MonitoredActivity MonitoredActivity
    {
      get => monitoredActivity ??= new();
      set => monitoredActivity = value;
    }

    /// <summary>
    /// A value of MonitoredActivityAssignment.
    /// </summary>
    [JsonPropertyName("monitoredActivityAssignment")]
    public MonitoredActivityAssignment MonitoredActivityAssignment
    {
      get => monitoredActivityAssignment ??= new();
      set => monitoredActivityAssignment = value;
    }

    private EventDetail eventDetail;
    private Event1 event1;
    private Infrastructure infrastructure;
    private MonitoredActivity monitoredActivity;
    private MonitoredActivityAssignment monitoredActivityAssignment;
  }
#endregion
}
