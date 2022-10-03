// Program: SP_CADS_EXTERNAL_ALERT, ID: 371732373, model: 746.
// Short name: SWE01872
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CADS_EXTERNAL_ALERT.
/// </summary>
[Serializable]
public partial class SpCadsExternalAlert: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CADS_EXTERNAL_ALERT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCadsExternalAlert(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCadsExternalAlert.
  /// </summary>
  public SpCadsExternalAlert(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------------------------------
    // ALERT CODE:  40
    // ALERT TEXT:  AP NAME         1-  7
    //              OSP NAME        8- 14
    //              REASON CODE    15- 16
    // ALERT CODE:  41
    // ALERT TEXT:  AP NAME         1-  7
    //              OSP NAME        8- 14
    // --------------------------------------------------------------------
    // ---------------------------------------------------------------
    // 		M A I N T E N A N C E   L O G
    // Date	    Developer		Description
    // ??/??/????  ??????????          Original Development
    // 07/19/1999  W.Campbell          Added an extra AND clause to an
    //                                 
    // IF statement in order to also
    //                                 
    // check for the local_kscares
    //                                 
    // ief_supplied flag = SPACES
    // before
    //                                 
    // escaping out of the CAB.
    // 08/22/2018  Raj S               Modified to generate alerts for
    // CQ00063056                      deactived AR persons. Fix also
    //                                 
    // applied to address KEES
    //                                 
    // Primary & Secondary Issues by
    //                                 
    // checking preferred id value
    //                                 
    // returned by external SREXIU50.
    // -----------------------------------------------------------------
    export.InterfaceAlert.AlertCode = import.InterfaceAlert.AlertCode ?? "";
    local.Current.Date = Now().Date;
    local.ApProvided.Flag = "N";

    // ******************************************************************************
    // *** CQ00063056:  The below listed read change will read the AR person 
    // info ***
    // ***              selected by user on the screen.  This will enable to
    // ***
    // ***              process deacticated AR person information.
    // ***
    // ******************************************************************************
    if (ReadCsePersonCaseRole())
    {
      export.InterfaceAlert.CsePersonNumber = entities.CsePerson.Number;
    }

    if (!entities.CsePerson.Populated)
    {
      ExitState = "CO0000_AR_NF";

      return;
    }

    local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
    UseCabReadAdabasPerson();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    // -----------------------------------------------------------
    // 07/19/99 W.Campbell - Added an extra and clause
    // to the following IF statement to also check for
    // the local_kscares ief_supplied flag = SPACES
    // before escaping out of the CAB.
    // -----------------------------------------------------------
    if (IsEmpty(local.Ae.Flag) && IsEmpty(local.Kscares.Flag))
    {
      // ******************************************************************************
      // *** The selected AR person number is not know to KEES (i.e. know only 
      // to   ***
      // *** Legacy System. Need to make sure client is associated with any 
      // primary/***
      // *** preferred Id, if so, make sure the preferred is known to KEES.
      // ***
      // ******************************************************************************
      // ******************************************************************************
      // *** CQ00063056:  Below new coded added to check the associated primary
      // ***
      // ***              client is associated with any KEES case or not in 
      // order to***
      // ***              generate the coop alerts to KEES. The above code 
      // verifies ***
      // ***              CSS client which is a secondary client to KEES and 
      // will   ***
      // ***              not be assoacited with any of the KEES Case.
      // ***
      // ******************************************************************************
      local.InputIds.Index = 0;
      local.InputIds.CheckSize();

      local.InputIds.Update.GlocalInputIds.Number = entities.CsePerson.Number;
      local.EabDmlFlag.Flag = "D";
      UseEabMaintainKeesSyncClient();

      local.List.Index = 0;
      local.List.CheckSize();

      if (!Equal(local.List.Item.GlocalPersonInfo.Number,
        local.List.Item.GlocalPreferredId.Number))
      {
        local.CsePersonsWorkSet.Number =
          local.List.Item.GlocalPreferredId.Number;
        UseCabReadAdabasPerson();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (IsEmpty(local.Ae.Flag) && IsEmpty(local.Kscares.Flag))
          {
            // ******************************************************************************
            // *** The selected CSS AR associated preferred ID is not Known to 
            // KEES as    ***
            // *** well person is not actively participating in any of the KEES 
            // cases.    ***
            // *** No need to generate KEES Alerts.
            // 
            // ***
            // ******************************************************************************
            return;
          }
        }
        else
        {
          ExitState = "ACO_NN0000_ALL_OK";
        }
      }
      else
      {
        // ******************************************************************************
        // *** The selected CSS AR is not associated with any KEES preferred ID.
        // ***
        // *** No need to generate KEES Alerts.
        // 
        // ***
        // ******************************************************************************
        return;
      }
    }

    // ---------------------------------------------------------------------
    //     AP NAME: Note that an AP does not have to be provided.
    // ---------------------------------------------------------------------
    if (!IsEmpty(import.Ap.Type1))
    {
      local.ApProvided.Flag = "Y";

      if (ReadCsePerson())
      {
        local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
        UseSiReadCsePerson();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          return;
        }
      }
      else
      {
        ExitState = "CO0000_AP_NF";

        return;
      }
    }

    if (AsChar(local.ApProvided.Flag) == 'Y')
    {
      local.Temp.Value =
        Substring(local.CsePersonsWorkSet.LastName,
        CsePersonsWorkSet.LastName_MaxLength, 1, 5) + " ";
      local.Temp.Value = (local.Temp.Value ?? "") + Substring
        (local.CsePersonsWorkSet.FirstName,
        CsePersonsWorkSet.FirstName_MaxLength, 1, 1);

      // ************* MOVE TEMP FIELD_VALUE TO FINAL FIELD_VALUE 
      // *****************
      local.LastPosition.Count = 0;
      local.Final.Value = local.Temp.Value ?? "";
    }
    else
    {
      local.LastPosition.Count = 0;
      local.Final.Value = "UNKNOWN";
    }

    if (AsChar(import.Case1.Status) == 'C')
    {
      ReadServiceProviderCaseAssignment();

      if (!entities.ServiceProvider.Populated)
      {
        ExitState = "CASE_ASSIGNMENT_NF_FOR_CASE";

        return;
      }
    }
    else if (!ReadServiceProvider())
    {
      ExitState = "CASE_ASSIGNMENT_NF_FOR_CASE";

      return;
    }

    local.Temp.Value =
      Substring(entities.ServiceProvider.LastName,
      ServiceProvider.LastName_MaxLength, 1, 5) + " ";
    local.Temp.Value = (local.Temp.Value ?? "") + Substring
      (entities.ServiceProvider.FirstName, ServiceProvider.FirstName_MaxLength,
      1, 1);

    // ************* MOVE TEMP FIELD_VALUE TO FINAL FIELD_VALUE 
    // *****************
    local.LastPosition.Count = 7;
    local.Final.Value =
      Substring(local.Final.Value, 1, local.LastPosition.Count) + (
        local.Temp.Value ?? "");

    // **************************************************************************
    // ---------------------------------------------------------------------
    //     REASON CODE
    // ---------------------------------------------------------------------
    if (Equal(import.InterfaceAlert.AlertCode, "40"))
    {
      local.Temp.Value = import.NonCooperation.Reason ?? "";

      // ************* MOVE TEMP FIELD_VALUE TO FINAL FIELD_VALUE 
      // *****************
      local.LastPosition.Count = 14;
      local.Final.Value =
        Substring(local.Final.Value, 1, local.LastPosition.Count) + (
          local.Temp.Value ?? "");

      // **************************************************************************
    }

    export.InterfaceAlert.NoteText = local.Final.Value ?? "";
    UseSpCreateOutgoingExtAlert();
  }

  private static void MoveInputIds(Local.InputIdsGroup source,
    EabMaintainKeesSyncClient.Import.IdsGroup target)
  {
    target.GimportInputIds.Number = source.GlocalInputIds.Number;
  }

  private static void MoveList1(EabMaintainKeesSyncClient.Export.
    ListGroup source, Local.ListGroup target)
  {
    target.G.SelectChar = source.G.SelectChar;
    target.GlocalPersonInfo.Assign(source.GexportPersonInfo);
    target.GlocalAeFlag.Flag = source.GexportAeFlag.Flag;
    target.GlocalCsFlag.Flag = source.GexportCsFlag.Flag;
    target.GlocalFaFlag.Flag = source.GexportFaFlag.Flag;
    target.GlocalKmFlag.Flag = source.GexportKmFlag.Flag;
    target.GlocalPreferredId.Number = source.GexportPreferredId.Number;
  }

  private static void MoveList2(Local.ListGroup source,
    EabMaintainKeesSyncClient.Import.ListGroup target)
  {
    target.G.SelectChar = source.G.SelectChar;
    target.GimportPersonInfo.Assign(source.GlocalPersonInfo);
    target.GimportAeFlag.Flag = source.GlocalAeFlag.Flag;
    target.GimportCsFlag.Flag = source.GlocalCsFlag.Flag;
    target.GimportFaFlag.Flag = source.GlocalFaFlag.Flag;
    target.GimportKmFlag.Flag = source.GlocalKmFlag.Flag;
    target.GimportPreferredId.Number = source.GlocalPreferredId.Number;
  }

  private static void MoveList3(Local.ListGroup source,
    EabMaintainKeesSyncClient.Export.ListGroup target)
  {
    target.G.SelectChar = source.G.SelectChar;
    target.GexportPersonInfo.Assign(source.GlocalPersonInfo);
    target.GexportAeFlag.Flag = source.GlocalAeFlag.Flag;
    target.GexportCsFlag.Flag = source.GlocalCsFlag.Flag;
    target.GexportFaFlag.Flag = source.GlocalFaFlag.Flag;
    target.GexportKmFlag.Flag = source.GlocalKmFlag.Flag;
    target.GexportPreferredId.Number = source.GlocalPreferredId.Number;
  }

  private void UseCabReadAdabasPerson()
  {
    var useImport = new CabReadAdabasPerson.Import();
    var useExport = new CabReadAdabasPerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(CabReadAdabasPerson.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    local.ReadCsePerson.Type1 = useExport.AbendData.Type1;
    local.Ae.Flag = useExport.Ae.Flag;
    local.Kscares.Flag = useExport.Kscares.Flag;
  }

  private void UseEabMaintainKeesSyncClient()
  {
    var useImport = new EabMaintainKeesSyncClient.Import();
    var useExport = new EabMaintainKeesSyncClient.Export();

    local.List.CopyTo(useImport.List, MoveList2);
    local.InputIds.CopyTo(useImport.Ids, MoveInputIds);
    useImport.UpdatePreferredId.Number = local.UpdatePreferredId.Number;
    useImport.UpdatePersonWorkSet.Assign(local.PrimaryPersonWorkSet);
    useImport.EabDmlFlag.Flag = local.EabDmlFlag.Flag;
    useExport.DmlReturnCode.Count = local.DmlReturnCode.Count;
    local.List.CopyTo(useExport.List, MoveList3);

    Call(EabMaintainKeesSyncClient.Execute, useImport, useExport);

    local.DmlReturnCode.Count = useExport.DmlReturnCode.Count;
    useExport.List.CopyTo(local.List, MoveList1);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    local.ReadCsePerson.Type1 = useExport.AbendData.Type1;
  }

  private void UseSpCreateOutgoingExtAlert()
  {
    var useImport = new SpCreateOutgoingExtAlert.Import();
    var useExport = new SpCreateOutgoingExtAlert.Export();

    useImport.KscParticipation.Flag = local.Kscares.Flag;
    useImport.InterfaceAlert.Assign(export.InterfaceAlert);

    Call(SpCreateOutgoingExtAlert.Execute, useImport, useExport);
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetInt32(command, "caseRoleId", import.Ap.Identifier);
        db.SetString(command, "type", import.Ap.Type1);
        db.SetString(command, "casNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadCsePersonCaseRole()
  {
    entities.CaseRole.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadCsePersonCaseRole",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDate", import.Ar.StartDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "startDate", import.Ar.EndDate.GetValueOrDefault());
        db.SetString(command, "casNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 0);
        entities.CaseRole.CasNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadServiceProvider()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetString(command, "casNo", import.Case1.Number);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.LastName = db.GetString(reader, 1);
        entities.ServiceProvider.FirstName = db.GetString(reader, 2);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 3);
        entities.ServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProviderCaseAssignment()
  {
    entities.CaseAssignment.Populated = false;
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProviderCaseAssignment",
      (db, command) =>
      {
        db.SetString(command, "casNo", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.LastName = db.GetString(reader, 1);
        entities.ServiceProvider.FirstName = db.GetString(reader, 2);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 3);
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 4);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 5);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 6);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 8);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 9);
        entities.CaseAssignment.OspCode = db.GetString(reader, 10);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 11);
        entities.CaseAssignment.CasNo = db.GetString(reader, 12);
        entities.CaseAssignment.Populated = true;
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
    /// A value of NonCooperation.
    /// </summary>
    [JsonPropertyName("nonCooperation")]
    public NonCooperation NonCooperation
    {
      get => nonCooperation ??= new();
      set => nonCooperation = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CaseRole Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of InterfaceAlert.
    /// </summary>
    [JsonPropertyName("interfaceAlert")]
    public InterfaceAlert InterfaceAlert
    {
      get => interfaceAlert ??= new();
      set => interfaceAlert = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CaseRole Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    private NonCooperation nonCooperation;
    private Case1 case1;
    private CaseRole ap;
    private InterfaceAlert interfaceAlert;
    private CaseRole ar;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of InterfaceAlert.
    /// </summary>
    [JsonPropertyName("interfaceAlert")]
    public InterfaceAlert InterfaceAlert
    {
      get => interfaceAlert ??= new();
      set => interfaceAlert = value;
    }

    private InterfaceAlert interfaceAlert;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A ListGroup group.</summary>
    [Serializable]
    public class ListGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public Common G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>
      /// A value of GlocalPersonInfo.
      /// </summary>
      [JsonPropertyName("glocalPersonInfo")]
      public CsePersonsWorkSet GlocalPersonInfo
      {
        get => glocalPersonInfo ??= new();
        set => glocalPersonInfo = value;
      }

      /// <summary>
      /// A value of GlocalAeFlag.
      /// </summary>
      [JsonPropertyName("glocalAeFlag")]
      public Common GlocalAeFlag
      {
        get => glocalAeFlag ??= new();
        set => glocalAeFlag = value;
      }

      /// <summary>
      /// A value of GlocalCsFlag.
      /// </summary>
      [JsonPropertyName("glocalCsFlag")]
      public Common GlocalCsFlag
      {
        get => glocalCsFlag ??= new();
        set => glocalCsFlag = value;
      }

      /// <summary>
      /// A value of GlocalFaFlag.
      /// </summary>
      [JsonPropertyName("glocalFaFlag")]
      public Common GlocalFaFlag
      {
        get => glocalFaFlag ??= new();
        set => glocalFaFlag = value;
      }

      /// <summary>
      /// A value of GlocalKmFlag.
      /// </summary>
      [JsonPropertyName("glocalKmFlag")]
      public Common GlocalKmFlag
      {
        get => glocalKmFlag ??= new();
        set => glocalKmFlag = value;
      }

      /// <summary>
      /// A value of GlocalPreferredId.
      /// </summary>
      [JsonPropertyName("glocalPreferredId")]
      public CsePersonsWorkSet GlocalPreferredId
      {
        get => glocalPreferredId ??= new();
        set => glocalPreferredId = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Common g;
      private CsePersonsWorkSet glocalPersonInfo;
      private Common glocalAeFlag;
      private Common glocalCsFlag;
      private Common glocalFaFlag;
      private Common glocalKmFlag;
      private CsePersonsWorkSet glocalPreferredId;
    }

    /// <summary>A InputIdsGroup group.</summary>
    [Serializable]
    public class InputIdsGroup
    {
      /// <summary>
      /// A value of GlocalInputIds.
      /// </summary>
      [JsonPropertyName("glocalInputIds")]
      public CsePersonsWorkSet GlocalInputIds
      {
        get => glocalInputIds ??= new();
        set => glocalInputIds = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private CsePersonsWorkSet glocalInputIds;
    }

    /// <summary>
    /// A value of DmlReturnCode.
    /// </summary>
    [JsonPropertyName("dmlReturnCode")]
    public Common DmlReturnCode
    {
      get => dmlReturnCode ??= new();
      set => dmlReturnCode = value;
    }

    /// <summary>
    /// Gets a value of List.
    /// </summary>
    [JsonIgnore]
    public Array<ListGroup> List => list ??= new(ListGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of List for json serialization.
    /// </summary>
    [JsonPropertyName("list")]
    [Computed]
    public IList<ListGroup> List_Json
    {
      get => list;
      set => List.Assign(value);
    }

    /// <summary>
    /// Gets a value of InputIds.
    /// </summary>
    [JsonIgnore]
    public Array<InputIdsGroup> InputIds => inputIds ??= new(
      InputIdsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of InputIds for json serialization.
    /// </summary>
    [JsonPropertyName("inputIds")]
    [Computed]
    public IList<InputIdsGroup> InputIds_Json
    {
      get => inputIds;
      set => InputIds.Assign(value);
    }

    /// <summary>
    /// A value of UpdatePreferredId.
    /// </summary>
    [JsonPropertyName("updatePreferredId")]
    public CsePersonsWorkSet UpdatePreferredId
    {
      get => updatePreferredId ??= new();
      set => updatePreferredId = value;
    }

    /// <summary>
    /// A value of PrimaryPersonWorkSet.
    /// </summary>
    [JsonPropertyName("primaryPersonWorkSet")]
    public CsePersonsWorkSet PrimaryPersonWorkSet
    {
      get => primaryPersonWorkSet ??= new();
      set => primaryPersonWorkSet = value;
    }

    /// <summary>
    /// A value of EabDmlFlag.
    /// </summary>
    [JsonPropertyName("eabDmlFlag")]
    public Common EabDmlFlag
    {
      get => eabDmlFlag ??= new();
      set => eabDmlFlag = value;
    }

    /// <summary>
    /// A value of ApProvided.
    /// </summary>
    [JsonPropertyName("apProvided")]
    public Common ApProvided
    {
      get => apProvided ??= new();
      set => apProvided = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ReadCsePerson.
    /// </summary>
    [JsonPropertyName("readCsePerson")]
    public AbendData ReadCsePerson
    {
      get => readCsePerson ??= new();
      set => readCsePerson = value;
    }

    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public FieldValue Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    /// <summary>
    /// A value of LastPosition.
    /// </summary>
    [JsonPropertyName("lastPosition")]
    public Common LastPosition
    {
      get => lastPosition ??= new();
      set => lastPosition = value;
    }

    /// <summary>
    /// A value of Final.
    /// </summary>
    [JsonPropertyName("final")]
    public FieldValue Final
    {
      get => final ??= new();
      set => final = value;
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
    /// A value of Kscares.
    /// </summary>
    [JsonPropertyName("kscares")]
    public Common Kscares
    {
      get => kscares ??= new();
      set => kscares = value;
    }

    private Common dmlReturnCode;
    private Array<ListGroup> list;
    private Array<InputIdsGroup> inputIds;
    private CsePersonsWorkSet updatePreferredId;
    private CsePersonsWorkSet primaryPersonWorkSet;
    private Common eabDmlFlag;
    private Common apProvided;
    private DateWorkArea current;
    private CsePersonsWorkSet csePersonsWorkSet;
    private AbendData readCsePerson;
    private FieldValue temp;
    private Common lastPosition;
    private FieldValue final;
    private Common ae;
    private Common kscares;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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

    private CaseAssignment caseAssignment;
    private Case1 case1;
    private CaseRole caseRole;
    private CsePerson csePerson;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
  }
#endregion
}
