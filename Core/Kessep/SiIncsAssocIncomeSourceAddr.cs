// Program: SI_INCS_ASSOC_INCOME_SOURCE_ADDR, ID: 371763118, model: 746.
// Short name: SWE01098
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_INCS_ASSOC_INCOME_SOURCE_ADDR.
/// </para>
/// <para>
/// RESP: SRVINIT
/// Depending on the Income Source Type, the appropriate Address will be 
/// assoicated with the Income Source.
/// </para>
/// </summary>
[Serializable]
public partial class SiIncsAssocIncomeSourceAddr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_INCS_ASSOC_INCOME_SOURCE_ADDR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiIncsAssocIncomeSourceAddr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiIncsAssocIncomeSourceAddr.
  /// </summary>
  public SiIncsAssocIncomeSourceAddr(IContext context, Import import,
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
    // -------------------------------------------------------------------------------------
    // Date	  Developer	Request#	Description
    // --------  ------------	-------------	
    // ---------------------------------------------
    // ??/??/??  ??????			Initial Development
    // 10/20/98  W. Campbell  			Fixed problems dealing with
    // 					non_empl_income_source_address and type 'O'
    // 					or type 'R' income.
    // 05/17/02  M. Lachowicz  PR146291	Finished this CAB when employer is not 
    // found.
    // 06/15/05  M J Quinn	PR 240878	An update abend was occurring on the
    // 					ASSOCIATE statement for the new employer and
    // 					the income_source.   It seems that currency
    // 					on the income_source table was being lost
    // 					(probably on the DISASSOCIATE statement).
    // 					Re-reading the income_source table fixes the
    // 					problem.
    // 08/02/18  GVandy	CQ61457		Update SVES and 'O' type employer to work
    // 					with eIWO for SSA.
    // -------------------------------------------------------------------------------------
    if (IsEmpty(import.CsePerson.Number))
    {
      import.CsePerson.Number = import.CsePersonsWorkSet.Number;
    }

    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    if (ReadIncomeSource())
    {
      // --08/02/18 GVandy CQ61457 Update SVES and 'O' type employer to work 
      // with eIWO for SSA.
      // (Associate type "O", code "SA" to the SSA employer record).
      if (AsChar(import.IncomeSource.Type1) == 'E' || AsChar
        (import.IncomeSource.Type1) == 'M' || AsChar
        (import.IncomeSource.Type1) == 'O' && Equal
        (entities.IncomeSource.Code, "SA"))
      {
        if (ReadEmployer2())
        {
          DisassociateIncomeSource();
        }

        // 06/15/2005      M J Quinn   PR 240878 Abend:   An update abend was 
        // occurring on the ASSOCIATE statement
        // for the new employer and the income_source.   It seems that
        // currency on the income_source table was                  being lost
        // ( probably on the earlier DISASSOCIATE statement).
        // 
        // Re-reading the income_source table fixes the problem.
        ReadIncomeSource();

        if (ReadEmployer1())
        {
          AssociateIncomeSource();
        }
        else
        {
          // 05/17/02 M.L Start
          ExitState = "EMPLOYER_NF";

          // 05/17/02 M.L End
        }
      }
      else
      {
        if (AsChar(import.IncomeSource.Type1) == 'R')
        {
          if (ReadCsePersonResource())
          {
            AssociateCsePersonResource();
          }
          else
          {
            ExitState = "CSE_PERSON_RESOURCE_NF";

            return;
          }
        }

        if (ReadNonEmployIncomeSourceAddress())
        {
          try
          {
            UpdateNonEmployIncomeSourceAddress();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "NON_EMPL_INC_SOURCE_ADDR_NU";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "NON_EMPL_INC_SOURCE_ADDR_PV";

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
          try
          {
            CreateNonEmployIncomeSourceAddress();
            ExitState = "ACO_NN0000_ALL_OK";
            import.NonEmployIncomeSourceAddress.Assign(
              entities.NonEmployIncomeSourceAddress);
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "NON_EMPL_INC_SOURCE_ADDR_AE";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "NON_EMPL_INC_SOURCE_ADDR_PV";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }
    }
    else
    {
      ExitState = "INCOME_SOURCE_NF";
    }
  }

  private void AssociateCsePersonResource()
  {
    System.Diagnostics.Debug.Assert(entities.IncomeSource.Populated);
    System.Diagnostics.Debug.Assert(entities.NewCsePersonResource.Populated);

    var cprResourceNo = entities.NewCsePersonResource.ResourceNo;
    var cspNumber = entities.NewCsePersonResource.CspNumber;

    entities.IncomeSource.Populated = false;
    Update("AssociateCsePersonResource",
      (db, command) =>
      {
        db.SetNullableInt32(command, "cprResourceNo", cprResourceNo);
        db.SetNullableString(command, "cspNumber", cspNumber);
        db.SetDateTime(
          command, "identifier",
          entities.IncomeSource.Identifier.GetValueOrDefault());
        db.SetString(command, "cspINumber", entities.IncomeSource.CspINumber);
      });

    entities.IncomeSource.CprResourceNo = cprResourceNo;
    entities.IncomeSource.CspNumber = cspNumber;
    entities.IncomeSource.Populated = true;
  }

  private void AssociateIncomeSource()
  {
    System.Diagnostics.Debug.Assert(entities.IncomeSource.Populated);

    var empId = entities.NewEmployer.Identifier;

    entities.IncomeSource.Populated = false;
    Update("AssociateIncomeSource",
      (db, command) =>
      {
        db.SetNullableInt32(command, "empId", empId);
        db.SetDateTime(
          command, "identifier",
          entities.IncomeSource.Identifier.GetValueOrDefault());
        db.SetString(command, "cspINumber", entities.IncomeSource.CspINumber);
      });

    entities.IncomeSource.EmpId = empId;
    entities.IncomeSource.Populated = true;
  }

  private void CreateNonEmployIncomeSourceAddress()
  {
    System.Diagnostics.Debug.Assert(entities.IncomeSource.Populated);

    var isrIdentifier = entities.IncomeSource.Identifier;
    var street1 = import.NonEmployIncomeSourceAddress.Street1 ?? "";
    var street2 = import.NonEmployIncomeSourceAddress.Street2 ?? "";
    var city = import.NonEmployIncomeSourceAddress.City ?? "";
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var state = import.NonEmployIncomeSourceAddress.State ?? "";
    var zipCode = import.NonEmployIncomeSourceAddress.ZipCode ?? "";
    var zip4 = import.NonEmployIncomeSourceAddress.Zip4 ?? "";
    var locationType = import.NonEmployIncomeSourceAddress.LocationType;
    var csePerson1 = entities.IncomeSource.CspINumber;

    CheckValid<NonEmployIncomeSourceAddress>("LocationType", locationType);
    entities.NonEmployIncomeSourceAddress.Populated = false;
    Update("CreateNonEmployIncomeSourceAddress",
      (db, command) =>
      {
        db.SetDateTime(command, "isrIdentifier", isrIdentifier);
        db.SetNullableString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetNullableString(command, "city", city);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "zipCode", zipCode);
        db.SetNullableString(command, "zip4", zip4);
        db.SetNullableString(command, "zip3", "");
        db.SetNullableString(command, "street3", "");
        db.SetNullableString(command, "province", "");
        db.SetNullableString(command, "postalCode", "");
        db.SetNullableString(command, "country", "");
        db.SetString(command, "locationType", locationType);
        db.SetString(command, "csePerson", csePerson1);
      });

    entities.NonEmployIncomeSourceAddress.IsrIdentifier = isrIdentifier;
    entities.NonEmployIncomeSourceAddress.Street1 = street1;
    entities.NonEmployIncomeSourceAddress.Street2 = street2;
    entities.NonEmployIncomeSourceAddress.City = city;
    entities.NonEmployIncomeSourceAddress.LastUpdatedBy = "";
    entities.NonEmployIncomeSourceAddress.LastUpdatedTimestamp = null;
    entities.NonEmployIncomeSourceAddress.CreatedBy = createdBy;
    entities.NonEmployIncomeSourceAddress.CreatedTimestamp = createdTimestamp;
    entities.NonEmployIncomeSourceAddress.State = state;
    entities.NonEmployIncomeSourceAddress.ZipCode = zipCode;
    entities.NonEmployIncomeSourceAddress.Zip4 = zip4;
    entities.NonEmployIncomeSourceAddress.LocationType = locationType;
    entities.NonEmployIncomeSourceAddress.CsePerson = csePerson1;
    entities.NonEmployIncomeSourceAddress.Populated = true;
  }

  private void DisassociateIncomeSource()
  {
    System.Diagnostics.Debug.Assert(entities.IncomeSource.Populated);
    entities.IncomeSource.Populated = false;
    Update("DisassociateIncomeSource",
      (db, command) =>
      {
        db.SetDateTime(
          command, "identifier",
          entities.IncomeSource.Identifier.GetValueOrDefault());
        db.SetString(command, "cspINumber", entities.IncomeSource.CspINumber);
      });

    entities.IncomeSource.EmpId = null;
    entities.IncomeSource.Populated = true;
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

  private bool ReadCsePersonResource()
  {
    entities.NewCsePersonResource.Populated = false;

    return Read("ReadCsePersonResource",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetInt32(command, "resourceNo", import.CsePersonResource.ResourceNo);
      },
      (db, reader) =>
      {
        entities.NewCsePersonResource.CspNumber = db.GetString(reader, 0);
        entities.NewCsePersonResource.ResourceNo = db.GetInt32(reader, 1);
        entities.NewCsePersonResource.Populated = true;
      });
  }

  private bool ReadEmployer1()
  {
    entities.NewEmployer.Populated = false;

    return Read("ReadEmployer1",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.Employer.Identifier);
      },
      (db, reader) =>
      {
        entities.NewEmployer.Identifier = db.GetInt32(reader, 0);
        entities.NewEmployer.Populated = true;
      });
  }

  private bool ReadEmployer2()
  {
    System.Diagnostics.Debug.Assert(entities.IncomeSource.Populated);
    entities.Old.Populated = false;

    return Read("ReadEmployer2",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.IncomeSource.EmpId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Old.Identifier = db.GetInt32(reader, 0);
        entities.Old.Populated = true;
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
        entities.IncomeSource.Code = db.GetNullableString(reader, 2);
        entities.IncomeSource.CspINumber = db.GetString(reader, 3);
        entities.IncomeSource.CprResourceNo = db.GetNullableInt32(reader, 4);
        entities.IncomeSource.CspNumber = db.GetNullableString(reader, 5);
        entities.IncomeSource.EmpId = db.GetNullableInt32(reader, 6);
        entities.IncomeSource.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.IncomeSource.Type1);
      });
  }

  private bool ReadNonEmployIncomeSourceAddress()
  {
    System.Diagnostics.Debug.Assert(entities.IncomeSource.Populated);
    entities.NonEmployIncomeSourceAddress.Populated = false;

    return Read("ReadNonEmployIncomeSourceAddress",
      (db, command) =>
      {
        db.SetString(command, "csePerson", entities.IncomeSource.CspINumber);
        db.SetDateTime(
          command, "isrIdentifier",
          entities.IncomeSource.Identifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.NonEmployIncomeSourceAddress.IsrIdentifier =
          db.GetDateTime(reader, 0);
        entities.NonEmployIncomeSourceAddress.Street1 =
          db.GetNullableString(reader, 1);
        entities.NonEmployIncomeSourceAddress.Street2 =
          db.GetNullableString(reader, 2);
        entities.NonEmployIncomeSourceAddress.City =
          db.GetNullableString(reader, 3);
        entities.NonEmployIncomeSourceAddress.LastUpdatedBy =
          db.GetNullableString(reader, 4);
        entities.NonEmployIncomeSourceAddress.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 5);
        entities.NonEmployIncomeSourceAddress.CreatedBy =
          db.GetString(reader, 6);
        entities.NonEmployIncomeSourceAddress.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.NonEmployIncomeSourceAddress.State =
          db.GetNullableString(reader, 8);
        entities.NonEmployIncomeSourceAddress.ZipCode =
          db.GetNullableString(reader, 9);
        entities.NonEmployIncomeSourceAddress.Zip4 =
          db.GetNullableString(reader, 10);
        entities.NonEmployIncomeSourceAddress.LocationType =
          db.GetString(reader, 11);
        entities.NonEmployIncomeSourceAddress.CsePerson =
          db.GetString(reader, 12);
        entities.NonEmployIncomeSourceAddress.Populated = true;
        CheckValid<NonEmployIncomeSourceAddress>("LocationType",
          entities.NonEmployIncomeSourceAddress.LocationType);
      });
  }

  private void UpdateNonEmployIncomeSourceAddress()
  {
    System.Diagnostics.Debug.Assert(
      entities.NonEmployIncomeSourceAddress.Populated);

    var street1 = import.NonEmployIncomeSourceAddress.Street1 ?? "";
    var street2 = import.NonEmployIncomeSourceAddress.Street2 ?? "";
    var city = import.NonEmployIncomeSourceAddress.City ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var state = import.NonEmployIncomeSourceAddress.State ?? "";
    var zipCode = import.NonEmployIncomeSourceAddress.ZipCode ?? "";
    var zip4 = import.NonEmployIncomeSourceAddress.Zip4 ?? "";
    var locationType = import.NonEmployIncomeSourceAddress.LocationType;

    CheckValid<NonEmployIncomeSourceAddress>("LocationType", locationType);
    entities.NonEmployIncomeSourceAddress.Populated = false;
    Update("UpdateNonEmployIncomeSourceAddress",
      (db, command) =>
      {
        db.SetNullableString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetNullableString(command, "city", city);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "zipCode", zipCode);
        db.SetNullableString(command, "zip4", zip4);
        db.SetString(command, "locationType", locationType);
        db.SetDateTime(
          command, "isrIdentifier",
          entities.NonEmployIncomeSourceAddress.IsrIdentifier.
            GetValueOrDefault());
        db.SetString(
          command, "csePerson",
          entities.NonEmployIncomeSourceAddress.CsePerson);
      });

    entities.NonEmployIncomeSourceAddress.Street1 = street1;
    entities.NonEmployIncomeSourceAddress.Street2 = street2;
    entities.NonEmployIncomeSourceAddress.City = city;
    entities.NonEmployIncomeSourceAddress.LastUpdatedBy = lastUpdatedBy;
    entities.NonEmployIncomeSourceAddress.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.NonEmployIncomeSourceAddress.State = state;
    entities.NonEmployIncomeSourceAddress.ZipCode = zipCode;
    entities.NonEmployIncomeSourceAddress.Zip4 = zip4;
    entities.NonEmployIncomeSourceAddress.LocationType = locationType;
    entities.NonEmployIncomeSourceAddress.Populated = true;
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
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
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
    /// A value of CsePersonResource.
    /// </summary>
    [JsonPropertyName("csePersonResource")]
    public CsePersonResource CsePersonResource
    {
      get => csePersonResource ??= new();
      set => csePersonResource = value;
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

    private Employer employer;
    private IncomeSource incomeSource;
    private CsePersonResource csePersonResource;
    private NonEmployIncomeSourceAddress nonEmployIncomeSourceAddress;
    private CsePerson csePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
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
    /// A value of Old.
    /// </summary>
    [JsonPropertyName("old")]
    public Employer Old
    {
      get => old ??= new();
      set => old = value;
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
    /// A value of NewEmployer.
    /// </summary>
    [JsonPropertyName("newEmployer")]
    public Employer NewEmployer
    {
      get => newEmployer ??= new();
      set => newEmployer = value;
    }

    /// <summary>
    /// A value of NewCsePersonResource.
    /// </summary>
    [JsonPropertyName("newCsePersonResource")]
    public CsePersonResource NewCsePersonResource
    {
      get => newCsePersonResource ??= new();
      set => newCsePersonResource = value;
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

    private Employer old;
    private IncomeSource incomeSource;
    private Employer newEmployer;
    private CsePersonResource newCsePersonResource;
    private CsePerson csePerson;
    private NonEmployIncomeSourceAddress nonEmployIncomeSourceAddress;
  }
#endregion
}
