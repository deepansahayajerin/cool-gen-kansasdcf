// Program: SI_UPDATE_INCOME_SOURCE_CONTACT, ID: 371763122, model: 746.
// Short name: SWE01252
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_UPDATE_INCOME_SOURCE_CONTACT.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This PAD updates the details of a contact of an income source of a given CSE
/// Person
/// </para>
/// </summary>
[Serializable]
public partial class SiUpdateIncomeSourceContact: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_UPDATE_INCOME_SOURCE_CONTACT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiUpdateIncomeSourceContact(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiUpdateIncomeSourceContact.
  /// </summary>
  public SiUpdateIncomeSourceContact(IContext context, Import import,
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
    // This PAD updates an income source contact for
    // a CSE Person.
    // ---------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    if (IsEmpty(import.CsePerson.Number))
    {
      import.CsePerson.Number = import.CsePersonsWorkSet.Number;
    }

    export.IncomeSourceContact.Assign(import.IncomeSourceContact);

    // CQ31228 - Add separate READ for Income Source then READ Income Source 
    // Contact.
    if (ReadIncomeSource())
    {
      if (ReadIncomeSourceContact())
      {
        local.IncomeSourceContact.EmailAddress =
          import.IncomeSourceContact.EmailAddress ?? "";

        try
        {
          UpdateIncomeSourceContact();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "INCOME_SOURCE_CONTACT_NU";

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
        UseSiIncsCreateIncomeSrcContct();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
      }
    }
    else
    {
      ExitState = "INCOME_SOURCE_CONTACT_NF";
    }
  }

  private void UseSiIncsCreateIncomeSrcContct()
  {
    var useImport = new SiIncsCreateIncomeSrcContct.Import();
    var useExport = new SiIncsCreateIncomeSrcContct.Export();

    useImport.IncomeSource.Identifier = import.IncomeSource.Identifier;
    useImport.CsePerson.Number = import.CsePerson.Number;
    useImport.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;
    useImport.IncomeSourceContact.Assign(export.IncomeSourceContact);

    Call(SiIncsCreateIncomeSrcContct.Execute, useImport, useExport);

    import.CsePerson.Number = useImport.CsePerson.Number;
    export.IncomeSourceContact.Assign(useImport.IncomeSourceContact);
  }

  private bool ReadIncomeSource()
  {
    entities.IncomeSource.Populated = false;

    return Read("ReadIncomeSource",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", import.CsePerson.Number);
        db.SetDateTime(
          command, "identifier",
          import.IncomeSource.Identifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.CspINumber = db.GetString(reader, 1);
        entities.IncomeSource.Populated = true;
      });
  }

  private bool ReadIncomeSourceContact()
  {
    System.Diagnostics.Debug.Assert(entities.IncomeSource.Populated);
    entities.IncomeSourceContact.Populated = false;

    return Read("ReadIncomeSourceContact",
      (db, command) =>
      {
        db.SetString(command, "csePerson", entities.IncomeSource.CspINumber);
        db.SetDateTime(
          command, "isrIdentifier",
          entities.IncomeSource.Identifier.GetValueOrDefault());
        db.
          SetInt32(command, "identifier", import.IncomeSourceContact.Identifier);
          
        db.SetString(command, "type", import.IncomeSourceContact.Type1);
      },
      (db, reader) =>
      {
        entities.IncomeSourceContact.IsrIdentifier = db.GetDateTime(reader, 0);
        entities.IncomeSourceContact.Identifier = db.GetInt32(reader, 1);
        entities.IncomeSourceContact.Type1 = db.GetString(reader, 2);
        entities.IncomeSourceContact.Name = db.GetNullableString(reader, 3);
        entities.IncomeSourceContact.ExtensionNo =
          db.GetNullableString(reader, 4);
        entities.IncomeSourceContact.Number = db.GetNullableInt32(reader, 5);
        entities.IncomeSourceContact.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.IncomeSourceContact.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.IncomeSourceContact.AreaCode = db.GetNullableInt32(reader, 8);
        entities.IncomeSourceContact.CsePerson = db.GetString(reader, 9);
        entities.IncomeSourceContact.EmailAddress =
          db.GetNullableString(reader, 10);
        entities.IncomeSourceContact.Populated = true;
      });
  }

  private void UpdateIncomeSourceContact()
  {
    System.Diagnostics.Debug.Assert(entities.IncomeSourceContact.Populated);

    var name = import.IncomeSourceContact.Name ?? "";
    var extensionNo = import.IncomeSourceContact.ExtensionNo ?? "";
    var number = import.IncomeSourceContact.Number.GetValueOrDefault();
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var areaCode = import.IncomeSourceContact.AreaCode.GetValueOrDefault();
    var emailAddress = local.IncomeSourceContact.EmailAddress ?? "";

    entities.IncomeSourceContact.Populated = false;
    Update("UpdateIncomeSourceContact",
      (db, command) =>
      {
        db.SetNullableString(command, "name", name);
        db.SetNullableString(command, "extensionNo", extensionNo);
        db.SetNullableInt32(command, "numb", number);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableInt32(command, "areaCode", areaCode);
        db.SetNullableString(command, "emailAddress", emailAddress);
        db.SetDateTime(
          command, "isrIdentifier",
          entities.IncomeSourceContact.IsrIdentifier.GetValueOrDefault());
        db.SetInt32(
          command, "identifier", entities.IncomeSourceContact.Identifier);
        db.SetString(command, "type", entities.IncomeSourceContact.Type1);
        db.SetString(
          command, "csePerson", entities.IncomeSourceContact.CsePerson);
      });

    entities.IncomeSourceContact.Name = name;
    entities.IncomeSourceContact.ExtensionNo = extensionNo;
    entities.IncomeSourceContact.Number = number;
    entities.IncomeSourceContact.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.IncomeSourceContact.LastUpdatedBy = lastUpdatedBy;
    entities.IncomeSourceContact.AreaCode = areaCode;
    entities.IncomeSourceContact.EmailAddress = emailAddress;
    entities.IncomeSourceContact.Populated = true;
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

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private IncomeSourceContact incomeSourceContact;
    private IncomeSource incomeSource;
    private CsePerson csePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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

    private IncomeSourceContact incomeSourceContact;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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

    private IncomeSourceContact incomeSourceContact;
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
