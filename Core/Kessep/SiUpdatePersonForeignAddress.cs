// Program: SI_UPDATE_PERSON_FOREIGN_ADDRESS, ID: 371801284, model: 746.
// Short name: SWE01255
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_UPDATE_PERSON_FOREIGN_ADDRESS.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This PAD creates an foreign address for a given CSE Person
/// </para>
/// </summary>
[Serializable]
public partial class SiUpdatePersonForeignAddress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_UPDATE_PERSON_FOREIGN_ADDRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiUpdatePersonForeignAddress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiUpdatePersonForeignAddress.
  /// </summary>
  public SiUpdatePersonForeignAddress(IContext context, Import import,
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
    // 09/21/95  Sid                    Initial Development
    // ----------------------------------------------------------
    // 12/10/98  W.Campbell        Removed the SET stmts
    //                             from the UPDATE statement
    //                             for setting attributes:
    //                             VERIFIED_CODE and
    //                             START_DATE for
    //                             CSE_PERSON_ADDRESS.
    //                             Work done on IDCR 454.
    // ---------------------------------------------
    // 12/15/98  W.Campbell        Added an export view for
    //                             CSE_PERSON_ADDRESS and
    //                             logic to populate it on
    //                             a successful UPDATE.
    //                             Work done on IDCR454.
    // ---------------------------------------------
    // 12/18/98  W.Campbell        Modified code so that
    //                             the CURRENT_TIMESTAMP
    //                             function is only used once.
    //                             Work done on IDCR454.
    // ---------------------------------------------
    // ---------------------------------------------
    // This PAD updates an address for a CSE Person.
    // ---------------------------------------------
    UseOeCabSetMnemonics();

    // ---------------------------------------------
    // 12/18/98  W.Campbell - Modified code so that
    // the CURRENT_TIMESTAMP function is only
    // used once. Work done on IDCR454.
    // ---------------------------------------------
    local.Current.Timestamp = Now();

    if (Equal(import.CsePersonAddress.EndDate, null))
    {
      local.CsePersonAddress.EndDate = local.MaxDate.ExpirationDate;
    }
    else
    {
      local.CsePersonAddress.EndDate = import.CsePersonAddress.EndDate;
    }

    if (ReadCsePersonAddress())
    {
      // ---------------------------------------------
      // 12/10/98  W.Campbell - Removed the SET stmts
      // from the UPDATE statement for setting attributes:
      // VERIFIED_CODE and START_DATE for
      // CSE_PERSON_ADDRESS. Work done on IDCR 454.
      // ---------------------------------------------
      try
      {
        UpdateCsePersonAddress();

        // ---------------------------------------------
        // 12/15/98  W.Campbell - Added an export view for
        // CSE_PERSON_ADDRESS and logic to populate
        // it on a successful UPDATE.  Work done on IDCR454.
        // ---------------------------------------------
        export.CsePersonAddress.Assign(entities.CsePersonAddress);
        local.DateWorkArea.Date = entities.CsePersonAddress.EndDate;
        export.CsePersonAddress.EndDate = UseCabSetMaximumDiscontinueDate();
        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CSE_PERSON_ADDRESS_NU";

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
    else
    {
      ExitState = "CSE_PERSON_ADDRESS_NF";
    }
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

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

  private bool ReadCsePersonAddress()
  {
    entities.CsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress",
      (db, command) =>
      {
        db.SetDateTime(
          command, "identifier",
          import.CsePersonAddress.Identifier.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.ZdelStartDate = db.GetNullableDate(reader, 2);
        entities.CsePersonAddress.SendDate = db.GetNullableDate(reader, 3);
        entities.CsePersonAddress.Source = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.Street1 = db.GetNullableString(reader, 5);
        entities.CsePersonAddress.Street2 = db.GetNullableString(reader, 6);
        entities.CsePersonAddress.City = db.GetNullableString(reader, 7);
        entities.CsePersonAddress.Type1 = db.GetNullableString(reader, 8);
        entities.CsePersonAddress.WorkerId = db.GetNullableString(reader, 9);
        entities.CsePersonAddress.VerifiedDate = db.GetNullableDate(reader, 10);
        entities.CsePersonAddress.EndDate = db.GetNullableDate(reader, 11);
        entities.CsePersonAddress.EndCode = db.GetNullableString(reader, 12);
        entities.CsePersonAddress.ZdelVerifiedCode =
          db.GetNullableString(reader, 13);
        entities.CsePersonAddress.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 14);
        entities.CsePersonAddress.LastUpdatedBy =
          db.GetNullableString(reader, 15);
        entities.CsePersonAddress.Street3 = db.GetNullableString(reader, 16);
        entities.CsePersonAddress.Street4 = db.GetNullableString(reader, 17);
        entities.CsePersonAddress.Province = db.GetNullableString(reader, 18);
        entities.CsePersonAddress.PostalCode = db.GetNullableString(reader, 19);
        entities.CsePersonAddress.Country = db.GetNullableString(reader, 20);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 21);
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);
      });
  }

  private void UpdateCsePersonAddress()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAddress.Populated);

    var sendDate = import.CsePersonAddress.SendDate;
    var source = import.CsePersonAddress.Source ?? "";
    var street1 = import.CsePersonAddress.Street1 ?? "";
    var street2 = import.CsePersonAddress.Street2 ?? "";
    var city = import.CsePersonAddress.City ?? "";
    var type1 = import.CsePersonAddress.Type1 ?? "";
    var workerId = global.UserId;
    var verifiedDate = import.CsePersonAddress.VerifiedDate;
    var endDate = local.CsePersonAddress.EndDate;
    var endCode = import.CsePersonAddress.EndCode ?? "";
    var lastUpdatedTimestamp = local.Current.Timestamp;
    var street3 = import.CsePersonAddress.Street3 ?? "";
    var street4 = import.CsePersonAddress.Street4 ?? "";
    var province = import.CsePersonAddress.Province ?? "";
    var postalCode = import.CsePersonAddress.PostalCode ?? "";
    var country = import.CsePersonAddress.Country ?? "";
    var locationType = import.CsePersonAddress.LocationType;

    CheckValid<CsePersonAddress>("LocationType", locationType);
    entities.CsePersonAddress.Populated = false;
    Update("UpdateCsePersonAddress",
      (db, command) =>
      {
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
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", workerId);
        db.SetNullableString(command, "street3", street3);
        db.SetNullableString(command, "street4", street4);
        db.SetNullableString(command, "province", province);
        db.SetNullableString(command, "postalCode", postalCode);
        db.SetNullableString(command, "country", country);
        db.SetString(command, "locationType", locationType);
        db.SetDateTime(
          command, "identifier",
          entities.CsePersonAddress.Identifier.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.CsePersonAddress.CspNumber);
      });

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
    entities.CsePersonAddress.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CsePersonAddress.LastUpdatedBy = workerId;
    entities.CsePersonAddress.Street3 = street3;
    entities.CsePersonAddress.Street4 = street4;
    entities.CsePersonAddress.Province = province;
    entities.CsePersonAddress.PostalCode = postalCode;
    entities.CsePersonAddress.Country = country;
    entities.CsePersonAddress.LocationType = locationType;
    entities.CsePersonAddress.Populated = true;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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

    private DateWorkArea current;
    private DateWorkArea dateWorkArea;
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
