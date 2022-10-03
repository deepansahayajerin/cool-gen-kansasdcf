// Program: SP_ADDR_EXTERNAL_ALERT, ID: 371735360, model: 746.
// Short name: SWE01865
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_ADDR_EXTERNAL_ALERT.
/// </summary>
[Serializable]
public partial class SpAddrExternalAlert: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_ADDR_EXTERNAL_ALERT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpAddrExternalAlert(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpAddrExternalAlert.
  /// </summary>
  public SpAddrExternalAlert(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.CsePersonsWorkSet.Number = import.CsePerson.Number;
    UseCabReadAdabasPerson();

    if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
      ("ACO_NI0000_SUCCESSFUL_ADD") || IsExitState
      ("ACO_NI0000_SUCCESSFUL_UPDATE"))
    {
    }
    else
    {
      return;
    }

    export.InterfaceAlert.AlertCode = import.InterfaceAlert.AlertCode ?? "";
    export.InterfaceAlert.CsePersonNumber = import.CsePerson.Number;

    // MJR ****** AE / KSCARES MUST 'KNOW' THIS PERSON ******
    if (IsEmpty(local.Ae.Flag) && IsEmpty(local.Kscares.Flag))
    {
      return;
    }

    // for domestic address
    // ------------------------------------------------
    // ALERT CODE:  45
    // ALERT TEXT:  STREET-1        1- 25
    //              STREET-2       26- 50
    //              CITY           51- 65
    //              STATE          66- 67
    //              ZIP            68- 72
    // --------------------------------------------------------------------
    // for foreign address
    // -------------------------------------------------
    // ALERT CODE:  45
    // ALERT TEXT:  STREET-1        1- 25
    //              STREET-2       26- 50
    //              CITY           51- 65
    //              COUNTRY        66- 67
    //              POSTAL CODE    68- 72
    // --------------------------------------------------------------------
    // ---------------------------------------------------------------------
    //     ADDRESS
    // ---------------------------------------------------------------------
    local.Temp.Value = import.CsePersonAddress.Street1 ?? "";

    // ************* MOVE TEMP FIELD_VALUE TO FINAL FIELD_VALUE 
    // *****************
    local.LastPosition.Count = 0;
    local.Final.Value = local.Temp.Value ?? "";
    local.Temp.Value = import.CsePersonAddress.Street2 ?? "";

    // ************* MOVE TEMP FIELD_VALUE TO FINAL FIELD_VALUE 
    // *****************
    local.LastPosition.Count = 25;
    local.Final.Value =
      Substring(local.Final.Value, 1, local.LastPosition.Count) + (
        local.Temp.Value ?? "");
    local.Temp.Value = import.CsePersonAddress.City ?? "";

    // ************* MOVE TEMP FIELD_VALUE TO FINAL FIELD_VALUE 
    // *****************
    local.LastPosition.Count = 50;
    local.Final.Value =
      Substring(local.Final.Value, 1, local.LastPosition.Count) + (
        local.Temp.Value ?? "");

    if (AsChar(import.CsePersonAddress.LocationType) == 'D')
    {
      // The CSE Person address is domestic. Use State code and zip code.
      local.Temp.Value = import.CsePersonAddress.State ?? "";

      // ************* MOVE TEMP FIELD_VALUE TO FINAL FIELD_VALUE 
      // *****************
      local.LastPosition.Count = 65;
      local.Final.Value =
        Substring(local.Final.Value, 1, local.LastPosition.Count) + (
          local.Temp.Value ?? "");
      local.Temp.Value = import.CsePersonAddress.ZipCode ?? "";

      // ************* MOVE TEMP FIELD_VALUE TO FINAL FIELD_VALUE 
      // *****************
      local.LastPosition.Count = 67;
      local.Final.Value =
        Substring(local.Final.Value, 1, local.LastPosition.Count) + (
          local.Temp.Value ?? "");
    }

    if (AsChar(import.CsePersonAddress.LocationType) == 'F')
    {
      // The CSE Person address is foreign. Use Country code and postal code.
      local.Temp.Value = import.CsePersonAddress.Country ?? "";

      // ************* MOVE TEMP FIELD_VALUE TO FINAL FIELD_VALUE 
      // *****************
      local.LastPosition.Count = 65;
      local.Final.Value =
        Substring(local.Final.Value, 1, local.LastPosition.Count) + (
          local.Temp.Value ?? "");
      local.Temp.Value = import.CsePersonAddress.PostalCode ?? "";

      // ************* MOVE TEMP FIELD_VALUE TO FINAL FIELD_VALUE 
      // *****************
      local.LastPosition.Count = 67;
      local.Final.Value =
        Substring(local.Final.Value, 1, local.LastPosition.Count) + (
          local.Temp.Value ?? "");
    }

    export.InterfaceAlert.NoteText = local.Final.Value ?? "";
    UseSpCreateOutgoingExtAlert();
  }

  private void UseCabReadAdabasPerson()
  {
    var useImport = new CabReadAdabasPerson.Import();
    var useExport = new CabReadAdabasPerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(CabReadAdabasPerson.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    local.Ae.Flag = useExport.Ae.Flag;
    local.Kscares.Flag = useExport.Kscares.Flag;
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
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
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
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
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

    private CsePerson csePerson;
    private CsePersonAddress csePersonAddress;
    private InterfaceAlert interfaceAlert;
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
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public FieldValue Temp
    {
      get => temp ??= new();
      set => temp = value;
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
    /// A value of LastPosition.
    /// </summary>
    [JsonPropertyName("lastPosition")]
    public Common LastPosition
    {
      get => lastPosition ??= new();
      set => lastPosition = value;
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

    /// <summary>
    /// A value of ReadCsePerson.
    /// </summary>
    [JsonPropertyName("readCsePerson")]
    public AbendData ReadCsePerson
    {
      get => readCsePerson ??= new();
      set => readCsePerson = value;
    }

    private FieldValue temp;
    private FieldValue final;
    private Common lastPosition;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common ae;
    private Common kscares;
    private AbendData readCsePerson;
  }
#endregion
}
