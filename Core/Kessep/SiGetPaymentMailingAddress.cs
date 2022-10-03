// Program: SI_GET_PAYMENT_MAILING_ADDRESS, ID: 372382224, model: 746.
// Short name: SWE01178
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
/// A program: SI_GET_PAYMENT_MAILING_ADDRESS.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiGetPaymentMailingAddress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_GET_PAYMENT_MAILING_ADDRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiGetPaymentMailingAddress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiGetPaymentMailingAddress.
  /// </summary>
  public SiGetPaymentMailingAddress(IContext context, Import import,
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
    // *********************************************************************************
    //    Date    Developer  WR#/PR#       Description
    // ---------------------------------------------------------------------------------
    // 04/01/1999 Carl Ott                 Initial Development.
    // *********************************************************************************
    // 02/08/2001 swsrchf    I00112308     Added check for PAYCTR (Kansas Pay 
    // Center)
    // *********************************************************************************
    // *********************************************************************************
    // 07/12/07  G. Pan   PR218020
    //                    added logic to read Interstate_request.
    //                    if it is outgoing case, then
    //                      SET local_kpc_nbr cse_person number TO "000004188O".
    //                    Un-delete the imports entity view for case and
    //                      cse_person inorder to read Interstate_request.
    //                    Added local work view for date_work_area.
    //                    Added Case, Case_role, Interstate_request and
    //                      Legal_action_case_role entity view in ENTITY 
    // ACTIONS.
    // *********************************************************************************
    if (ReadLegalAction())
    {
      if (Equal(entities.LegalAction.PaymentLocation, "COURT"))
      {
        if (ReadTribunalFipsTribAddress())
        {
          export.InterstateCase.PaymentMailingAddressLine1 =
            entities.FipsTribAddress.Street1;
          export.InterstateCase.PaymentAddressLine2 =
            entities.FipsTribAddress.Street2;
          export.InterstateCase.PaymentCity = entities.FipsTribAddress.City;
          export.InterstateCase.PaymentState = entities.FipsTribAddress.State;
          export.InterstateCase.PaymentZipCode5 =
            entities.FipsTribAddress.ZipCode;
          export.InterstateCase.PaymentZipCode4 = entities.FipsTribAddress.Zip4;
        }
        else if (ReadConvAltBillingAddress())
        {
          export.InterstateCase.PaymentMailingAddressLine1 =
            entities.ConvAltBillingAddress.BillingLine1;
          export.InterstateCase.PaymentAddressLine2 =
            entities.ConvAltBillingAddress.BillingLine2;
          export.InterstateCase.PaymentCity =
            entities.ConvAltBillingAddress.BillingCity;
          export.InterstateCase.PaymentState =
            entities.ConvAltBillingAddress.BillingState;
          export.InterstateCase.PaymentZipCode5 =
            entities.ConvAltBillingAddress.BillingZipCode;
          export.InterstateCase.PaymentZipCode4 =
            entities.ConvAltBillingAddress.BillingZip4;
        }
        else
        {
          ExitState = "LE0000_TRIBUNAL_ADDRESS_NF";
        }
      }
      else if (Equal(entities.LegalAction.PaymentLocation, "CSE") || Equal
        (entities.LegalAction.PaymentLocation, "DIRECT"))
      {
        if (ReadCsePerson2())
        {
          UseSiGetCsePersonMailingAddr2();

          if (!IsEmpty(local.CsePersonAddress.LocationType))
          {
            export.InterstateCase.PaymentMailingAddressLine1 =
              local.CsePersonAddress.Street1 ?? "";
            export.InterstateCase.PaymentAddressLine2 =
              local.CsePersonAddress.Street2 ?? "";
            export.InterstateCase.PaymentCity = local.CsePersonAddress.City ?? ""
              ;
            export.InterstateCase.PaymentState =
              local.CsePersonAddress.State ?? "";
            export.InterstateCase.PaymentZipCode5 =
              local.CsePersonAddress.ZipCode ?? "";
            export.InterstateCase.PaymentZipCode4 =
              local.CsePersonAddress.Zip4 ?? "";
          }
        }
        else
        {
          ExitState = "CSE_PERSON_NF";
        }
      }
      else
      {
        // *** Problem request I00112308
        // *** 02/08/01 swsrchf
        // *** start
        if (Equal(entities.LegalAction.PaymentLocation, "PAYCTR"))
        {
          // *********************************************************************************
          // #PR218020 Read outgoing interstate request using import AP person 
          // and case,if a valid
          //   outgoing interstate request exists, SET KPC number "000004188O" 
          // to get fips trib address.
          // *********************************************************************************
          local.KpcNbr.Number = "";
          local.CurrentDate.Date = Now().Date;

          if (ReadInterstateRequest())
          {
            local.KpcNbr.Number = "000004188O";
          }

          if (IsEmpty(local.KpcNbr.Number))
          {
            if (ReadCsePerson1())
            {
              local.KpcNbr.Number = entities.KeyOnly.Number;
            }
            else
            {
              local.KpcNbr.Number = "000004188O";
            }
          }

          foreach(var item in ReadCsePersonFipsTribAddress())
          {
            if (!Equal(entities.ExistingFipsTribAddress.Country, "US") && !
              IsEmpty(entities.ExistingFipsTribAddress.Country) && IsEmpty
              (entities.ExistingFipsTribAddress.State))
            {
              continue;
            }

            export.InterstateCase.PaymentMailingAddressLine1 =
              entities.ExistingFipsTribAddress.Street1;
            export.InterstateCase.PaymentAddressLine2 =
              entities.ExistingFipsTribAddress.Street2;
            export.InterstateCase.PaymentCity =
              entities.ExistingFipsTribAddress.City;
            export.InterstateCase.PaymentState =
              entities.ExistingFipsTribAddress.State;
            export.InterstateCase.PaymentZipCode5 =
              entities.ExistingFipsTribAddress.ZipCode;
            export.InterstateCase.PaymentZipCode4 =
              entities.ExistingFipsTribAddress.Zip4;

            return;
          }

          UseSiGetCsePersonMailingAddr1();

          if (!IsEmpty(local.CsePersonAddress.LocationType))
          {
            export.InterstateCase.PaymentMailingAddressLine1 =
              local.CsePersonAddress.Street1 ?? "";
            export.InterstateCase.PaymentAddressLine2 =
              local.CsePersonAddress.Street2 ?? "";
            export.InterstateCase.PaymentCity = local.CsePersonAddress.City ?? ""
              ;
            export.InterstateCase.PaymentState =
              local.CsePersonAddress.State ?? "";
            export.InterstateCase.PaymentZipCode5 =
              local.CsePersonAddress.ZipCode ?? "";
            export.InterstateCase.PaymentZipCode4 =
              local.CsePersonAddress.Zip4 ?? "";
          }
        }

        // *** end
        // *** 02/08/01 swsrchf
        // *** Problem request I00112308
      }
    }
    else
    {
      ExitState = "LEGAL_ACTION_DETAIL_NF";
    }
  }

  private void UseSiGetCsePersonMailingAddr1()
  {
    var useImport = new SiGetCsePersonMailingAddr.Import();
    var useExport = new SiGetCsePersonMailingAddr.Export();

    useImport.CsePerson.Number = local.KpcNbr.Number;

    Call(SiGetCsePersonMailingAddr.Execute, useImport, useExport);

    local.CsePersonAddress.Assign(useExport.CsePersonAddress);
  }

  private void UseSiGetCsePersonMailingAddr2()
  {
    var useImport = new SiGetCsePersonMailingAddr.Import();
    var useExport = new SiGetCsePersonMailingAddr.Export();

    useImport.CsePerson.Number = entities.ExistingCsePerson.Number;

    Call(SiGetCsePersonMailingAddr.Execute, useImport, useExport);

    local.CsePersonAddress.Assign(useExport.CsePersonAddress);
  }

  private bool ReadConvAltBillingAddress()
  {
    entities.ConvAltBillingAddress.Populated = false;

    return Read("ReadConvAltBillingAddress",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.ConvAltBillingAddress.BillingLine1 =
          db.GetNullableString(reader, 0);
        entities.ConvAltBillingAddress.BillingLine2 =
          db.GetNullableString(reader, 1);
        entities.ConvAltBillingAddress.BillingCity =
          db.GetNullableString(reader, 2);
        entities.ConvAltBillingAddress.BillingState =
          db.GetNullableString(reader, 3);
        entities.ConvAltBillingAddress.BillingZipCode =
          db.GetNullableString(reader, 4);
        entities.ConvAltBillingAddress.BillingZip4 =
          db.GetNullableString(reader, 5);
        entities.ConvAltBillingAddress.LgaIdentifier = db.GetInt32(reader, 6);
        entities.ConvAltBillingAddress.Populated = true;
      });
  }

  private bool ReadCsePerson1()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.KeyOnly.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.LegalAction.CspNumber ?? "");
      },
      (db, reader) =>
      {
        entities.KeyOnly.Number = db.GetString(reader, 0);
        entities.KeyOnly.Type1 = db.GetString(reader, 1);
        entities.KeyOnly.Populated = true;
        CheckValid<CsePerson>("Type1", entities.KeyOnly.Type1);
      });
  }

  private bool ReadCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.ExistingCsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.LegalAction.CspNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingCsePerson.Type1 = db.GetString(reader, 1);
        entities.ExistingCsePerson.OrganizationName =
          db.GetNullableString(reader, 2);
        entities.ExistingCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ExistingCsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePersonFipsTribAddress()
  {
    entities.ExistingFipsTribAddress.Populated = false;
    entities.ExistingCsePerson.Populated = false;

    return ReadEach("ReadCsePersonFipsTribAddress",
      (db, command) =>
      {
        db.SetString(command, "numb", local.KpcNbr.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingCsePerson.Type1 = db.GetString(reader, 1);
        entities.ExistingCsePerson.OrganizationName =
          db.GetNullableString(reader, 2);
        entities.ExistingFipsTribAddress.Identifier = db.GetInt32(reader, 3);
        entities.ExistingFipsTribAddress.Street1 = db.GetString(reader, 4);
        entities.ExistingFipsTribAddress.Street2 =
          db.GetNullableString(reader, 5);
        entities.ExistingFipsTribAddress.City = db.GetString(reader, 6);
        entities.ExistingFipsTribAddress.State = db.GetString(reader, 7);
        entities.ExistingFipsTribAddress.ZipCode = db.GetString(reader, 8);
        entities.ExistingFipsTribAddress.Zip4 = db.GetNullableString(reader, 9);
        entities.ExistingFipsTribAddress.Country =
          db.GetNullableString(reader, 10);
        entities.ExistingFipsTribAddress.CreatedTstamp =
          db.GetDateTime(reader, 11);
        entities.ExistingFipsTribAddress.FipState =
          db.GetNullableInt32(reader, 12);
        entities.ExistingFipsTribAddress.FipCounty =
          db.GetNullableInt32(reader, 13);
        entities.ExistingFipsTribAddress.FipLocation =
          db.GetNullableInt32(reader, 14);
        entities.ExistingFipsTribAddress.Populated = true;
        entities.ExistingCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ExistingCsePerson.Type1);

        return true;
      });
  }

  private bool ReadInterstateRequest()
  {
    entities.ExistingInterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", import.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.CurrentDate.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.Ap.Number);
        db.SetInt32(command, "lgaId", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingInterstateRequest.IntHGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingInterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 1);
        entities.ExistingInterstateRequest.KsCaseInd = db.GetString(reader, 2);
        entities.ExistingInterstateRequest.CasINumber =
          db.GetNullableString(reader, 3);
        entities.ExistingInterstateRequest.Populated = true;
      });
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.PaymentLocation = db.GetNullableString(reader, 1);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 2);
        entities.LegalAction.CspNumber = db.GetNullableString(reader, 3);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadTribunalFipsTribAddress()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.FipsTribAddress.Populated = false;
    entities.Tribunal.Populated = false;

    return Read("ReadTribunalFipsTribAddress",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "trbId", entities.LegalAction.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Tribunal.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 1);
        entities.FipsTribAddress.FaxExtension = db.GetNullableString(reader, 2);
        entities.FipsTribAddress.FaxAreaCode = db.GetNullableInt32(reader, 3);
        entities.FipsTribAddress.PhoneExtension =
          db.GetNullableString(reader, 4);
        entities.FipsTribAddress.AreaCode = db.GetNullableInt32(reader, 5);
        entities.FipsTribAddress.Type1 = db.GetString(reader, 6);
        entities.FipsTribAddress.Street1 = db.GetString(reader, 7);
        entities.FipsTribAddress.Street2 = db.GetNullableString(reader, 8);
        entities.FipsTribAddress.City = db.GetString(reader, 9);
        entities.FipsTribAddress.State = db.GetString(reader, 10);
        entities.FipsTribAddress.ZipCode = db.GetString(reader, 11);
        entities.FipsTribAddress.Zip4 = db.GetNullableString(reader, 12);
        entities.FipsTribAddress.Zip3 = db.GetNullableString(reader, 13);
        entities.FipsTribAddress.County = db.GetNullableString(reader, 14);
        entities.FipsTribAddress.Street3 = db.GetNullableString(reader, 15);
        entities.FipsTribAddress.Street4 = db.GetNullableString(reader, 16);
        entities.FipsTribAddress.Province = db.GetNullableString(reader, 17);
        entities.FipsTribAddress.PostalCode = db.GetNullableString(reader, 18);
        entities.FipsTribAddress.Country = db.GetNullableString(reader, 19);
        entities.FipsTribAddress.PhoneNumber = db.GetNullableInt32(reader, 20);
        entities.FipsTribAddress.FaxNumber = db.GetNullableInt32(reader, 21);
        entities.FipsTribAddress.CreatedBy = db.GetString(reader, 22);
        entities.FipsTribAddress.CreatedTstamp = db.GetDateTime(reader, 23);
        entities.FipsTribAddress.LastUpdatedBy =
          db.GetNullableString(reader, 24);
        entities.FipsTribAddress.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 25);
        entities.FipsTribAddress.TrbId = db.GetNullableInt32(reader, 26);
        entities.FipsTribAddress.Populated = true;
        entities.Tribunal.Populated = true;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private LegalAction legalAction;
    private CsePerson ap;
    private Case1 case1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    private InterstateCase interstateCase;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CurrentDate.
    /// </summary>
    [JsonPropertyName("currentDate")]
    public DateWorkArea CurrentDate
    {
      get => currentDate ??= new();
      set => currentDate = value;
    }

    /// <summary>
    /// A value of KpcNbr.
    /// </summary>
    [JsonPropertyName("kpcNbr")]
    public CsePerson KpcNbr
    {
      get => kpcNbr ??= new();
      set => kpcNbr = value;
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

    private DateWorkArea currentDate;
    private CsePerson kpcNbr;
    private CsePersonAddress csePersonAddress;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingLegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("existingLegalActionCaseRole")]
    public LegalActionCaseRole ExistingLegalActionCaseRole
    {
      get => existingLegalActionCaseRole ??= new();
      set => existingLegalActionCaseRole = value;
    }

    /// <summary>
    /// A value of ExistingCaseRole.
    /// </summary>
    [JsonPropertyName("existingCaseRole")]
    public CaseRole ExistingCaseRole
    {
      get => existingCaseRole ??= new();
      set => existingCaseRole = value;
    }

    /// <summary>
    /// A value of ExistingCase.
    /// </summary>
    [JsonPropertyName("existingCase")]
    public Case1 ExistingCase
    {
      get => existingCase ??= new();
      set => existingCase = value;
    }

    /// <summary>
    /// A value of ExistingInterstateRequest.
    /// </summary>
    [JsonPropertyName("existingInterstateRequest")]
    public InterstateRequest ExistingInterstateRequest
    {
      get => existingInterstateRequest ??= new();
      set => existingInterstateRequest = value;
    }

    /// <summary>
    /// A value of KeyOnly.
    /// </summary>
    [JsonPropertyName("keyOnly")]
    public CsePerson KeyOnly
    {
      get => keyOnly ??= new();
      set => keyOnly = value;
    }

    /// <summary>
    /// A value of ExistingFips.
    /// </summary>
    [JsonPropertyName("existingFips")]
    public Fips ExistingFips
    {
      get => existingFips ??= new();
      set => existingFips = value;
    }

    /// <summary>
    /// A value of ExistingFipsTribAddress.
    /// </summary>
    [JsonPropertyName("existingFipsTribAddress")]
    public FipsTribAddress ExistingFipsTribAddress
    {
      get => existingFipsTribAddress ??= new();
      set => existingFipsTribAddress = value;
    }

    /// <summary>
    /// A value of ConvAltBillingAddress.
    /// </summary>
    [JsonPropertyName("convAltBillingAddress")]
    public ConvAltBillingAddress ConvAltBillingAddress
    {
      get => convAltBillingAddress ??= new();
      set => convAltBillingAddress = value;
    }

    /// <summary>
    /// A value of ExistingCsePerson.
    /// </summary>
    [JsonPropertyName("existingCsePerson")]
    public CsePerson ExistingCsePerson
    {
      get => existingCsePerson ??= new();
      set => existingCsePerson = value;
    }

    /// <summary>
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    private LegalActionCaseRole existingLegalActionCaseRole;
    private CaseRole existingCaseRole;
    private Case1 existingCase;
    private InterstateRequest existingInterstateRequest;
    private CsePerson keyOnly;
    private Fips existingFips;
    private FipsTribAddress existingFipsTribAddress;
    private ConvAltBillingAddress convAltBillingAddress;
    private CsePerson existingCsePerson;
    private FipsTribAddress fipsTribAddress;
    private Tribunal tribunal;
    private LegalAction legalAction;
  }
#endregion
}
