// Program: SI_CREATE_PERSON_FOREIGN_ADDRESS, ID: 371801109, model: 746.
// Short name: SWE01146
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_CREATE_PERSON_FOREIGN_ADDRESS.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This PAD creates an address for a given CSE Person
/// </para>
/// </summary>
[Serializable]
public partial class SiCreatePersonForeignAddress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CREATE_PERSON_FOREIGN_ADDRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCreatePersonForeignAddress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCreatePersonForeignAddress.
  /// </summary>
  public SiCreatePersonForeignAddress(IContext context, Import import,
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
    // -----------------------------------------------------------
    //                 M A I N T E N A N C E   L O G
    //   Date      Developer       Request #       Description
    // 02/26/95  Helen Sharland                   Initial Development
    // ----------------------------------------------------------
    // 12/10/98  W.Campbell        Removed the SET stmts
    //                             from the CREATE statement
    //                             for setting attributes:
    //                             VERIFIED_CODE and
    //                             START_DATE for
    //                             CSE_PERSON_ADDRESS.
    //                             Work done on IDCR 454.
    // ---------------------------------------------
    // 12/18/98 W. Campbell       Inserted set statements
    //                            to set END_DATE in the export view.
    //                            Also, replaced use of the
    //                            CURRENT_TIMESTAMP function with
    //                            a local view set to same.
    //                            Work done on IDCR 454.
    // ---------------------------------------------
    // 07/13/99  Marek Lachowicz   Change property of READ (Select Only)
    UseOeCabSetMnemonics();
    local.Current.Timestamp = Now();
    export.CsePersonAddress.Assign(import.CsePersonAddress);
    export.CsePersonAddress.LocationType = "F";

    if (Equal(import.CsePersonAddress.EndDate, null))
    {
      local.CsePersonAddress.EndDate = local.MaxDate.ExpirationDate;
    }
    else
    {
      local.CsePersonAddress.EndDate = import.CsePersonAddress.EndDate;
    }

    // ---------------------------------------------
    // This PAD creates an address for a CSE Person.
    // ---------------------------------------------
    // 07/13/99  M.L 	Change property of READ (Select Only)
    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    // ---------------------------------------------
    // 12/10/98 W.Campbell - Removed the SET stmts
    // from the CREATE statement for setting attributes:
    // VERIFIED_CODE and START_DATE for
    // CSE_PERSON_ADDRESS.  Work done on IDCR 454.
    // ---------------------------------------------
    try
    {
      CreateCsePersonAddress();
      export.CsePersonAddress.Assign(entities.CsePersonAddress);

      // ---------------------------------------------
      // 12/18/98 W. Campbell - Inserted set statements
      // to set END_DATE in the export view.
      // Work done on IDCR 454.
      // ---------------------------------------------
      local.EndDate.Date = export.CsePersonAddress.EndDate;
      export.CsePersonAddress.EndDate = UseCabSetMaximumDiscontinueDate();
      ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "CSE_PERSON_ADDRESS_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "CSE_PERSON_ADDRESS_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.EndDate.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseOeCabSetMnemonics()
  {
    var useImport = new OeCabSetMnemonics.Import();
    var useExport = new OeCabSetMnemonics.Export();

    Call(OeCabSetMnemonics.Execute, useImport, useExport);

    local.MaxDate.ExpirationDate = useExport.MaxDate.ExpirationDate;
  }

  private void CreateCsePersonAddress()
  {
    var identifier = local.Current.Timestamp;
    var cspNumber = entities.CsePerson.Number;
    var sendDate = export.CsePersonAddress.SendDate;
    var source = export.CsePersonAddress.Source ?? "";
    var street1 = export.CsePersonAddress.Street1 ?? "";
    var street2 = export.CsePersonAddress.Street2 ?? "";
    var city = export.CsePersonAddress.City ?? "";
    var type1 = export.CsePersonAddress.Type1 ?? "";
    var workerId = global.UserId;
    var verifiedDate = export.CsePersonAddress.VerifiedDate;
    var endDate = local.CsePersonAddress.EndDate;
    var endCode = export.CsePersonAddress.EndCode ?? "";
    var street3 = export.CsePersonAddress.Street3 ?? "";
    var street4 = export.CsePersonAddress.Street4 ?? "";
    var province = export.CsePersonAddress.Province ?? "";
    var postalCode = export.CsePersonAddress.PostalCode ?? "";
    var country = export.CsePersonAddress.Country ?? "";
    var locationType = export.CsePersonAddress.LocationType;

    CheckValid<CsePersonAddress>("LocationType", locationType);
    entities.CsePersonAddress.Populated = false;
    Update("CreateCsePersonAddress",
      (db, command) =>
      {
        db.SetDateTime(command, "identifier", identifier);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetNullableDate(command, "zdelStartDate", null);
        db.SetNullableDate(command, "sendDate", sendDate);
        db.SetNullableString(command, "source", source);
        db.SetNullableString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetNullableString(command, "city", city);
        db.SetNullableString(command, "type", type1);
        db.SetNullableString(command, "workerId", workerId);
        db.SetNullableDate(command, "verifiedDate", verifiedDate);
        db.SetNullableDate(command, "endDate", endDate);
        db.SetNullableString(command, "endCode", endCode);
        db.SetNullableString(command, "zdelVerifiedCode", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", identifier);
        db.SetNullableString(command, "lastUpdatedBy", workerId);
        db.SetNullableDateTime(command, "createdTimestamp", identifier);
        db.SetNullableString(command, "createdBy", workerId);
        db.SetNullableString(command, "state", "");
        db.SetNullableString(command, "zipCode", "");
        db.SetNullableString(command, "zip4", "");
        db.SetNullableString(command, "zip3", "");
        db.SetNullableString(command, "street3", street3);
        db.SetNullableString(command, "street4", street4);
        db.SetNullableString(command, "province", province);
        db.SetNullableString(command, "postalCode", postalCode);
        db.SetNullableString(command, "country", country);
        db.SetString(command, "locationType", locationType);
      });

    entities.CsePersonAddress.Identifier = identifier;
    entities.CsePersonAddress.CspNumber = cspNumber;
    entities.CsePersonAddress.ZdelStartDate = null;
    entities.CsePersonAddress.SendDate = sendDate;
    entities.CsePersonAddress.Source = source;
    entities.CsePersonAddress.Street1 = street1;
    entities.CsePersonAddress.Street2 = street2;
    entities.CsePersonAddress.City = city;
    entities.CsePersonAddress.Type1 = type1;
    entities.CsePersonAddress.WorkerId = workerId;
    entities.CsePersonAddress.VerifiedDate = verifiedDate;
    entities.CsePersonAddress.EndDate = endDate;
    entities.CsePersonAddress.EndCode = endCode;
    entities.CsePersonAddress.ZdelVerifiedCode = "";
    entities.CsePersonAddress.LastUpdatedTimestamp = identifier;
    entities.CsePersonAddress.LastUpdatedBy = workerId;
    entities.CsePersonAddress.CreatedTimestamp = identifier;
    entities.CsePersonAddress.CreatedBy = workerId;
    entities.CsePersonAddress.Street3 = street3;
    entities.CsePersonAddress.Street4 = street4;
    entities.CsePersonAddress.Province = province;
    entities.CsePersonAddress.PostalCode = postalCode;
    entities.CsePersonAddress.Country = country;
    entities.CsePersonAddress.LocationType = locationType;
    entities.CsePersonAddress.Populated = true;
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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

    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePersonAddress csePersonAddress;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    private CsePersonAddress csePersonAddress;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of EndDate.
    /// </summary>
    [JsonPropertyName("endDate")]
    public DateWorkArea EndDate
    {
      get => endDate ??= new();
      set => endDate = value;
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
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public Code MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
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

    private DateWorkArea endDate;
    private DateWorkArea current;
    private Code maxDate;
    private CsePersonAddress csePersonAddress;
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
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    private CsePerson csePerson;
    private CsePersonAddress csePersonAddress;
  }
#endregion
}
