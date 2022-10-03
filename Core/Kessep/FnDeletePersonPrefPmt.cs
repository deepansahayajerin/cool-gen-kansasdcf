// Program: FN_DELETE_PERSON_PREF_PMT, ID: 371881979, model: 746.
// Short name: SWE00427
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DELETE_PERSON_PREF_PMT.
/// </summary>
[Serializable]
public partial class FnDeletePersonPrefPmt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DELETE_PERSON_PREF_PMT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDeletePersonPrefPmt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDeletePersonPrefPmt.
  /// </summary>
  public FnDeletePersonPrefPmt(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadPersonPreferredPaymentMethod())
    {
      DeletePersonPreferredPaymentMethod();
    }
    else
    {
      ExitState = "PERSON_PREFERRED_PAYMENT_MET_NF";
    }
  }

  private void DeletePersonPreferredPaymentMethod()
  {
    Update("DeletePersonPreferredPaymentMethod",
      (db, command) =>
      {
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
  }

  private bool ReadPersonPreferredPaymentMethod()
  {
    entities.PersonPreferredPaymentMethod.Populated = false;

    return Read("ReadPersonPreferredPaymentMethod",
      (db, command) =>
      {
        db.SetInt32(
          command, "persnPmntMethId",
          import.PersonPreferredPaymentMethod.SystemGeneratedIdentifier);
        db.SetString(command, "cspPNumber", import.CsePersonsWorkSet.Number);
        db.SetInt32(
          command, "pmtGeneratedId",
          import.PaymentMethodType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PersonPreferredPaymentMethod.PmtGeneratedId =
          db.GetInt32(reader, 0);
        entities.PersonPreferredPaymentMethod.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.PersonPreferredPaymentMethod.EffectiveDate =
          db.GetDate(reader, 2);
        entities.PersonPreferredPaymentMethod.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.PersonPreferredPaymentMethod.CspPNumber =
          db.GetString(reader, 4);
        entities.PersonPreferredPaymentMethod.Populated = true;
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

    private PaymentMethodType paymentMethodType;
    private CsePerson csePerson;
    private CsePersonAccount obligee;
    private PersonPreferredPaymentMethod personPreferredPaymentMethod;
  }
#endregion
}
