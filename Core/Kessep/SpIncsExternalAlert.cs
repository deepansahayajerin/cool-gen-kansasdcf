// Program: SP_INCS_EXTERNAL_ALERT, ID: 371763124, model: 746.
// Short name: SWE01866
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_INCS_EXTERNAL_ALERT.
/// </para>
/// <para>
/// This cab determines whether an alert needs to be sent.
/// AE Alert 46  Employer Info
/// </para>
/// </summary>
[Serializable]
public partial class SpIncsExternalAlert: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_INCS_EXTERNAL_ALERT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpIncsExternalAlert(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpIncsExternalAlert.
  /// </summary>
  public SpIncsExternalAlert(IContext context, Import import, Export export):
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
    //     M A I N T E N A N C E    L O G
    //   Date   Developer    Description
    // ??-??-?? ???          Initial Development
    // 10/31/98 W. Campbell  Modified an IF statement
    //                       to use the location_type instead
    //                       of the identifier of
    //                       NON_EMPLOY_INCOME_SOURCE_ADDR
    //                       in order to delete the
    //                       un-needed identifier.
    // ---------------------------------------------
    // Do not send alert if income source information originated from the DHR 
    // interface batch job,
    // indicated by UserId SWEI260B in the Income Source UserId attribute.
    if (ReadCsePerson())
    {
      if (ReadIncomeSource())
      {
        if (Equal(entities.IncomeSource.CreatedBy, "SWEI260B") || Equal
          (entities.IncomeSource.LastUpdatedBy, "SWEI260B"))
        {
          // Do not generate the outgoing external alert.
          return;
        }
      }
      else
      {
        ExitState = "ZD_INCOME_SOURCE_NF_2";

        return;
      }
    }
    else
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    export.InterfaceAlert.CsePersonNumber = import.CsePerson.Number;
    export.InterfaceAlert.AlertCode = "46";
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

    if (IsEmpty(local.Ae.Flag))
    {
      // AE and KsCares are not "using" the target CSE Person, so do not send 
      // the alert.
      return;
    }

    // *********************************************************************
    // GET AP NAME INTO CORRECT FORMAT:
    //         LLLLL F
    //   WHERE LLLLL = FIRST 5 CHARS OF LAST NAME
    //             F = FIRST NAME INIT
    // *********************************************************************
    local.Final.Value =
      Substring(local.CsePersonsWorkSet.LastName,
      CsePersonsWorkSet.LastName_MaxLength, 1, 5) + " ";
    local.Final.Value = (local.Final.Value ?? "") + Substring
      (local.CsePersonsWorkSet.FirstName, CsePersonsWorkSet.FirstName_MaxLength,
      1, 1);
    local.LastPosition.Count = 7;

    // GET EMPLOYER ADDRESS
    // --------------------------------------------------------------------
    // ALERT CODE:  46 (AND 66)
    // ALERT TEXT:  AP NAME         1-  7
    //              EMPLOYER NAME   8- 37
    //              STREET-1       38- 62
    //              STREET-2       63- 87
    //              CITY           88-102
    //              STATE         103-104
    //              ZIP           105-109
    // --------------------------------------------------------------------
    // ----------------------------------------------------------
    // 10/31/98 W. Campbell - Modified the following
    // IF statement to use the location_type instead
    // of the identifier of NON_EMPLOY_INCOME_SOURCE_ADDR
    // in order to delete the un-needed identifier.
    // ----------------------------------------------------------
    if (!IsEmpty(import.NonEmployIncomeSourceAddress.LocationType))
    {
      local.LastPosition.Count = 37;
      local.Temp.Value = import.NonEmployIncomeSourceAddress.Street1 ?? "";
      local.Final.Value =
        Substring(local.Final.Value, 1, local.LastPosition.Count) + (
          local.Temp.Value ?? "");
      local.LastPosition.Count = 62;
      local.Temp.Value = import.NonEmployIncomeSourceAddress.Street2 ?? "";
      local.Final.Value =
        Substring(local.Final.Value, 1, local.LastPosition.Count) + (
          local.Temp.Value ?? "");
      local.LastPosition.Count = 87;
      local.Temp.Value = import.NonEmployIncomeSourceAddress.City ?? "";
      local.Final.Value =
        Substring(local.Final.Value, 1, local.LastPosition.Count) + (
          local.Temp.Value ?? "");
      local.LastPosition.Count = 102;

      if (AsChar(import.NonEmployIncomeSourceAddress.LocationType) == 'D')
      {
        local.Temp.Value = import.NonEmployIncomeSourceAddress.State ?? "";
        local.Final.Value =
          Substring(local.Final.Value, 1, local.LastPosition.Count) + (
            local.Temp.Value ?? "");
        local.LastPosition.Count = 104;
        local.Temp.Value = import.NonEmployIncomeSourceAddress.ZipCode ?? "";
        local.Final.Value =
          Substring(local.Final.Value, 1, local.LastPosition.Count) + (
            local.Temp.Value ?? "");
      }

      if (AsChar(import.NonEmployIncomeSourceAddress.LocationType) == 'F')
      {
        local.Temp.Value = import.NonEmployIncomeSourceAddress.Country ?? "";
        local.Final.Value =
          Substring(local.Final.Value, 1, local.LastPosition.Count) + (
            local.Temp.Value ?? "");
        local.LastPosition.Count = 104;
        local.Temp.Value =
          Substring(import.NonEmployIncomeSourceAddress.PostalCode, 10, 1, 5);
        local.Final.Value =
          Substring(local.Final.Value, 1, local.LastPosition.Count) + (
            local.Temp.Value ?? "");
      }

      local.LastPosition.Count = 109;
    }
    else if (import.Employer.Identifier != 0)
    {
      // GET EMPLOYER NAME
      if (ReadEmployer())
      {
        local.Temp.Value = entities.Employer.Name;
        local.Final.Value =
          Substring(local.Final.Value, 1, local.LastPosition.Count) + (
            local.Temp.Value ?? "");
        local.LastPosition.Count = 37;
      }
      else
      {
        ExitState = "EMPLOYER_NF";

        return;
      }

      ReadEmployerAddress();

      if (!entities.EmployerAddress.Populated)
      {
        ExitState = "EMPLOYER_ADDRESS_NF";

        return;
      }

      local.Temp.Value = entities.EmployerAddress.Street1;
      local.Final.Value =
        Substring(local.Final.Value, 1, local.LastPosition.Count) + (
          local.Temp.Value ?? "");
      local.LastPosition.Count = 62;
      local.Temp.Value = entities.EmployerAddress.Street2;
      local.Final.Value =
        Substring(local.Final.Value, 1, local.LastPosition.Count) + (
          local.Temp.Value ?? "");
      local.LastPosition.Count = 87;
      local.Temp.Value = entities.EmployerAddress.City;
      local.Final.Value =
        Substring(local.Final.Value, 1, local.LastPosition.Count) + (
          local.Temp.Value ?? "");
      local.LastPosition.Count = 102;

      if (AsChar(entities.EmployerAddress.LocationType) == 'D')
      {
        local.Temp.Value = entities.EmployerAddress.State;
        local.Final.Value =
          Substring(local.Final.Value, 1, local.LastPosition.Count) + (
            local.Temp.Value ?? "");
        local.LastPosition.Count = 104;
        local.Temp.Value = entities.EmployerAddress.ZipCode;
        local.Final.Value =
          Substring(local.Final.Value, 1, local.LastPosition.Count) + (
            local.Temp.Value ?? "");
        local.LastPosition.Count = 109;
      }

      if (AsChar(entities.EmployerAddress.LocationType) == 'F')
      {
        local.Temp.Value = entities.EmployerAddress.Country;
        local.Final.Value =
          Substring(local.Final.Value, 1, local.LastPosition.Count) + (
            local.Temp.Value ?? "");
        local.LastPosition.Count = 104;
        local.Temp.Value =
          Substring(entities.EmployerAddress.PostalCode,
          EmployerAddress.PostalCode_MaxLength, 1, 5);
        local.Final.Value =
          Substring(local.Final.Value, 1, local.LastPosition.Count) + (
            local.Temp.Value ?? "");
        local.LastPosition.Count = 109;
      }
    }
    else
    {
      return;
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
    local.ReadCsePerson.Type1 = useExport.AbendData.Type1;
    local.Ae.Flag = useExport.Ae.Flag;
    local.Kscares.Flag = useExport.Kscares.Flag;
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
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadEmployer()
  {
    entities.Employer.Populated = false;

    return Read("ReadEmployer",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.Employer.Identifier);
      },
      (db, reader) =>
      {
        entities.Employer.Identifier = db.GetInt32(reader, 0);
        entities.Employer.Name = db.GetNullableString(reader, 1);
        entities.Employer.Populated = true;
      });
  }

  private bool ReadEmployerAddress()
  {
    entities.EmployerAddress.Populated = false;

    return Read("ReadEmployerAddress",
      (db, command) =>
      {
        db.SetInt32(command, "empId", entities.Employer.Identifier);
      },
      (db, reader) =>
      {
        entities.EmployerAddress.LocationType = db.GetString(reader, 0);
        entities.EmployerAddress.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 1);
        entities.EmployerAddress.LastUpdatedBy =
          db.GetNullableString(reader, 2);
        entities.EmployerAddress.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.EmployerAddress.CreatedBy = db.GetString(reader, 4);
        entities.EmployerAddress.Street1 = db.GetNullableString(reader, 5);
        entities.EmployerAddress.Street2 = db.GetNullableString(reader, 6);
        entities.EmployerAddress.City = db.GetNullableString(reader, 7);
        entities.EmployerAddress.Identifier = db.GetDateTime(reader, 8);
        entities.EmployerAddress.Street3 = db.GetNullableString(reader, 9);
        entities.EmployerAddress.Street4 = db.GetNullableString(reader, 10);
        entities.EmployerAddress.Province = db.GetNullableString(reader, 11);
        entities.EmployerAddress.Country = db.GetNullableString(reader, 12);
        entities.EmployerAddress.PostalCode = db.GetNullableString(reader, 13);
        entities.EmployerAddress.State = db.GetNullableString(reader, 14);
        entities.EmployerAddress.ZipCode = db.GetNullableString(reader, 15);
        entities.EmployerAddress.Zip4 = db.GetNullableString(reader, 16);
        entities.EmployerAddress.Zip3 = db.GetNullableString(reader, 17);
        entities.EmployerAddress.EmpId = db.GetInt32(reader, 18);
        entities.EmployerAddress.County = db.GetNullableString(reader, 19);
        entities.EmployerAddress.Populated = true;
        CheckValid<EmployerAddress>("LocationType",
          entities.EmployerAddress.LocationType);
      });
  }

  private bool ReadIncomeSource()
  {
    entities.IncomeSource.Populated = false;

    return Read("ReadIncomeSource",
      (db, command) =>
      {
        db.SetDateTime(
          command, "identifier",
          import.IncomeSource.Identifier.GetValueOrDefault());
        db.SetString(command, "cspINumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.Type1 = db.GetString(reader, 1);
        entities.IncomeSource.LastUpdatedBy = db.GetNullableString(reader, 2);
        entities.IncomeSource.CreatedBy = db.GetString(reader, 3);
        entities.IncomeSource.CspINumber = db.GetString(reader, 4);
        entities.IncomeSource.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.IncomeSource.Type1);
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
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
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
    /// A value of NonEmployIncomeSourceAddress.
    /// </summary>
    [JsonPropertyName("nonEmployIncomeSourceAddress")]
    public NonEmployIncomeSourceAddress NonEmployIncomeSourceAddress
    {
      get => nonEmployIncomeSourceAddress ??= new();
      set => nonEmployIncomeSourceAddress = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    private Employer employer;
    private CsePerson csePerson;
    private NonEmployIncomeSourceAddress nonEmployIncomeSourceAddress;
    private IncomeSource incomeSource;
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
    /// A value of LastPosition.
    /// </summary>
    [JsonPropertyName("lastPosition")]
    public Common LastPosition
    {
      get => lastPosition ??= new();
      set => lastPosition = value;
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
    /// A value of SpPrintWorkSet.
    /// </summary>
    [JsonPropertyName("spPrintWorkSet")]
    public SpPrintWorkSet SpPrintWorkSet
    {
      get => spPrintWorkSet ??= new();
      set => spPrintWorkSet = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
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

    private Common lastPosition;
    private FieldValue temp;
    private CsePersonsWorkSet csePersonsWorkSet;
    private FieldValue final;
    private SpPrintWorkSet spPrintWorkSet;
    private DateWorkArea initialized;
    private AbendData readCsePerson;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
    }

    private CsePerson csePerson;
    private IncomeSource incomeSource;
    private Employer employer;
    private EmployerAddress employerAddress;
  }
#endregion
}
