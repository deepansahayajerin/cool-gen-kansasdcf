// Program: SI_READ_AP_LOCATE_CURRENT, ID: 372511599, model: 746.
// Short name: SWE01198
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
/// A program: SI_READ_AP_LOCATE_CURRENT.
/// </para>
/// <para>
/// RESP: SRVINIT		
/// This PAB will populate the CSE - Interstate
/// AP Current - IAPC  PRAD.
/// </para>
/// </summary>
[Serializable]
public partial class SiReadApLocateCurrent: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_READ_AP_LOCATE_CURRENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiReadApLocateCurrent(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiReadApLocateCurrent.
  /// </summary>
  public SiReadApLocateCurrent(IContext context, Import import, Export export):
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
    //         M A I N T E N A N C E   l O G
    // Date     Developer    Request #  Description
    // 4/20/95  Sherri Newman        0  Initial Dev.
    // ---------------------------------------------
    if (ReadInterstateApLocate())
    {
      // ---------------------------------------------
      //  Format Spouse Name.
      // ---------------------------------------------
      local.CsePersonsWorkSet.FirstName =
        entities.InterstateApLocate.CurrentSpouseFirstName ?? Spaces(12);
      local.CsePersonsWorkSet.MiddleInitial =
        Substring(entities.InterstateApLocate.CurrentSpouseMiddleName, 1, 1);
      local.CsePersonsWorkSet.LastName =
        entities.InterstateApLocate.CurrentSpouseLastName ?? Spaces(17);
      UseSiFormatCsePersonName1();
      MoveInterstateApLocate(entities.InterstateApLocate,
        export.InterstateApLocate);

      // Move employer information into the group view.
      export.Employment.Index = 0;
      export.Employment.CheckSize();

      export.Employment.Update.InterstateApLocate.EmployerAddressLine1 =
        entities.InterstateApLocate.EmployerAddressLine1;
      export.Employment.Update.InterstateApLocate.EmployerAddressLine2 =
        entities.InterstateApLocate.EmployerAddressLine2;
      export.Employment.Update.InterstateApLocate.EmployerAreaCode =
        entities.InterstateApLocate.EmployerAreaCode;
      export.Employment.Update.InterstateApLocate.EmployerCity =
        entities.InterstateApLocate.EmployerCity;
      export.Employment.Update.InterstateApLocate.EmployerConfirmedInd =
        entities.InterstateApLocate.EmployerConfirmedInd;
      export.Employment.Update.InterstateApLocate.EmployerEffectiveDate =
        entities.InterstateApLocate.EmployerEffectiveDate;
      export.Employment.Update.InterstateApLocate.EmployerEin =
        entities.InterstateApLocate.EmployerEin;
      export.Employment.Update.InterstateApLocate.EmployerEndDate =
        entities.InterstateApLocate.EmployerEndDate;
      export.Employment.Update.InterstateApLocate.EmployerName =
        entities.InterstateApLocate.EmployerName;
      export.Employment.Update.InterstateApLocate.EmployerPhoneNum =
        entities.InterstateApLocate.EmployerPhoneNum;
      export.Employment.Update.InterstateApLocate.EmployerState =
        entities.InterstateApLocate.EmployerState;
      export.Employment.Update.InterstateApLocate.EmployerZipCode5 =
        entities.InterstateApLocate.EmployerZipCode5;
      export.Employment.Update.InterstateApLocate.EmployerZipCode4 =
        entities.InterstateApLocate.EmployerZipCode4;
      export.Employment.Update.InterstateApLocate.WageYear =
        entities.InterstateApLocate.WageYear;
      export.Employment.Update.InterstateApLocate.WageQtr =
        entities.InterstateApLocate.WageQtr;
      export.Employment.Update.InterstateApLocate.WageAmount =
        entities.InterstateApLocate.WageAmount;

      if (!IsEmpty(entities.InterstateApLocate.Employer2Name))
      {
        ++export.Employment.Index;
        export.Employment.CheckSize();

        export.Employment.Update.InterstateApLocate.EmployerAddressLine1 =
          entities.InterstateApLocate.Employer2AddressLine1;
        export.Employment.Update.InterstateApLocate.EmployerAddressLine2 =
          entities.InterstateApLocate.Employer2AddressLine2;
        export.Employment.Update.InterstateApLocate.EmployerCity =
          entities.InterstateApLocate.Employer2City;
        export.Employment.Update.InterstateApLocate.EmployerConfirmedInd =
          entities.InterstateApLocate.Employer2ConfirmedIndicator;
        export.Employment.Update.InterstateApLocate.EmployerEffectiveDate =
          entities.InterstateApLocate.Employer2EffectiveDate;
        export.Employment.Update.InterstateApLocate.EmployerEin =
          entities.InterstateApLocate.Employer2Ein;
        export.Employment.Update.InterstateApLocate.EmployerEndDate =
          entities.InterstateApLocate.Employer2EndDate;
        export.Employment.Update.InterstateApLocate.EmployerName =
          entities.InterstateApLocate.Employer2Name;
        export.Employment.Update.InterstateApLocate.EmployerState =
          entities.InterstateApLocate.Employer2State;
        export.Employment.Update.InterstateApLocate.WageYear =
          entities.InterstateApLocate.Employer2WageYear;
        export.Employment.Update.InterstateApLocate.WageQtr =
          entities.InterstateApLocate.Employer2WageQuarter;
        export.Employment.Update.InterstateApLocate.WageAmount =
          entities.InterstateApLocate.Employer2WageAmount;
        export.Employment.Update.InterstateApLocate.EmployerAreaCode =
          (int?)StringToNumber(entities.InterstateApLocate.Employer2AreaCode);
        export.Employment.Update.InterstateApLocate.EmployerPhoneNum =
          (int?)StringToNumber(entities.InterstateApLocate.Employer2PhoneNumber);
          
        export.Employment.Update.InterstateApLocate.EmployerZipCode5 =
          NumberToString(entities.InterstateApLocate.Employer2ZipCode5.
            GetValueOrDefault(), 11, 5);
        export.Employment.Update.InterstateApLocate.EmployerZipCode4 =
          NumberToString(entities.InterstateApLocate.Employer2ZipCode4.
            GetValueOrDefault(), 12, 4);
      }

      if (!IsEmpty(entities.InterstateApLocate.Employer3Name))
      {
        ++export.Employment.Index;
        export.Employment.CheckSize();

        export.Employment.Update.InterstateApLocate.EmployerAddressLine1 =
          entities.InterstateApLocate.Employer3AddressLine1;
        export.Employment.Update.InterstateApLocate.EmployerAddressLine2 =
          entities.InterstateApLocate.Employer3AddressLine2;
        export.Employment.Update.InterstateApLocate.EmployerCity =
          entities.InterstateApLocate.Employer3City;
        export.Employment.Update.InterstateApLocate.EmployerConfirmedInd =
          entities.InterstateApLocate.Employer3ConfirmedIndicator;
        export.Employment.Update.InterstateApLocate.EmployerEffectiveDate =
          entities.InterstateApLocate.Employer3EffectiveDate;
        export.Employment.Update.InterstateApLocate.EmployerEin =
          entities.InterstateApLocate.Employer3Ein;
        export.Employment.Update.InterstateApLocate.EmployerEndDate =
          entities.InterstateApLocate.Employer3EndDate;
        export.Employment.Update.InterstateApLocate.EmployerName =
          entities.InterstateApLocate.Employer3Name;
        export.Employment.Update.InterstateApLocate.EmployerState =
          entities.InterstateApLocate.Employer3State;
        export.Employment.Update.InterstateApLocate.WageYear =
          entities.InterstateApLocate.Employer3WageYear;
        export.Employment.Update.InterstateApLocate.WageQtr =
          entities.InterstateApLocate.Employer3WageQuarter;
        export.Employment.Update.InterstateApLocate.WageAmount =
          entities.InterstateApLocate.Employer3WageAmount;
        export.Employment.Update.InterstateApLocate.EmployerAreaCode =
          (int?)StringToNumber(entities.InterstateApLocate.Employer3AreaCode);
        export.Employment.Update.InterstateApLocate.EmployerPhoneNum =
          (int?)StringToNumber(entities.InterstateApLocate.Employer3PhoneNumber);
          
        export.Employment.Update.InterstateApLocate.EmployerZipCode5 =
          NumberToString(entities.InterstateApLocate.Employer3ZipCode5.
            GetValueOrDefault(), 11, 5);
        export.Employment.Update.InterstateApLocate.EmployerZipCode4 =
          NumberToString(entities.InterstateApLocate.Employer3ZipCode5.
            GetValueOrDefault(), 12, 4);
      }

      if (ReadInterstateApIdentification())
      {
        // ---------------------------------------------
        //  Format Absent Parent Name.
        // ---------------------------------------------
        local.CsePersonsWorkSet.FirstName =
          entities.InterstateApIdentification.NameFirst;
        local.CsePersonsWorkSet.MiddleInitial =
          Substring(entities.InterstateApIdentification.MiddleName, 1, 1);
        local.CsePersonsWorkSet.LastName =
          entities.InterstateApIdentification.NameLast ?? Spaces(17);
        UseSiFormatCsePersonName2();
        MoveInterstateApIdentification(entities.InterstateApIdentification,
          export.InterstateApIdentification);
      }
      else
      {
        ExitState = "CSENET_AP_ID_NF";
      }
    }
    else
    {
      ExitState = "CSENET_AP_LOCATE_NF";
    }
  }

  private static void MoveInterstateApIdentification(
    InterstateApIdentification source, InterstateApIdentification target)
  {
    target.Ssn = source.Ssn;
    target.DateOfBirth = source.DateOfBirth;
  }

  private static void MoveInterstateApLocate(InterstateApLocate source,
    InterstateApLocate target)
  {
    target.ResidentialAddressLine1 = source.ResidentialAddressLine1;
    target.ResidentialAddressLine2 = source.ResidentialAddressLine2;
    target.ResidentialCity = source.ResidentialCity;
    target.ResidentialState = source.ResidentialState;
    target.ResidentialZipCode5 = source.ResidentialZipCode5;
    target.ResidentialZipCode4 = source.ResidentialZipCode4;
    target.MailingAddressLine1 = source.MailingAddressLine1;
    target.MailingAddressLine2 = source.MailingAddressLine2;
    target.MailingCity = source.MailingCity;
    target.MailingState = source.MailingState;
    target.MailingZipCode5 = source.MailingZipCode5;
    target.MailingZipCode4 = source.MailingZipCode4;
    target.ResidentialAddressEffectvDate = source.ResidentialAddressEffectvDate;
    target.ResidentialAddressEndDate = source.ResidentialAddressEndDate;
    target.ResidentialAddressConfirmInd = source.ResidentialAddressConfirmInd;
    target.MailingAddressEffectiveDate = source.MailingAddressEffectiveDate;
    target.MailingAddressEndDate = source.MailingAddressEndDate;
    target.MailingAddressConfirmedInd = source.MailingAddressConfirmedInd;
    target.HomePhoneNumber = source.HomePhoneNumber;
    target.WorkPhoneNumber = source.WorkPhoneNumber;
    target.DriversLicState = source.DriversLicState;
    target.DriversLicenseNum = source.DriversLicenseNum;
    target.Occupation = source.Occupation;
    target.WageQtr = source.WageQtr;
    target.WageYear = source.WageYear;
    target.WageAmount = source.WageAmount;
    target.InsuranceCarrierName = source.InsuranceCarrierName;
    target.InsurancePolicyNum = source.InsurancePolicyNum;
    target.ProfessionalLicenses = source.ProfessionalLicenses;
    target.WorkAreaCode = source.WorkAreaCode;
    target.HomeAreaCode = source.HomeAreaCode;
  }

  private void UseSiFormatCsePersonName1()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    export.Spouse.FormattedName = useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseSiFormatCsePersonName2()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    export.Ap.FormattedName = useExport.CsePersonsWorkSet.FormattedName;
  }

  private bool ReadInterstateApIdentification()
  {
    entities.InterstateApIdentification.Populated = false;

    return Read("ReadInterstateApIdentification",
      (db, command) =>
      {
        db.SetInt64(
          command, "ccaTransSerNum", import.InterstateCase.TransSerialNumber);
        db.SetDate(
          command, "ccaTransactionDt",
          import.InterstateCase.TransactionDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateApIdentification.CcaTransactionDt =
          db.GetDate(reader, 0);
        entities.InterstateApIdentification.CcaTransSerNum =
          db.GetInt64(reader, 1);
        entities.InterstateApIdentification.Ssn =
          db.GetNullableString(reader, 2);
        entities.InterstateApIdentification.DateOfBirth =
          db.GetNullableDate(reader, 3);
        entities.InterstateApIdentification.NameFirst = db.GetString(reader, 4);
        entities.InterstateApIdentification.NameLast =
          db.GetNullableString(reader, 5);
        entities.InterstateApIdentification.MiddleName =
          db.GetNullableString(reader, 6);
        entities.InterstateApIdentification.Populated = true;
      });
  }

  private bool ReadInterstateApLocate()
  {
    entities.InterstateApLocate.Populated = false;

    return Read("ReadInterstateApLocate",
      (db, command) =>
      {
        db.SetInt64(
          command, "cncTransSerlNbr", import.InterstateCase.TransSerialNumber);
        db.SetDate(
          command, "cncTransactionDt",
          import.InterstateCase.TransactionDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateApLocate.CncTransactionDt = db.GetDate(reader, 0);
        entities.InterstateApLocate.CncTransSerlNbr = db.GetInt64(reader, 1);
        entities.InterstateApLocate.EmployerEin =
          db.GetNullableInt32(reader, 2);
        entities.InterstateApLocate.EmployerName =
          db.GetNullableString(reader, 3);
        entities.InterstateApLocate.EmployerPhoneNum =
          db.GetNullableInt32(reader, 4);
        entities.InterstateApLocate.EmployerEffectiveDate =
          db.GetNullableDate(reader, 5);
        entities.InterstateApLocate.EmployerEndDate =
          db.GetNullableDate(reader, 6);
        entities.InterstateApLocate.EmployerConfirmedInd =
          db.GetNullableString(reader, 7);
        entities.InterstateApLocate.ResidentialAddressLine1 =
          db.GetNullableString(reader, 8);
        entities.InterstateApLocate.ResidentialAddressLine2 =
          db.GetNullableString(reader, 9);
        entities.InterstateApLocate.ResidentialCity =
          db.GetNullableString(reader, 10);
        entities.InterstateApLocate.ResidentialState =
          db.GetNullableString(reader, 11);
        entities.InterstateApLocate.ResidentialZipCode5 =
          db.GetNullableString(reader, 12);
        entities.InterstateApLocate.ResidentialZipCode4 =
          db.GetNullableString(reader, 13);
        entities.InterstateApLocate.MailingAddressLine1 =
          db.GetNullableString(reader, 14);
        entities.InterstateApLocate.MailingAddressLine2 =
          db.GetNullableString(reader, 15);
        entities.InterstateApLocate.MailingCity =
          db.GetNullableString(reader, 16);
        entities.InterstateApLocate.MailingState =
          db.GetNullableString(reader, 17);
        entities.InterstateApLocate.MailingZipCode5 =
          db.GetNullableString(reader, 18);
        entities.InterstateApLocate.MailingZipCode4 =
          db.GetNullableString(reader, 19);
        entities.InterstateApLocate.ResidentialAddressEffectvDate =
          db.GetNullableDate(reader, 20);
        entities.InterstateApLocate.ResidentialAddressEndDate =
          db.GetNullableDate(reader, 21);
        entities.InterstateApLocate.ResidentialAddressConfirmInd =
          db.GetNullableString(reader, 22);
        entities.InterstateApLocate.MailingAddressEffectiveDate =
          db.GetNullableDate(reader, 23);
        entities.InterstateApLocate.MailingAddressEndDate =
          db.GetNullableDate(reader, 24);
        entities.InterstateApLocate.MailingAddressConfirmedInd =
          db.GetNullableString(reader, 25);
        entities.InterstateApLocate.HomePhoneNumber =
          db.GetNullableInt32(reader, 26);
        entities.InterstateApLocate.WorkPhoneNumber =
          db.GetNullableInt32(reader, 27);
        entities.InterstateApLocate.DriversLicState =
          db.GetNullableString(reader, 28);
        entities.InterstateApLocate.DriversLicenseNum =
          db.GetNullableString(reader, 29);
        entities.InterstateApLocate.CurrentSpouseFirstName =
          db.GetNullableString(reader, 30);
        entities.InterstateApLocate.CurrentSpouseMiddleName =
          db.GetNullableString(reader, 31);
        entities.InterstateApLocate.CurrentSpouseLastName =
          db.GetNullableString(reader, 32);
        entities.InterstateApLocate.Occupation =
          db.GetNullableString(reader, 33);
        entities.InterstateApLocate.EmployerAddressLine1 =
          db.GetNullableString(reader, 34);
        entities.InterstateApLocate.EmployerAddressLine2 =
          db.GetNullableString(reader, 35);
        entities.InterstateApLocate.EmployerCity =
          db.GetNullableString(reader, 36);
        entities.InterstateApLocate.EmployerState =
          db.GetNullableString(reader, 37);
        entities.InterstateApLocate.EmployerZipCode5 =
          db.GetNullableString(reader, 38);
        entities.InterstateApLocate.EmployerZipCode4 =
          db.GetNullableString(reader, 39);
        entities.InterstateApLocate.WageQtr = db.GetNullableInt32(reader, 40);
        entities.InterstateApLocate.WageYear = db.GetNullableInt32(reader, 41);
        entities.InterstateApLocate.WageAmount =
          db.GetNullableDecimal(reader, 42);
        entities.InterstateApLocate.InsuranceCarrierName =
          db.GetNullableString(reader, 43);
        entities.InterstateApLocate.InsurancePolicyNum =
          db.GetNullableString(reader, 44);
        entities.InterstateApLocate.ProfessionalLicenses =
          db.GetNullableString(reader, 45);
        entities.InterstateApLocate.WorkAreaCode =
          db.GetNullableInt32(reader, 46);
        entities.InterstateApLocate.HomeAreaCode =
          db.GetNullableInt32(reader, 47);
        entities.InterstateApLocate.EmployerAreaCode =
          db.GetNullableInt32(reader, 48);
        entities.InterstateApLocate.Employer2Name =
          db.GetNullableString(reader, 49);
        entities.InterstateApLocate.Employer2Ein =
          db.GetNullableInt32(reader, 50);
        entities.InterstateApLocate.Employer2PhoneNumber =
          db.GetNullableString(reader, 51);
        entities.InterstateApLocate.Employer2AreaCode =
          db.GetNullableString(reader, 52);
        entities.InterstateApLocate.Employer2AddressLine1 =
          db.GetNullableString(reader, 53);
        entities.InterstateApLocate.Employer2AddressLine2 =
          db.GetNullableString(reader, 54);
        entities.InterstateApLocate.Employer2City =
          db.GetNullableString(reader, 55);
        entities.InterstateApLocate.Employer2State =
          db.GetNullableString(reader, 56);
        entities.InterstateApLocate.Employer2ZipCode5 =
          db.GetNullableInt32(reader, 57);
        entities.InterstateApLocate.Employer2ZipCode4 =
          db.GetNullableInt32(reader, 58);
        entities.InterstateApLocate.Employer2ConfirmedIndicator =
          db.GetNullableString(reader, 59);
        entities.InterstateApLocate.Employer2EffectiveDate =
          db.GetNullableDate(reader, 60);
        entities.InterstateApLocate.Employer2EndDate =
          db.GetNullableDate(reader, 61);
        entities.InterstateApLocate.Employer2WageAmount =
          db.GetNullableInt64(reader, 62);
        entities.InterstateApLocate.Employer2WageQuarter =
          db.GetNullableInt32(reader, 63);
        entities.InterstateApLocate.Employer2WageYear =
          db.GetNullableInt32(reader, 64);
        entities.InterstateApLocate.Employer3Name =
          db.GetNullableString(reader, 65);
        entities.InterstateApLocate.Employer3Ein =
          db.GetNullableInt32(reader, 66);
        entities.InterstateApLocate.Employer3PhoneNumber =
          db.GetNullableString(reader, 67);
        entities.InterstateApLocate.Employer3AreaCode =
          db.GetNullableString(reader, 68);
        entities.InterstateApLocate.Employer3AddressLine1 =
          db.GetNullableString(reader, 69);
        entities.InterstateApLocate.Employer3AddressLine2 =
          db.GetNullableString(reader, 70);
        entities.InterstateApLocate.Employer3City =
          db.GetNullableString(reader, 71);
        entities.InterstateApLocate.Employer3State =
          db.GetNullableString(reader, 72);
        entities.InterstateApLocate.Employer3ZipCode5 =
          db.GetNullableInt32(reader, 73);
        entities.InterstateApLocate.Employer3ZipCode4 =
          db.GetNullableInt32(reader, 74);
        entities.InterstateApLocate.Employer3ConfirmedIndicator =
          db.GetNullableString(reader, 75);
        entities.InterstateApLocate.Employer3EffectiveDate =
          db.GetNullableDate(reader, 76);
        entities.InterstateApLocate.Employer3EndDate =
          db.GetNullableDate(reader, 77);
        entities.InterstateApLocate.Employer3WageAmount =
          db.GetNullableInt64(reader, 78);
        entities.InterstateApLocate.Employer3WageQuarter =
          db.GetNullableInt32(reader, 79);
        entities.InterstateApLocate.Employer3WageYear =
          db.GetNullableInt32(reader, 80);
        entities.InterstateApLocate.Populated = true;
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
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A EmploymentGroup group.</summary>
    [Serializable]
    public class EmploymentGroup
    {
      /// <summary>
      /// A value of InterstateApLocate.
      /// </summary>
      [JsonPropertyName("interstateApLocate")]
      public InterstateApLocate InterstateApLocate
      {
        get => interstateApLocate ??= new();
        set => interstateApLocate = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private InterstateApLocate interstateApLocate;
    }

    /// <summary>
    /// A value of Spouse.
    /// </summary>
    [JsonPropertyName("spouse")]
    public CsePersonsWorkSet Spouse
    {
      get => spouse ??= new();
      set => spouse = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of InterstateApLocate.
    /// </summary>
    [JsonPropertyName("interstateApLocate")]
    public InterstateApLocate InterstateApLocate
    {
      get => interstateApLocate ??= new();
      set => interstateApLocate = value;
    }

    /// <summary>
    /// A value of InterstateApIdentification.
    /// </summary>
    [JsonPropertyName("interstateApIdentification")]
    public InterstateApIdentification InterstateApIdentification
    {
      get => interstateApIdentification ??= new();
      set => interstateApIdentification = value;
    }

    /// <summary>
    /// Gets a value of Employment.
    /// </summary>
    [JsonIgnore]
    public Array<EmploymentGroup> Employment => employment ??= new(
      EmploymentGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Employment for json serialization.
    /// </summary>
    [JsonPropertyName("employment")]
    [Computed]
    public IList<EmploymentGroup> Employment_Json
    {
      get => employment;
      set => Employment.Assign(value);
    }

    private CsePersonsWorkSet spouse;
    private CsePersonsWorkSet ap;
    private InterstateApLocate interstateApLocate;
    private InterstateApIdentification interstateApIdentification;
    private Array<EmploymentGroup> employment;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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

    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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

    /// <summary>
    /// A value of InterstateApLocate.
    /// </summary>
    [JsonPropertyName("interstateApLocate")]
    public InterstateApLocate InterstateApLocate
    {
      get => interstateApLocate ??= new();
      set => interstateApLocate = value;
    }

    /// <summary>
    /// A value of InterstateApIdentification.
    /// </summary>
    [JsonPropertyName("interstateApIdentification")]
    public InterstateApIdentification InterstateApIdentification
    {
      get => interstateApIdentification ??= new();
      set => interstateApIdentification = value;
    }

    private InterstateCase interstateCase;
    private InterstateApLocate interstateApLocate;
    private InterstateApIdentification interstateApIdentification;
  }
#endregion
}
