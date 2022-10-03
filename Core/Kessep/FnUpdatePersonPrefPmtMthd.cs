// Program: FN_UPDATE_PERSON_PREF_PMT_MTHD, ID: 371882022, model: 746.
// Short name: SWE00671
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_UPDATE_PERSON_PREF_PMT_MTHD.
/// </summary>
[Serializable]
public partial class FnUpdatePersonPrefPmtMthd: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_UPDATE_PERSON_PREF_PMT_MTHD program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnUpdatePersonPrefPmtMthd(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnUpdatePersonPrefPmtMthd.
  /// </summary>
  public FnUpdatePersonPrefPmtMthd(IContext context, Import import,
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
    local.FlagUpdate.Flag = "";

    if (ReadPersonPreferredPaymentMethodPaymentMethodType())
    {
      local.FlagUpdate.Flag = "Y";
    }

    // -------------------------------------
    // Retrieve the first record for the same person.
    // -------------------------------------
    ReadPersonPreferredPaymentMethod2();

    // -------------------------------------
    // Retrieve the last record for the same person.
    // -------------------------------------
    ReadPersonPreferredPaymentMethod3();

    // -------------------------------------
    // Compare the two.
    // -------------------------------------
    if (!Lt(entities.Test1.DiscontinueDate,
      import.PersonPreferredPaymentMethod.DiscontinueDate) || !
      Lt(import.PersonPreferredPaymentMethod.EffectiveDate,
      entities.Test2.EffectiveDate))
    {
      local.FlagUpdate.Flag = "Y";
    }
    else if (entities.Test1.SystemGeneratedIdentifier != entities
      .Test2.SystemGeneratedIdentifier && (
        !Lt(entities.Test1.EffectiveDate,
      import.PersonPreferredPaymentMethod.EffectiveDate) && !
      Lt(import.PersonPreferredPaymentMethod.DiscontinueDate,
      entities.Test2.DiscontinueDate) || !
      Lt(import.PersonPreferredPaymentMethod.DiscontinueDate,
      entities.Test2.DiscontinueDate) && import
      .PersonPreferredPaymentMethod.SystemGeneratedIdentifier != entities
      .Test2.SystemGeneratedIdentifier || !
      Lt(entities.Test1.EffectiveDate,
      import.PersonPreferredPaymentMethod.EffectiveDate) && import
      .PersonPreferredPaymentMethod.SystemGeneratedIdentifier != entities
      .Test1.SystemGeneratedIdentifier))
    {
      ExitState = "ACO_NE0000_DATE_OVERLAP";

      return;
    }
    else
    {
      foreach(var item in ReadPersonPreferredPaymentMethod4())
      {
        // ---------------------------------------------
        // N.Engoor - 02/01/99
        // Check for any date overlaps with existing records for the same person
        // before doing an updation.
        // ---------------------------------------------
        if (Lt(entities.PersonPreferredPaymentMethod.EffectiveDate,
          import.PersonPreferredPaymentMethod.DiscontinueDate) && Lt
          (import.PersonPreferredPaymentMethod.DiscontinueDate,
          entities.PersonPreferredPaymentMethod.DiscontinueDate) || Lt
          (entities.PersonPreferredPaymentMethod.EffectiveDate,
          import.PersonPreferredPaymentMethod.EffectiveDate) && Lt
          (import.PersonPreferredPaymentMethod.EffectiveDate,
          entities.PersonPreferredPaymentMethod.DiscontinueDate) || Equal
          (import.PersonPreferredPaymentMethod.EffectiveDate,
          entities.PersonPreferredPaymentMethod.EffectiveDate) || Equal
          (import.PersonPreferredPaymentMethod.DiscontinueDate,
          entities.PersonPreferredPaymentMethod.DiscontinueDate) || Equal
          (import.PersonPreferredPaymentMethod.DiscontinueDate,
          entities.PersonPreferredPaymentMethod.EffectiveDate) || Equal
          (import.PersonPreferredPaymentMethod.EffectiveDate,
          entities.PersonPreferredPaymentMethod.DiscontinueDate))
        {
          ExitState = "ACO_NE0000_DATE_OVERLAP";

          return;
        }
        else
        {
          // ---------------------------------------------
          // N.Engoor - 02/01/99
          // Retrieve the next record.
          // ---------------------------------------------
          continue;
        }
      }

      local.FlagUpdate.Flag = "Y";
    }

    if (AsChar(local.FlagUpdate.Flag) == 'Y')
    {
      if (ReadPersonPreferredPaymentMethod1())
      {
        try
        {
          UpdatePersonPreferredPaymentMethod();
          export.PersonPreferredPaymentMethod.Assign(
            entities.PersonPreferredPaymentMethod);
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "PERSON_PREFERRED_PAYMENT_MET_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "PERSON_PREFERRED_PAYMENT_MET_PV";

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

  private bool ReadPersonPreferredPaymentMethod1()
  {
    entities.PersonPreferredPaymentMethod.Populated = false;

    return Read("ReadPersonPreferredPaymentMethod1",
      (db, command) =>
      {
        db.SetInt32(
          command, "persnPmntMethId",
          import.PersonPreferredPaymentMethod.SystemGeneratedIdentifier);
        db.SetString(command, "cspPNumber", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.PersonPreferredPaymentMethod.PmtGeneratedId =
          db.GetInt32(reader, 0);
        entities.PersonPreferredPaymentMethod.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.PersonPreferredPaymentMethod.AbaRoutingNumber =
          db.GetNullableInt64(reader, 2);
        entities.PersonPreferredPaymentMethod.DfiAccountNumber =
          db.GetNullableString(reader, 3);
        entities.PersonPreferredPaymentMethod.EffectiveDate =
          db.GetDate(reader, 4);
        entities.PersonPreferredPaymentMethod.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.PersonPreferredPaymentMethod.CreatedBy =
          db.GetString(reader, 6);
        entities.PersonPreferredPaymentMethod.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.PersonPreferredPaymentMethod.LastUpdateBy =
          db.GetNullableString(reader, 8);
        entities.PersonPreferredPaymentMethod.LastUpdateTmst =
          db.GetNullableDateTime(reader, 9);
        entities.PersonPreferredPaymentMethod.CspPNumber =
          db.GetString(reader, 10);
        entities.PersonPreferredPaymentMethod.Description =
          db.GetNullableString(reader, 11);
        entities.PersonPreferredPaymentMethod.AccountType =
          db.GetNullableString(reader, 12);
        entities.PersonPreferredPaymentMethod.Populated = true;
      });
  }

  private bool ReadPersonPreferredPaymentMethod2()
  {
    entities.Test1.Populated = false;

    return Read("ReadPersonPreferredPaymentMethod2",
      (db, command) =>
      {
        db.SetString(command, "cspPNumber", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.Test1.PmtGeneratedId = db.GetInt32(reader, 0);
        entities.Test1.SystemGeneratedIdentifier = db.GetInt32(reader, 1);
        entities.Test1.AbaRoutingNumber = db.GetNullableInt64(reader, 2);
        entities.Test1.DfiAccountNumber = db.GetNullableString(reader, 3);
        entities.Test1.EffectiveDate = db.GetDate(reader, 4);
        entities.Test1.DiscontinueDate = db.GetNullableDate(reader, 5);
        entities.Test1.CreatedBy = db.GetString(reader, 6);
        entities.Test1.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.Test1.LastUpdateBy = db.GetNullableString(reader, 8);
        entities.Test1.LastUpdateTmst = db.GetNullableDateTime(reader, 9);
        entities.Test1.CspPNumber = db.GetString(reader, 10);
        entities.Test1.Description = db.GetNullableString(reader, 11);
        entities.Test1.AccountType = db.GetNullableString(reader, 12);
        entities.Test1.Populated = true;
      });
  }

  private bool ReadPersonPreferredPaymentMethod3()
  {
    entities.Test2.Populated = false;

    return Read("ReadPersonPreferredPaymentMethod3",
      (db, command) =>
      {
        db.SetString(command, "cspPNumber", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.Test2.PmtGeneratedId = db.GetInt32(reader, 0);
        entities.Test2.SystemGeneratedIdentifier = db.GetInt32(reader, 1);
        entities.Test2.AbaRoutingNumber = db.GetNullableInt64(reader, 2);
        entities.Test2.DfiAccountNumber = db.GetNullableString(reader, 3);
        entities.Test2.EffectiveDate = db.GetDate(reader, 4);
        entities.Test2.DiscontinueDate = db.GetNullableDate(reader, 5);
        entities.Test2.CreatedBy = db.GetString(reader, 6);
        entities.Test2.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.Test2.LastUpdateBy = db.GetNullableString(reader, 8);
        entities.Test2.LastUpdateTmst = db.GetNullableDateTime(reader, 9);
        entities.Test2.CspPNumber = db.GetString(reader, 10);
        entities.Test2.Description = db.GetNullableString(reader, 11);
        entities.Test2.AccountType = db.GetNullableString(reader, 12);
        entities.Test2.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPersonPreferredPaymentMethod4()
  {
    entities.PersonPreferredPaymentMethod.Populated = false;

    return ReadEach("ReadPersonPreferredPaymentMethod4",
      (db, command) =>
      {
        db.SetInt32(
          command, "persnPmntMethId",
          import.PersonPreferredPaymentMethod.SystemGeneratedIdentifier);
        db.SetString(command, "cspPNumber", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.PersonPreferredPaymentMethod.PmtGeneratedId =
          db.GetInt32(reader, 0);
        entities.PersonPreferredPaymentMethod.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.PersonPreferredPaymentMethod.AbaRoutingNumber =
          db.GetNullableInt64(reader, 2);
        entities.PersonPreferredPaymentMethod.DfiAccountNumber =
          db.GetNullableString(reader, 3);
        entities.PersonPreferredPaymentMethod.EffectiveDate =
          db.GetDate(reader, 4);
        entities.PersonPreferredPaymentMethod.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.PersonPreferredPaymentMethod.CreatedBy =
          db.GetString(reader, 6);
        entities.PersonPreferredPaymentMethod.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.PersonPreferredPaymentMethod.LastUpdateBy =
          db.GetNullableString(reader, 8);
        entities.PersonPreferredPaymentMethod.LastUpdateTmst =
          db.GetNullableDateTime(reader, 9);
        entities.PersonPreferredPaymentMethod.CspPNumber =
          db.GetString(reader, 10);
        entities.PersonPreferredPaymentMethod.Description =
          db.GetNullableString(reader, 11);
        entities.PersonPreferredPaymentMethod.AccountType =
          db.GetNullableString(reader, 12);
        entities.PersonPreferredPaymentMethod.Populated = true;

        return true;
      });
  }

  private bool ReadPersonPreferredPaymentMethodPaymentMethodType()
  {
    entities.PaymentMethodType.Populated = false;
    entities.PersonPreferredPaymentMethod.Populated = false;

    return Read("ReadPersonPreferredPaymentMethodPaymentMethodType",
      (db, command) =>
      {
        db.SetInt32(
          command, "persnPmntMethId",
          import.PersonPreferredPaymentMethod.SystemGeneratedIdentifier);
        db.SetDate(
          command, "effectiveDate",
          import.PersonPreferredPaymentMethod.EffectiveDate.
            GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate",
          import.PersonPreferredPaymentMethod.DiscontinueDate.
            GetValueOrDefault());
        db.SetString(command, "cspPNumber", import.CsePersonsWorkSet.Number);
        db.SetInt32(
          command, "paymntMethTypId",
          import.PaymentMethodType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PersonPreferredPaymentMethod.PmtGeneratedId =
          db.GetInt32(reader, 0);
        entities.PaymentMethodType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PersonPreferredPaymentMethod.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.PersonPreferredPaymentMethod.AbaRoutingNumber =
          db.GetNullableInt64(reader, 2);
        entities.PersonPreferredPaymentMethod.DfiAccountNumber =
          db.GetNullableString(reader, 3);
        entities.PersonPreferredPaymentMethod.EffectiveDate =
          db.GetDate(reader, 4);
        entities.PersonPreferredPaymentMethod.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.PersonPreferredPaymentMethod.CreatedBy =
          db.GetString(reader, 6);
        entities.PersonPreferredPaymentMethod.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.PersonPreferredPaymentMethod.LastUpdateBy =
          db.GetNullableString(reader, 8);
        entities.PersonPreferredPaymentMethod.LastUpdateTmst =
          db.GetNullableDateTime(reader, 9);
        entities.PersonPreferredPaymentMethod.CspPNumber =
          db.GetString(reader, 10);
        entities.PersonPreferredPaymentMethod.Description =
          db.GetNullableString(reader, 11);
        entities.PersonPreferredPaymentMethod.AccountType =
          db.GetNullableString(reader, 12);
        entities.PaymentMethodType.Code = db.GetString(reader, 13);
        entities.PaymentMethodType.EffectiveDate = db.GetDate(reader, 14);
        entities.PaymentMethodType.DiscontinueDate =
          db.GetNullableDate(reader, 15);
        entities.PaymentMethodType.Populated = true;
        entities.PersonPreferredPaymentMethod.Populated = true;
      });
  }

  private void UpdatePersonPreferredPaymentMethod()
  {
    System.Diagnostics.Debug.Assert(
      entities.PersonPreferredPaymentMethod.Populated);

    var abaRoutingNumber = import.Routing.AbaRoutingNumber.GetValueOrDefault();
    var dfiAccountNumber =
      import.PersonPreferredPaymentMethod.DfiAccountNumber ?? "";
    var effectiveDate = import.PersonPreferredPaymentMethod.EffectiveDate;
    var discontinueDate = import.PersonPreferredPaymentMethod.DiscontinueDate;
    var lastUpdateBy = global.UserId;
    var lastUpdateTmst = Now();
    var description = import.PersonPreferredPaymentMethod.Description ?? "";
    var accountType = import.PersonPreferredPaymentMethod.AccountType ?? "";

    entities.PersonPreferredPaymentMethod.Populated = false;
    Update("UpdatePersonPreferredPaymentMethod",
      (db, command) =>
      {
        db.SetNullableInt64(command, "abaRoutingNumber", abaRoutingNumber);
        db.SetNullableString(command, "dfiAccountNo", dfiAccountNumber);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdateBy", lastUpdateBy);
        db.SetNullableDateTime(command, "lastUpdateTmst", lastUpdateTmst);
        db.SetNullableString(command, "description", description);
        db.SetNullableString(command, "accountType", accountType);
        db.SetInt32(
          command, "pmtGeneratedId",
          entities.PersonPreferredPaymentMethod.PmtGeneratedId);
        db.SetInt32(
          command, "persnPmntMethId",
          entities.PersonPreferredPaymentMethod.SystemGeneratedIdentifier);
        db.SetString(
          command, "cspPNumber",
          entities.PersonPreferredPaymentMethod.CspPNumber);
      });

    entities.PersonPreferredPaymentMethod.AbaRoutingNumber = abaRoutingNumber;
    entities.PersonPreferredPaymentMethod.DfiAccountNumber = dfiAccountNumber;
    entities.PersonPreferredPaymentMethod.EffectiveDate = effectiveDate;
    entities.PersonPreferredPaymentMethod.DiscontinueDate = discontinueDate;
    entities.PersonPreferredPaymentMethod.LastUpdateBy = lastUpdateBy;
    entities.PersonPreferredPaymentMethod.LastUpdateTmst = lastUpdateTmst;
    entities.PersonPreferredPaymentMethod.Description = description;
    entities.PersonPreferredPaymentMethod.AccountType = accountType;
    entities.PersonPreferredPaymentMethod.Populated = true;
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
    /// A value of Routing.
    /// </summary>
    [JsonPropertyName("routing")]
    public PersonPreferredPaymentMethod Routing
    {
      get => routing ??= new();
      set => routing = value;
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
    /// A value of PaymentMethodType.
    /// </summary>
    [JsonPropertyName("paymentMethodType")]
    public PaymentMethodType PaymentMethodType
    {
      get => paymentMethodType ??= new();
      set => paymentMethodType = value;
    }

    /// <summary>
    /// A value of PersonPreferredPaymentMethod.
    /// </summary>
    [JsonPropertyName("personPreferredPaymentMethod")]
    public PersonPreferredPaymentMethod PersonPreferredPaymentMethod
    {
      get => personPreferredPaymentMethod ??= new();
      set => personPreferredPaymentMethod = value;
    }

    private PersonPreferredPaymentMethod routing;
    private CsePersonsWorkSet csePersonsWorkSet;
    private PaymentMethodType paymentMethodType;
    private PersonPreferredPaymentMethod personPreferredPaymentMethod;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of PaymentMethodType.
    /// </summary>
    [JsonPropertyName("paymentMethodType")]
    public PaymentMethodType PaymentMethodType
    {
      get => paymentMethodType ??= new();
      set => paymentMethodType = value;
    }

    /// <summary>
    /// A value of PersonPreferredPaymentMethod.
    /// </summary>
    [JsonPropertyName("personPreferredPaymentMethod")]
    public PersonPreferredPaymentMethod PersonPreferredPaymentMethod
    {
      get => personPreferredPaymentMethod ??= new();
      set => personPreferredPaymentMethod = value;
    }

    private PaymentMethodType paymentMethodType;
    private PersonPreferredPaymentMethod personPreferredPaymentMethod;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of NoChange.
    /// </summary>
    [JsonPropertyName("noChange")]
    public Common NoChange
    {
      get => noChange ??= new();
      set => noChange = value;
    }

    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public PersonPreferredPaymentMethod Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public PersonPreferredPaymentMethod Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of PaymentMethodType.
    /// </summary>
    [JsonPropertyName("paymentMethodType")]
    public PaymentMethodType PaymentMethodType
    {
      get => paymentMethodType ??= new();
      set => paymentMethodType = value;
    }

    /// <summary>
    /// A value of PersonPreferredPaymentMethod.
    /// </summary>
    [JsonPropertyName("personPreferredPaymentMethod")]
    public PersonPreferredPaymentMethod PersonPreferredPaymentMethod
    {
      get => personPreferredPaymentMethod ??= new();
      set => personPreferredPaymentMethod = value;
    }

    /// <summary>
    /// A value of FlagUpdate.
    /// </summary>
    [JsonPropertyName("flagUpdate")]
    public Common FlagUpdate
    {
      get => flagUpdate ??= new();
      set => flagUpdate = value;
    }

    private Common noChange;
    private PersonPreferredPaymentMethod next;
    private PersonPreferredPaymentMethod prev;
    private PaymentMethodType paymentMethodType;
    private PersonPreferredPaymentMethod personPreferredPaymentMethod;
    private Common flagUpdate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Test.
    /// </summary>
    [JsonPropertyName("test")]
    public PaymentMethodType Test
    {
      get => test ??= new();
      set => test = value;
    }

    /// <summary>
    /// A value of Create.
    /// </summary>
    [JsonPropertyName("create")]
    public PersonPreferredPaymentMethod Create
    {
      get => create ??= new();
      set => create = value;
    }

    /// <summary>
    /// A value of Test2.
    /// </summary>
    [JsonPropertyName("test2")]
    public PersonPreferredPaymentMethod Test2
    {
      get => test2 ??= new();
      set => test2 = value;
    }

    /// <summary>
    /// A value of Test1.
    /// </summary>
    [JsonPropertyName("test1")]
    public PersonPreferredPaymentMethod Test1
    {
      get => test1 ??= new();
      set => test1 = value;
    }

    /// <summary>
    /// A value of PaymentMethodType.
    /// </summary>
    [JsonPropertyName("paymentMethodType")]
    public PaymentMethodType PaymentMethodType
    {
      get => paymentMethodType ??= new();
      set => paymentMethodType = value;
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
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePersonAccount Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
    }

    /// <summary>
    /// A value of PersonPreferredPaymentMethod.
    /// </summary>
    [JsonPropertyName("personPreferredPaymentMethod")]
    public PersonPreferredPaymentMethod PersonPreferredPaymentMethod
    {
      get => personPreferredPaymentMethod ??= new();
      set => personPreferredPaymentMethod = value;
    }

    private PaymentMethodType test;
    private PersonPreferredPaymentMethod create;
    private PersonPreferredPaymentMethod test2;
    private PersonPreferredPaymentMethod test1;
    private PaymentMethodType paymentMethodType;
    private CsePerson csePerson;
    private CsePersonAccount obligee;
    private PersonPreferredPaymentMethod personPreferredPaymentMethod;
  }
#endregion
}
