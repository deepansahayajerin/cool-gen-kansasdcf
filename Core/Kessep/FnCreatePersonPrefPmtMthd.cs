// Program: FN_CREATE_PERSON_PREF_PMT_MTHD, ID: 371881977, model: 746.
// Short name: SWE00386
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CREATE_PERSON_PREF_PMT_MTHD.
/// </summary>
[Serializable]
public partial class FnCreatePersonPrefPmtMthd: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CREATE_PERSON_PREF_PMT_MTHD program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCreatePersonPrefPmtMthd(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCreatePersonPrefPmtMthd.
  /// </summary>
  public FnCreatePersonPrefPmtMthd(IContext context, Import import,
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
    export.PersonPreferredPaymentMethod.Assign(
      import.PersonPreferredPaymentMethod);

    if (ReadPaymentMethodType())
    {
      local.PaymentMethodType.Assign(entities.PaymentMethodType);

      if (ReadCsePerson())
      {
        // -------------------------------------------------------------
        // N.Engoor  -  02/01/99
        // Check if there are any existing records that are overlapping with the
        // new record that is to be created. If no overlaps existing create
        // one.
        // -------------------------------------------------------------
        if (ReadPersonPreferredPaymentMethod())
        {
          ExitState = "ACO_NE0000_DATE_OVERLAP";
        }
        else
        {
          try
          {
            CreatePersonPreferredPaymentMethod();
            export.PersonPreferredPaymentMethod.Assign(entities.New1);
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "PERSON_PREFERRED_PAYMENT_MET_AE";

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
      else
      {
        ExitState = "CSE_PERSON_NF";
      }
    }
  }

  private int UseCabGenerate3DigitRandomNum()
  {
    var useImport = new CabGenerate3DigitRandomNum.Import();
    var useExport = new CabGenerate3DigitRandomNum.Export();

    Call(CabGenerate3DigitRandomNum.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute3DigitRandomNumber;
  }

  private void CreatePersonPreferredPaymentMethod()
  {
    var pmtGeneratedId = entities.PaymentMethodType.SystemGeneratedIdentifier;
    var systemGeneratedIdentifier = UseCabGenerate3DigitRandomNum();
    var abaRoutingNumber = import.Routing.AbaRoutingNumber.GetValueOrDefault();
    var dfiAccountNumber =
      import.PersonPreferredPaymentMethod.DfiAccountNumber ?? "";
    var effectiveDate = import.PersonPreferredPaymentMethod.EffectiveDate;
    var discontinueDate = import.PersonPreferredPaymentMethod.DiscontinueDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var cspPNumber = entities.CsePerson.Number;
    var description = import.PersonPreferredPaymentMethod.Description ?? "";
    var accountType = import.PersonPreferredPaymentMethod.AccountType ?? "";

    entities.New1.Populated = false;
    Update("CreatePersonPreferredPaymentMethod",
      (db, command) =>
      {
        db.SetInt32(command, "pmtGeneratedId", pmtGeneratedId);
        db.SetInt32(command, "persnPmntMethId", systemGeneratedIdentifier);
        db.SetNullableInt64(command, "abaRoutingNumber", abaRoutingNumber);
        db.SetNullableString(command, "dfiAccountNo", dfiAccountNumber);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdateBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdateTmst", createdTimestamp);
        db.SetString(command, "cspPNumber", cspPNumber);
        db.SetNullableString(command, "description", description);
        db.SetNullableString(command, "accountType", accountType);
      });

    entities.New1.PmtGeneratedId = pmtGeneratedId;
    entities.New1.SystemGeneratedIdentifier = systemGeneratedIdentifier;
    entities.New1.AbaRoutingNumber = abaRoutingNumber;
    entities.New1.DfiAccountNumber = dfiAccountNumber;
    entities.New1.EffectiveDate = effectiveDate;
    entities.New1.DiscontinueDate = discontinueDate;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTimestamp = createdTimestamp;
    entities.New1.LastUpdateBy = createdBy;
    entities.New1.LastUpdateTmst = createdTimestamp;
    entities.New1.CspPNumber = cspPNumber;
    entities.New1.Description = description;
    entities.New1.AccountType = accountType;
    entities.New1.Populated = true;
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

  private bool ReadPaymentMethodType()
  {
    entities.PaymentMethodType.Populated = false;

    return Read("ReadPaymentMethodType",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymntMethTypId",
          import.PaymentMethodType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PaymentMethodType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentMethodType.Code = db.GetString(reader, 1);
        entities.PaymentMethodType.EffectiveDate = db.GetDate(reader, 2);
        entities.PaymentMethodType.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.PaymentMethodType.Populated = true;
      });
  }

  private bool ReadPersonPreferredPaymentMethod()
  {
    entities.Existing.Populated = false;

    return Read("ReadPersonPreferredPaymentMethod",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate1",
          import.PersonPreferredPaymentMethod.EffectiveDate.
            GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate2",
          import.PersonPreferredPaymentMethod.DiscontinueDate.
            GetValueOrDefault());
        db.SetString(command, "cspPNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Existing.PmtGeneratedId = db.GetInt32(reader, 0);
        entities.Existing.SystemGeneratedIdentifier = db.GetInt32(reader, 1);
        entities.Existing.AbaRoutingNumber = db.GetNullableInt64(reader, 2);
        entities.Existing.DfiAccountNumber = db.GetNullableString(reader, 3);
        entities.Existing.EffectiveDate = db.GetDate(reader, 4);
        entities.Existing.DiscontinueDate = db.GetNullableDate(reader, 5);
        entities.Existing.CspPNumber = db.GetString(reader, 6);
        entities.Existing.Description = db.GetNullableString(reader, 7);
        entities.Existing.AccountType = db.GetNullableString(reader, 8);
        entities.Existing.Populated = true;
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
    /// A value of PersonPreferredPaymentMethod.
    /// </summary>
    [JsonPropertyName("personPreferredPaymentMethod")]
    public PersonPreferredPaymentMethod PersonPreferredPaymentMethod
    {
      get => personPreferredPaymentMethod ??= new();
      set => personPreferredPaymentMethod = value;
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

    private PersonPreferredPaymentMethod routing;
    private CsePersonsWorkSet csePersonsWorkSet;
    private PersonPreferredPaymentMethod personPreferredPaymentMethod;
    private PaymentMethodType paymentMethodType;
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public PersonPreferredPaymentMethod Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public PersonPreferredPaymentMethod New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    private CsePerson csePerson;
    private CsePersonAccount csePersonAccount;
    private PaymentMethodType paymentMethodType;
    private PersonPreferredPaymentMethod existing;
    private PersonPreferredPaymentMethod new1;
  }
#endregion
}
