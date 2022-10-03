// Program: FN_GET_ACTIVE_CSE_PERSON_ADDRESS, ID: 371752458, model: 746.
// Short name: SWE02149
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_GET_ACTIVE_CSE_PERSON_ADDRESS.
/// </para>
/// <para>
/// This CAB gets the Active Address of a person. If the person has a most 
/// recent &quot;REC&quot; type address, it should be the one. Else, check for
/// the most recent cse_person_address type in the following order - M,R,S,P,F
/// </para>
/// </summary>
[Serializable]
public partial class FnGetActiveCsePersonAddress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_GET_ACTIVE_CSE_PERSON_ADDRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnGetActiveCsePersonAddress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnGetActiveCsePersonAddress.
  /// </summary>
  public FnGetActiveCsePersonAddress(IContext context, Import import,
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
    // =================================================
    // 1/6/99 - b adams  -  replaced all references to CSE_Person_
    //   Address zdelStart_Dt to Verified_Dt; eliminated all references
    //   to Verified_Code, replacing it to nothing.  Verified_Dt now
    //   represents two things.
    // =================================================
    if (Equal(import.AsOfDate.Date, local.Initialized.Date))
    {
      local.AsOfDate.Date = Now().Date;
    }
    else
    {
      local.AsOfDate.Date = import.AsOfDate.Date;
    }

    if (IsEmpty(import.CsePerson.Number) && IsEmpty
      (import.CsePersonsWorkSet.Number))
    {
      ExitState = "FN0000_CSE_PERS_NO_NOT_PASSED";

      return;
    }

    if (!IsEmpty(import.CsePerson.Number))
    {
      local.CsePerson.Number = import.CsePerson.Number;
    }
    else if (!IsEmpty(import.CsePersonsWorkSet.Number))
    {
      local.CsePerson.Number = import.CsePersonsWorkSet.Number;
    }

    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    export.ActiveAddressFound.Flag = "N";

    // =================================================
    // 1/21/99 - b adams  -  selection of 'end_date >= ' changed to
    //   'end_date >'
    // =================================================
    foreach(var item in ReadCsePersonAddress2())
    {
      export.ActiveAddressFound.Flag = "Y";

      if (Lt(local.Previous.VerifiedDate, entities.CsePersonAddress.VerifiedDate))
        
      {
        // <<< This is true only for the First Time >>>
        if (AsChar(entities.CsePersonAddress.Type1) == 'M')
        {
          local.Temp.Assign(entities.CsePersonAddress);

          break;
        }
        else
        {
          local.Temp.Assign(entities.CsePersonAddress);
          MoveCsePersonAddress(entities.CsePersonAddress, local.Previous);

          continue;
        }
      }
      else if (Lt(entities.CsePersonAddress.VerifiedDate,
        local.Previous.VerifiedDate))
      {
        // <<<The Start_Date of the current record is less than the Previous 
        // Start_Date.>>>
        //    So the previous address is the desired address
        break;
      }
      else
      {
        // <<< Current Address start_date = Previous Address start_date >>>
        if (AsChar(entities.CsePersonAddress.Type1) == 'M')
        {
          local.Temp.Assign(entities.CsePersonAddress);

          break;
        }

        if (AsChar(entities.CsePersonAddress.Type1) == 'R')
        {
          if (AsChar(local.Previous.Type1) != 'M')
          {
            local.Temp.Assign(entities.CsePersonAddress);
            MoveCsePersonAddress(entities.CsePersonAddress, local.Previous);
          }
          else
          {
            // <<< Retain the previous values and Temp address >>>
          }
        }
        else
        {
          // =================================================
          // 1/7/99 - B Adams  -  The only address types that are valid any more
          // are just M and R.
          // =================================================
          ExitState = "FN0000_INVALID_ADDRESS_TYPE_FND";

          return;

          if (AsChar(entities.CsePersonAddress.Type1) == 'S')
          {
            if (AsChar(local.Previous.Type1) != 'R')
            {
              local.Temp.Assign(entities.CsePersonAddress);
              MoveCsePersonAddress(entities.CsePersonAddress, local.Previous);
            }
            else
            {
              // <<< Retain the previous values and Temp address >>>
            }
          }
          else if (AsChar(entities.CsePersonAddress.Type1) == 'P')
          {
            if (AsChar(local.Previous.Type1) != 'R' && AsChar
              (local.Previous.Type1) != 'S')
            {
              local.Temp.Assign(entities.CsePersonAddress);
              MoveCsePersonAddress(entities.CsePersonAddress, local.Previous);
            }
            else
            {
              // <<< Retain the previous values and Temp address >>>
            }
          }
          else if (AsChar(entities.CsePersonAddress.Type1) == 'F')
          {
            if (AsChar(local.Previous.Type1) != 'R' && AsChar
              (local.Previous.Type1) != 'S' && AsChar(local.Previous.Type1) != 'P'
              )
            {
              local.Temp.Assign(entities.CsePersonAddress);
              MoveCsePersonAddress(entities.CsePersonAddress, local.Previous);
            }
            else
            {
              // <<< Retain the previous values and Temp address >>>
            }
          }
          else
          {
          }
        }
      }
    }

    // <<< Check if there is an Address with Source_type = "REC" which is 
    // currently active and more recent than the one which was found out in the
    // previous step >>>
    //  It is confirmed that there will be no date overlap between the "REC" 
    // type addresses for a person
    // 03/04/1998  RBM  Per Tom Redmond, the REC type of addresses will never 
    // have a VERIFIED Code and are assumed to be VERIFIED under every
    // situation.
    // So the checking for the verified code 'VE' is removed from the following 
    // read.
    export.CsePersonAddress.Assign(local.Temp);

    if (ReadCsePersonAddress1())
    {
      export.CsePersonAddress.Assign(entities.CsePersonAddress);
      export.ActiveAddressFound.Flag = "Y";
    }
  }

  private static void MoveCsePersonAddress(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.ZdelStartDate = source.ZdelStartDate;
    target.Type1 = source.Type1;
    target.VerifiedDate = source.VerifiedDate;
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", local.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadCsePersonAddress1()
  {
    entities.CsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "verifiedDate1", local.AsOfDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "verifiedDate2",
          local.Temp.VerifiedDate.GetValueOrDefault());
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
        entities.CsePersonAddress.CreatedTimestamp =
          db.GetNullableDateTime(reader, 16);
        entities.CsePersonAddress.CreatedBy = db.GetNullableString(reader, 17);
        entities.CsePersonAddress.State = db.GetNullableString(reader, 18);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 19);
        entities.CsePersonAddress.Zip4 = db.GetNullableString(reader, 20);
        entities.CsePersonAddress.Zip3 = db.GetNullableString(reader, 21);
        entities.CsePersonAddress.Street3 = db.GetNullableString(reader, 22);
        entities.CsePersonAddress.Street4 = db.GetNullableString(reader, 23);
        entities.CsePersonAddress.Province = db.GetNullableString(reader, 24);
        entities.CsePersonAddress.PostalCode = db.GetNullableString(reader, 25);
        entities.CsePersonAddress.Country = db.GetNullableString(reader, 26);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 27);
        entities.CsePersonAddress.County = db.GetNullableString(reader, 28);
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);
      });
  }

  private IEnumerable<bool> ReadCsePersonAddress2()
  {
    entities.CsePersonAddress.Populated = false;

    return ReadEach("ReadCsePersonAddress2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "verifiedDate", local.AsOfDate.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
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
        entities.CsePersonAddress.CreatedTimestamp =
          db.GetNullableDateTime(reader, 16);
        entities.CsePersonAddress.CreatedBy = db.GetNullableString(reader, 17);
        entities.CsePersonAddress.State = db.GetNullableString(reader, 18);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 19);
        entities.CsePersonAddress.Zip4 = db.GetNullableString(reader, 20);
        entities.CsePersonAddress.Zip3 = db.GetNullableString(reader, 21);
        entities.CsePersonAddress.Street3 = db.GetNullableString(reader, 22);
        entities.CsePersonAddress.Street4 = db.GetNullableString(reader, 23);
        entities.CsePersonAddress.Province = db.GetNullableString(reader, 24);
        entities.CsePersonAddress.PostalCode = db.GetNullableString(reader, 25);
        entities.CsePersonAddress.Country = db.GetNullableString(reader, 26);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 27);
        entities.CsePersonAddress.County = db.GetNullableString(reader, 28);
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);

        return true;
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
    /// A value of AsOfDate.
    /// </summary>
    [JsonPropertyName("asOfDate")]
    public DateWorkArea AsOfDate
    {
      get => asOfDate ??= new();
      set => asOfDate = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private DateWorkArea asOfDate;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ActiveAddressFound.
    /// </summary>
    [JsonPropertyName("activeAddressFound")]
    public Common ActiveAddressFound
    {
      get => activeAddressFound ??= new();
      set => activeAddressFound = value;
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

    private Common activeAddressFound;
    private CsePersonAddress csePersonAddress;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
    /// A value of ActiveAddressFound.
    /// </summary>
    [JsonPropertyName("activeAddressFound")]
    public Common ActiveAddressFound
    {
      get => activeAddressFound ??= new();
      set => activeAddressFound = value;
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
    /// A value of AsOfDate.
    /// </summary>
    [JsonPropertyName("asOfDate")]
    public DateWorkArea AsOfDate
    {
      get => asOfDate ??= new();
      set => asOfDate = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public CsePersonAddress Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public CsePersonAddress Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    private CsePerson csePerson;
    private Common activeAddressFound;
    private DateWorkArea initialized;
    private DateWorkArea asOfDate;
    private CsePersonAddress previous;
    private CsePersonAddress temp;
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
