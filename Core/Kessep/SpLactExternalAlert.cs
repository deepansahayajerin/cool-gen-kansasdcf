// Program: SP_LACT_EXTERNAL_ALERT, ID: 371986350, model: 746.
// Short name: SWE01880
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_LACT_EXTERNAL_ALERT.
/// </summary>
[Serializable]
public partial class SpLactExternalAlert: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_LACT_EXTERNAL_ALERT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpLactExternalAlert(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpLactExternalAlert.
  /// </summary>
  public SpLactExternalAlert(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------------
    // ALERT CODE:  42
    // ALERT TEXT:  OSP NAME        1-  7
    // ALERT CODE:  43
    // ALERT TEXT:  AMOUNT          1-  6
    //              CHILD NAME      7- 13
    //              CO ORD EFF DT  14- 17
    //              AP NAME        18- 24
    //              OSP NAME       25- 31
    // ------------------------------------------------------------------
    // ********************************************
    // 12/09/97  R Grey	H00033115 Change concat text from
    // 			le_dtl to le_act_pers $
    // ********************************************
    export.InterfaceAlert.AlertCode = import.InterfaceAlert.AlertCode ?? "";

    if (ReadCsePerson2())
    {
      export.InterfaceAlert.CsePersonNumber = entities.Ar.Number;
    }
    else
    {
      ExitState = "CO0000_AR_NF";

      return;
    }

    local.CsePersonsWorkSet.Number = entities.Ar.Number;
    UseCabReadAdabasPerson();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    if (IsEmpty(local.Ae.Flag))
    {
      return;
    }

    if (Equal(import.InterfaceAlert.AlertCode, "43"))
    {
      // ---------------------------------------------------------------------
      //     AMOUNT
      // ---------------------------------------------------------------------
      if (import.LegalActionPerson.CurrentAmount.GetValueOrDefault() > 0)
      {
        local.Temp.Value =
          NumberToString((long)import.LegalActionPerson.CurrentAmount.
            GetValueOrDefault(), 10, 6);
      }
      else if (import.LegalActionPerson.ArrearsAmount.GetValueOrDefault() > 0)
      {
        local.Temp.Value =
          NumberToString((long)import.LegalActionPerson.ArrearsAmount.
            GetValueOrDefault(), 10, 6);
      }
      else if (import.LegalActionPerson.JudgementAmount.GetValueOrDefault() > 0)
      {
        local.Temp.Value =
          NumberToString((long)import.LegalActionPerson.JudgementAmount.
            GetValueOrDefault(), 10, 6);
      }
      else
      {
        local.Temp.Value = "      ";
      }

      // ************* MOVE TEMP FIELD_VALUE TO FINAL FIELD_VALUE 
      // *****************
      local.LastPosition.Count = 0;
      local.Final.Value = local.Temp.Value ?? "";

      // **************************************************************************
      if (!ReadCsePerson1())
      {
        ExitState = "CO0000_AP_NF";

        return;
      }

      if (!ReadCsePerson3())
      {
        ExitState = "CO0000_CHILD_NF";

        return;
      }

      // ---------------------------------------------------------------------
      // CORRECT FORMAT FOR ALL NAMES:
      //         LLLLL F
      //   WHERE LLLLL = FIRST 5 CHARS OF LAST NAME
      //             F = FIRST NAME INIT
      // ---------------------------------------------------------------------
      // ---------------------------------------------------------------------
      //     CHILD NAME
      // ---------------------------------------------------------------------
      local.CsePersonsWorkSet.Number = entities.Child.Number;
      UseSiReadCsePerson();

      if (!IsEmpty(local.ReadCsePerson.Type1))
      {
        ExitState = "CASE_ROLE_CHILD_NF";

        return;
      }

      local.Temp.Value =
        Substring(local.CsePersonsWorkSet.LastName,
        CsePersonsWorkSet.LastName_MaxLength, 1, 5) + " ";
      local.Temp.Value = (local.Temp.Value ?? "") + Substring
        (local.CsePersonsWorkSet.FirstName,
        CsePersonsWorkSet.FirstName_MaxLength, 1, 1);

      // ************* MOVE TEMP FIELD_VALUE TO FINAL FIELD_VALUE 
      // *****************
      local.LastPosition.Count = 6;
      local.Final.Value =
        Substring(local.Final.Value, 1, local.LastPosition.Count) + (
          local.Temp.Value ?? "");

      // **************************************************************************
      // ---------------------------------------------------------------------
      //     FILED DATE  (MMYY)
      // ---------------------------------------------------------------------
      local.BatchTimestampWorkArea.TextDate =
        NumberToString(DateToInt(import.LegalAction.FiledDate), 8);
      local.BatchTimestampWorkArea.TextDateMm =
        Substring(local.BatchTimestampWorkArea.TextDate, 5, 2);

      // *********************************************
      // The local test_date_dd is being used to capture the last 2 digits of 
      // the year instead of the 4 position test_date_yyyy.
      // *********************************************
      local.BatchTimestampWorkArea.TestDateDd =
        Substring(local.BatchTimestampWorkArea.TextDate, 3, 2);
      local.Temp.Value = local.BatchTimestampWorkArea.TextDateMm + local
        .BatchTimestampWorkArea.TestDateDd;

      // ************* MOVE TEMP FIELD_VALUE TO FINAL FIELD_VALUE 
      // *****************
      local.LastPosition.Count = 13;
      local.Final.Value =
        Substring(local.Final.Value, 1, local.LastPosition.Count) + (
          local.Temp.Value ?? "");

      // **************************************************************************
      // ---------------------------------------------------------------------
      //     AP NAME
      // ---------------------------------------------------------------------
      local.CsePersonsWorkSet.Number = entities.Ap.Number;
      UseSiReadCsePerson();

      if (!IsEmpty(local.ReadCsePerson.Type1))
      {
        ExitState = "SP0000_AP_XTRNL_DTLS_NF";

        return;
      }

      local.Temp.Value =
        Substring(local.CsePersonsWorkSet.LastName,
        CsePersonsWorkSet.LastName_MaxLength, 1, 5) + " ";
      local.Temp.Value = (local.Temp.Value ?? "") + Substring
        (local.CsePersonsWorkSet.FirstName,
        CsePersonsWorkSet.FirstName_MaxLength, 1, 1);

      // ************* MOVE TEMP FIELD_VALUE TO FINAL FIELD_VALUE 
      // *****************
      local.LastPosition.Count = 17;
      local.Final.Value =
        Substring(local.Final.Value, 1, local.LastPosition.Count) + (
          local.Temp.Value ?? "");

      // **************************************************************************
    }

    // ---------------------------------------------------------------------
    //     SERVICE PROVIDER
    // ---------------------------------------------------------------------
    UseSpCabDetOspAssgndToCsecase();

    if (local.ServiceProvider.SystemGeneratedId == 0)
    {
      ExitState = "ZD_SERVICE_PROVIDER_NF_1";

      return;
    }

    local.Temp.Value =
      Substring(local.ServiceProvider.LastName,
      ServiceProvider.LastName_MaxLength, 1, 5) + " ";
    local.Temp.Value = (local.Temp.Value ?? "") + Substring
      (local.ServiceProvider.FirstName, ServiceProvider.FirstName_MaxLength, 1,
      1);

    // ************* MOVE TEMP FIELD_VALUE TO FINAL FIELD_VALUE 
    // *****************
    if (Equal(import.InterfaceAlert.AlertCode, "43"))
    {
      local.LastPosition.Count = 24;
      local.Final.Value =
        Substring(local.Final.Value, 1, local.LastPosition.Count) + (
          local.Temp.Value ?? "");
    }
    else
    {
      local.LastPosition.Count = 0;
      local.Final.Value = local.Temp.Value ?? "";
    }

    // **************************************************************************
    export.InterfaceAlert.NoteText = local.Final.Value ?? "";
    UseSpCreateOutgoingExtAlert();
  }

  private void UseCabReadAdabasPerson()
  {
    var useImport = new CabReadAdabasPerson.Import();
    var useExport = new CabReadAdabasPerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(CabReadAdabasPerson.Execute, useImport, useExport);

    local.ReadCsePerson.Type1 = useExport.AbendData.Type1;
    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    local.Ae.Flag = useExport.Ae.Flag;
    local.Kscares.Flag = useExport.Kscares.Flag;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.ReadCsePerson.Type1 = useExport.AbendData.Type1;
    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSpCabDetOspAssgndToCsecase()
  {
    var useImport = new SpCabDetOspAssgndToCsecase.Import();
    var useExport = new SpCabDetOspAssgndToCsecase.Export();

    useImport.Case1.Number = import.Case1.Number;

    Call(SpCabDetOspAssgndToCsecase.Execute, useImport, useExport);

    local.ServiceProvider.Assign(useExport.ServiceProvider);
  }

  private void UseSpCreateOutgoingExtAlert()
  {
    var useImport = new SpCreateOutgoingExtAlert.Import();
    var useExport = new SpCreateOutgoingExtAlert.Export();

    useImport.KscParticipation.Flag = local.Kscares.Flag;
    useImport.InterfaceAlert.Assign(export.InterfaceAlert);

    Call(SpCreateOutgoingExtAlert.Execute, useImport, useExport);
  }

  private bool ReadCsePerson1()
  {
    entities.Ap.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "casNo", import.Case1.Number);
        db.SetInt32(command, "cuNumber", import.CaseUnit.CuNumber);
      },
      (db, reader) =>
      {
        entities.Ap.Number = db.GetString(reader, 0);
        entities.Ap.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    entities.Ar.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetInt32(command, "cuNumber", import.CaseUnit.CuNumber);
        db.SetString(command, "casNo", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Ar.Number = db.GetString(reader, 0);
        entities.Ar.Populated = true;
      });
  }

  private bool ReadCsePerson3()
  {
    entities.Child.Populated = false;

    return Read("ReadCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "casNo", import.Case1.Number);
        db.SetInt32(command, "cuNumber", import.CaseUnit.CuNumber);
      },
      (db, reader) =>
      {
        entities.Child.Number = db.GetString(reader, 0);
        entities.Child.Populated = true;
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
    /// A value of InterfaceAlert.
    /// </summary>
    [JsonPropertyName("interfaceAlert")]
    public InterfaceAlert InterfaceAlert
    {
      get => interfaceAlert ??= new();
      set => interfaceAlert = value;
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
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
    }

    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
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

    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    private InterfaceAlert interfaceAlert;
    private Case1 case1;
    private CaseUnit caseUnit;
    private LegalActionDetail legalActionDetail;
    private LegalAction legalAction;
    private LegalActionPerson legalActionPerson;
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
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("batchTimestampWorkArea")]
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
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
    /// A value of ReadCsePerson.
    /// </summary>
    [JsonPropertyName("readCsePerson")]
    public AbendData ReadCsePerson
    {
      get => readCsePerson ??= new();
      set => readCsePerson = value;
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
    /// A value of Final.
    /// </summary>
    [JsonPropertyName("final")]
    public FieldValue Final
    {
      get => final ??= new();
      set => final = value;
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

    private ServiceProvider serviceProvider;
    private BatchTimestampWorkArea batchTimestampWorkArea;
    private Common lastPosition;
    private AbendData readCsePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private FieldValue final;
    private FieldValue temp;
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
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

    /// <summary>
    /// A value of LegalActionAssigment.
    /// </summary>
    [JsonPropertyName("legalActionAssigment")]
    public LegalActionAssigment LegalActionAssigment
    {
      get => legalActionAssigment ??= new();
      set => legalActionAssigment = value;
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
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CsePerson Child
    {
      get => child ??= new();
      set => child = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
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
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
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

    private CsePerson ar;
    private LegalAction legalAction;
    private LegalActionAssigment legalActionAssigment;
    private ServiceProvider serviceProvider;
    private CsePerson child;
    private CsePerson ap;
    private OfficeServiceProvider officeServiceProvider;
    private CaseUnit caseUnit;
    private Case1 case1;
  }
#endregion
}
