// Program: SI_INCS_CREATE_INCOME_SRC_CONTCT, ID: 371763117, model: 746.
// Short name: SWE01134
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_INCS_CREATE_INCOME_SRC_CONTCT.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This PAD creates an income source contact of a given CSE Person
/// </para>
/// </summary>
[Serializable]
public partial class SiIncsCreateIncomeSrcContct: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_INCS_CREATE_INCOME_SRC_CONTCT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiIncsCreateIncomeSrcContct(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiIncsCreateIncomeSrcContct.
  /// </summary>
  public SiIncsCreateIncomeSrcContct(IContext context, Import import,
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
    // ---------------------------------------------
    // This PAD creates an income source contact for
    // a CSE Person.
    // ---------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    if (IsEmpty(import.CsePerson.Number))
    {
      import.CsePerson.Number = import.CsePersonsWorkSet.Number;
    }

    if (ReadIncomeSource())
    {
      // ---------------------------------------------
      // Call the action block that generates the next unique identifier for an 
      // income source contact for a given income source of a given CSE Person.
      // ---------------------------------------------
      UseSiGenIncomeSrceContactId();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      try
      {
        CreateIncomeSourceContact();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "INCOME_SOURCE_CONTACT_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "INCOME_SOURCE_CONTACT_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      ExitState = "INCOME_SOURCE_NF";
    }
  }

  private void UseSiGenIncomeSrceContactId()
  {
    var useImport = new SiGenIncomeSrceContactId.Import();
    var useExport = new SiGenIncomeSrceContactId.Export();

    Call(SiGenIncomeSrceContactId.Execute, useImport, useExport);

    import.IncomeSourceContact.Identifier =
      useExport.IncomeSourceContact.Identifier;
  }

  private void CreateIncomeSourceContact()
  {
    System.Diagnostics.Debug.Assert(entities.IncomeSource.Populated);

    var isrIdentifier = entities.IncomeSource.Identifier;
    var identifier = import.IncomeSourceContact.Identifier;
    var type1 = import.IncomeSourceContact.Type1;
    var name = import.IncomeSourceContact.Name ?? "";
    var extensionNo = import.IncomeSourceContact.ExtensionNo ?? "";
    var number = import.IncomeSourceContact.Number.GetValueOrDefault();
    var createdTimestamp = Now();
    var createdBy = global.UserId;
    var areaCode = import.IncomeSourceContact.AreaCode.GetValueOrDefault();
    var csePerson1 = entities.IncomeSource.CspINumber;
    var emailAddress = import.IncomeSourceContact.EmailAddress ?? "";

    entities.IncomeSourceContact.Populated = false;
    Update("CreateIncomeSourceContact",
      (db, command) =>
      {
        db.SetDateTime(command, "isrIdentifier", isrIdentifier);
        db.SetInt32(command, "identifier", identifier);
        db.SetString(command, "type", type1);
        db.SetNullableString(command, "name", name);
        db.SetNullableString(command, "extensionNo", extensionNo);
        db.SetNullableInt32(command, "numb", number);
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableInt32(command, "areaCode", areaCode);
        db.SetString(command, "csePerson", csePerson1);
        db.SetNullableString(command, "emailAddress", emailAddress);
      });

    entities.IncomeSourceContact.IsrIdentifier = isrIdentifier;
    entities.IncomeSourceContact.Identifier = identifier;
    entities.IncomeSourceContact.Type1 = type1;
    entities.IncomeSourceContact.Name = name;
    entities.IncomeSourceContact.ExtensionNo = extensionNo;
    entities.IncomeSourceContact.Number = number;
    entities.IncomeSourceContact.CreatedTimestamp = createdTimestamp;
    entities.IncomeSourceContact.CreatedBy = createdBy;
    entities.IncomeSourceContact.AreaCode = areaCode;
    entities.IncomeSourceContact.CsePerson = csePerson1;
    entities.IncomeSourceContact.EmailAddress = emailAddress;
    entities.IncomeSourceContact.Populated = true;
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
        db.SetString(command, "cspINumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.CspINumber = db.GetString(reader, 1);
        entities.IncomeSource.Populated = true;
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
    /// A value of IncomeSourceContact.
    /// </summary>
    [JsonPropertyName("incomeSourceContact")]
    public IncomeSourceContact IncomeSourceContact
    {
      get => incomeSourceContact ??= new();
      set => incomeSourceContact = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private IncomeSourceContact incomeSourceContact;
    private IncomeSource incomeSource;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of IncomeSourceContact.
    /// </summary>
    [JsonPropertyName("incomeSourceContact")]
    public IncomeSourceContact IncomeSourceContact
    {
      get => incomeSourceContact ??= new();
      set => incomeSourceContact = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private IncomeSourceContact incomeSourceContact;
    private IncomeSource incomeSource;
    private CsePerson csePerson;
  }
#endregion
}
