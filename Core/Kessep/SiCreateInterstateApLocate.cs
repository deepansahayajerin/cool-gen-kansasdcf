// Program: SI_CREATE_INTERSTATE_AP_LOCATE, ID: 371083472, model: 746.
// Short name: SWE02622
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_CREATE_INTERSTATE_AP_LOCATE.
/// </summary>
[Serializable]
public partial class SiCreateInterstateApLocate: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CREATE_INTERSTATE_AP_LOCATE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCreateInterstateApLocate(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCreateInterstateApLocate.
  /// </summary>
  public SiCreateInterstateApLocate(IContext context, Import import,
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
    // --------------------------------------------------------------------------
    // Date		Developer	Request		Description
    // --------------------------------------------------------------------------
    // 2001/04/16	M Ramirez			Initial Development
    // --------------------------------------------------------------------------
    if (!ReadInterstateApIdentification())
    {
      if (ReadInterstateCase())
      {
        ExitState = "CSENET_AP_ID_NF";
      }
      else
      {
        ExitState = "INTERSTATE_CASE_NF";
      }

      return;
    }

    try
    {
      CreateInterstateApLocate();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "SI0000_INTERSTATE_AP_LOCATE_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "SI0000_INTERSTATE_AP_LOCATE_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void CreateInterstateApLocate()
  {
    System.Diagnostics.Debug.Assert(
      entities.InterstateApIdentification.Populated);

    var cncTransactionDt = entities.InterstateApIdentification.CcaTransactionDt;
    var cncTransSerlNbr = entities.InterstateApIdentification.CcaTransSerNum;
    var employerEin = import.InterstateApLocate.EmployerEin.GetValueOrDefault();
    var employerName = import.InterstateApLocate.EmployerName ?? "";
    var employerPhoneNum =
      import.InterstateApLocate.EmployerPhoneNum.GetValueOrDefault();
    var employerEffectiveDate = import.InterstateApLocate.EmployerEffectiveDate;
    var employerEndDate = import.InterstateApLocate.EmployerEndDate;
    var employerConfirmedInd =
      import.InterstateApLocate.EmployerConfirmedInd ?? "";
    var residentialAddressLine1 =
      import.InterstateApLocate.ResidentialAddressLine1 ?? "";
    var residentialAddressLine2 =
      import.InterstateApLocate.ResidentialAddressLine2 ?? "";
    var residentialCity = import.InterstateApLocate.ResidentialCity ?? "";
    var residentialState = import.InterstateApLocate.ResidentialState ?? "";
    var residentialZipCode5 = import.InterstateApLocate.ResidentialZipCode5 ?? ""
      ;
    var residentialZipCode4 = import.InterstateApLocate.ResidentialZipCode4 ?? ""
      ;
    var mailingAddressLine1 = import.InterstateApLocate.MailingAddressLine1 ?? ""
      ;
    var mailingAddressLine2 = import.InterstateApLocate.MailingAddressLine2 ?? ""
      ;
    var mailingCity = import.InterstateApLocate.MailingCity ?? "";
    var mailingState = import.InterstateApLocate.MailingState ?? "";
    var mailingZipCode5 = import.InterstateApLocate.MailingZipCode5 ?? "";
    var mailingZipCode4 = import.InterstateApLocate.MailingZipCode4 ?? "";
    var residentialAddressEffectvDate =
      import.InterstateApLocate.ResidentialAddressEffectvDate;
    var residentialAddressEndDate =
      import.InterstateApLocate.ResidentialAddressEndDate;
    var residentialAddressConfirmInd =
      import.InterstateApLocate.ResidentialAddressConfirmInd ?? "";
    var mailingAddressEffectiveDate =
      import.InterstateApLocate.MailingAddressEffectiveDate;
    var mailingAddressEndDate = import.InterstateApLocate.MailingAddressEndDate;
    var mailingAddressConfirmedInd =
      import.InterstateApLocate.MailingAddressConfirmedInd ?? "";
    var homePhoneNumber =
      import.InterstateApLocate.HomePhoneNumber.GetValueOrDefault();
    var workPhoneNumber =
      import.InterstateApLocate.WorkPhoneNumber.GetValueOrDefault();
    var driversLicState = import.InterstateApLocate.DriversLicState ?? "";
    var driversLicenseNum = import.InterstateApLocate.DriversLicenseNum ?? "";
    var alias1FirstName = import.InterstateApLocate.Alias1FirstName ?? "";
    var alias1MiddleName = import.InterstateApLocate.Alias1MiddleName ?? "";
    var alias1LastName = import.InterstateApLocate.Alias1LastName ?? "";
    var alias1Suffix = import.InterstateApLocate.Alias1Suffix ?? "";
    var alias2FirstName = import.InterstateApLocate.Alias2FirstName ?? "";
    var alias2MiddleName = import.InterstateApLocate.Alias2MiddleName ?? "";
    var alias2LastName = import.InterstateApLocate.Alias2LastName ?? "";
    var alias2Suffix = import.InterstateApLocate.Alias2Suffix ?? "";
    var alias3FirstName = import.InterstateApLocate.Alias3FirstName ?? "";
    var alias3MiddleName = import.InterstateApLocate.Alias3MiddleName ?? "";
    var alias3LastName = import.InterstateApLocate.Alias3LastName ?? "";
    var alias3Suffix = import.InterstateApLocate.Alias3Suffix ?? "";
    var currentSpouseFirstName =
      import.InterstateApLocate.CurrentSpouseFirstName ?? "";
    var currentSpouseMiddleName =
      import.InterstateApLocate.CurrentSpouseMiddleName ?? "";
    var currentSpouseLastName =
      import.InterstateApLocate.CurrentSpouseLastName ?? "";
    var currentSpouseSuffix = import.InterstateApLocate.CurrentSpouseSuffix ?? ""
      ;
    var occupation = import.InterstateApLocate.Occupation ?? "";
    var employerAddressLine1 =
      import.InterstateApLocate.EmployerAddressLine1 ?? "";
    var employerAddressLine2 =
      import.InterstateApLocate.EmployerAddressLine2 ?? "";
    var employerCity = import.InterstateApLocate.EmployerCity ?? "";
    var employerState = import.InterstateApLocate.EmployerState ?? "";
    var employerZipCode5 = import.InterstateApLocate.EmployerZipCode5 ?? "";
    var employerZipCode4 = import.InterstateApLocate.EmployerZipCode4 ?? "";
    var wageQtr = import.InterstateApLocate.WageQtr.GetValueOrDefault();
    var wageYear = import.InterstateApLocate.WageYear.GetValueOrDefault();
    var wageAmount = import.InterstateApLocate.WageAmount.GetValueOrDefault();
    var insuranceCarrierName =
      import.InterstateApLocate.InsuranceCarrierName ?? "";
    var insurancePolicyNum = import.InterstateApLocate.InsurancePolicyNum ?? "";
    var lastResAddressLine1 = import.InterstateApLocate.LastResAddressLine1 ?? ""
      ;
    var lastResAddressLine2 = import.InterstateApLocate.LastResAddressLine2 ?? ""
      ;
    var lastResCity = import.InterstateApLocate.LastResCity ?? "";
    var lastResState = import.InterstateApLocate.LastResState ?? "";
    var lastResZipCode5 = import.InterstateApLocate.LastResZipCode5 ?? "";
    var lastResZipCode4 = import.InterstateApLocate.LastResZipCode4 ?? "";
    var lastResAddressDate = import.InterstateApLocate.LastResAddressDate;
    var lastMailAddressLine1 =
      import.InterstateApLocate.LastMailAddressLine1 ?? "";
    var lastMailAddressLine2 =
      import.InterstateApLocate.LastMailAddressLine2 ?? "";
    var lastMailCity = import.InterstateApLocate.LastMailCity ?? "";
    var lastMailState = import.InterstateApLocate.LastMailState ?? "";
    var lastMailZipCode5 = import.InterstateApLocate.LastMailZipCode5 ?? "";
    var lastMailZipCode4 = import.InterstateApLocate.LastMailZipCode4 ?? "";
    var lastMailAddressDate = import.InterstateApLocate.LastMailAddressDate;
    var lastEmployerName = import.InterstateApLocate.LastEmployerName ?? "";
    var lastEmployerDate = import.InterstateApLocate.LastEmployerDate;
    var lastEmployerAddressLine1 =
      import.InterstateApLocate.LastEmployerAddressLine1 ?? "";
    var lastEmployerAddressLine2 =
      import.InterstateApLocate.LastEmployerAddressLine2 ?? "";
    var lastEmployerCity = import.InterstateApLocate.LastEmployerCity ?? "";
    var lastEmployerState = import.InterstateApLocate.LastEmployerState ?? "";
    var lastEmployerZipCode5 =
      import.InterstateApLocate.LastEmployerZipCode5 ?? "";
    var lastEmployerZipCode4 =
      import.InterstateApLocate.LastEmployerZipCode4 ?? "";
    var professionalLicenses =
      import.InterstateApLocate.ProfessionalLicenses ?? "";
    var workAreaCode =
      import.InterstateApLocate.WorkAreaCode.GetValueOrDefault();
    var homeAreaCode =
      import.InterstateApLocate.HomeAreaCode.GetValueOrDefault();
    var lastEmployerEndDate = import.InterstateApLocate.LastEmployerEndDate;
    var employerAreaCode =
      import.InterstateApLocate.EmployerAreaCode.GetValueOrDefault();
    var employer2Name = import.InterstateApLocate.Employer2Name ?? "";
    var employer2Ein =
      import.InterstateApLocate.Employer2Ein.GetValueOrDefault();
    var employer2PhoneNumber =
      import.InterstateApLocate.Employer2PhoneNumber ?? "";
    var employer2AreaCode = import.InterstateApLocate.Employer2AreaCode ?? "";
    var employer2AddressLine1 =
      import.InterstateApLocate.Employer2AddressLine1 ?? "";
    var employer2AddressLine2 =
      import.InterstateApLocate.Employer2AddressLine2 ?? "";
    var employer2City = import.InterstateApLocate.Employer2City ?? "";
    var employer2State = import.InterstateApLocate.Employer2State ?? "";
    var employer2ZipCode5 =
      import.InterstateApLocate.Employer2ZipCode5.GetValueOrDefault();
    var employer2ZipCode4 =
      import.InterstateApLocate.Employer2ZipCode4.GetValueOrDefault();
    var employer2ConfirmedIndicator =
      import.InterstateApLocate.Employer2ConfirmedIndicator ?? "";
    var employer2EffectiveDate =
      import.InterstateApLocate.Employer2EffectiveDate;
    var employer2EndDate = import.InterstateApLocate.Employer2EndDate;
    var employer2WageAmount =
      import.InterstateApLocate.Employer2WageAmount.GetValueOrDefault();
    var employer2WageQuarter =
      import.InterstateApLocate.Employer2WageQuarter.GetValueOrDefault();
    var employer2WageYear =
      import.InterstateApLocate.Employer2WageYear.GetValueOrDefault();
    var employer3Name = import.InterstateApLocate.Employer3Name ?? "";
    var employer3Ein =
      import.InterstateApLocate.Employer3Ein.GetValueOrDefault();
    var employer3PhoneNumber =
      import.InterstateApLocate.Employer3PhoneNumber ?? "";
    var employer3AreaCode = import.InterstateApLocate.Employer3AreaCode ?? "";
    var employer3AddressLine1 =
      import.InterstateApLocate.Employer3AddressLine1 ?? "";
    var employer3AddressLine2 =
      import.InterstateApLocate.Employer3AddressLine2 ?? "";
    var employer3City = import.InterstateApLocate.Employer3City ?? "";
    var employer3State = import.InterstateApLocate.Employer3State ?? "";
    var employer3ZipCode5 =
      import.InterstateApLocate.Employer3ZipCode5.GetValueOrDefault();
    var employer3ZipCode4 =
      import.InterstateApLocate.Employer3ZipCode4.GetValueOrDefault();
    var employer3ConfirmedIndicator =
      import.InterstateApLocate.Employer3ConfirmedIndicator ?? "";
    var employer3EffectiveDate =
      import.InterstateApLocate.Employer3EffectiveDate;
    var employer3EndDate = import.InterstateApLocate.Employer3EndDate;
    var employer3WageAmount =
      import.InterstateApLocate.Employer3WageAmount.GetValueOrDefault();
    var employer3WageQuarter =
      import.InterstateApLocate.Employer3WageQuarter.GetValueOrDefault();
    var employer3WageYear =
      import.InterstateApLocate.Employer3WageYear.GetValueOrDefault();

    entities.InterstateApLocate.Populated = false;
    Update("CreateInterstateApLocate",
      (db, command) =>
      {
        db.SetDate(command, "cncTransactionDt", cncTransactionDt);
        db.SetInt64(command, "cncTransSerlNbr", cncTransSerlNbr);
        db.SetNullableInt32(command, "employerEin", employerEin);
        db.SetNullableString(command, "employerName", employerName);
        db.SetNullableInt32(command, "employerPhoneNum", employerPhoneNum);
        db.SetNullableDate(command, "employerEffDate", employerEffectiveDate);
        db.SetNullableDate(command, "employerEndDate", employerEndDate);
        db.SetNullableString(command, "employerCfmdInd", employerConfirmedInd);
        db.SetNullableString(command, "resAddrLine1", residentialAddressLine1);
        db.SetNullableString(command, "resAddrLine2", residentialAddressLine2);
        db.SetNullableString(command, "residentialCity", residentialCity);
        db.SetNullableString(command, "residentialState", residentialState);
        db.SetNullableString(command, "residentialZip5", residentialZipCode5);
        db.SetNullableString(command, "residentialZip4", residentialZipCode4);
        db.SetNullableString(command, "mailingAddrLine1", mailingAddressLine1);
        db.SetNullableString(command, "mailingAddrLine2", mailingAddressLine2);
        db.SetNullableString(command, "mailingCity", mailingCity);
        db.SetNullableString(command, "mailingState", mailingState);
        db.SetNullableString(command, "mailingZip5", mailingZipCode5);
        db.SetNullableString(command, "mailingZip4", mailingZipCode4);
        db.SetNullableDate(
          command, "resAddrEffDate", residentialAddressEffectvDate);
        db.
          SetNullableDate(command, "resAddrEndDate", residentialAddressEndDate);
          
        db.SetNullableString(
          command, "resAddrConInd", residentialAddressConfirmInd);
        db.SetNullableDate(
          command, "mailAddrEffDte", mailingAddressEffectiveDate);
        db.SetNullableDate(command, "mailAddrEndDte", mailingAddressEndDate);
        db.SetNullableString(
          command, "mailAddrConfInd", mailingAddressConfirmedInd);
        db.SetNullableInt32(command, "homePhoneNumber", homePhoneNumber);
        db.SetNullableInt32(command, "workPhoneNumber", workPhoneNumber);
        db.SetNullableString(command, "driversLicState", driversLicState);
        db.SetNullableString(command, "driverLicenseNbr", driversLicenseNum);
        db.SetNullableString(command, "alias1FirstName", alias1FirstName);
        db.SetNullableString(command, "alias1MiddleNam", alias1MiddleName);
        db.SetNullableString(command, "alias1LastName", alias1LastName);
        db.SetNullableString(command, "alias1Suffix", alias1Suffix);
        db.SetNullableString(command, "alias2FirstName", alias2FirstName);
        db.SetNullableString(command, "alias2MiddleNam", alias2MiddleName);
        db.SetNullableString(command, "alias2LastName", alias2LastName);
        db.SetNullableString(command, "alias2Suffix", alias2Suffix);
        db.SetNullableString(command, "alias3FirstName", alias3FirstName);
        db.SetNullableString(command, "alias3MiddleNam", alias3MiddleName);
        db.SetNullableString(command, "alias3LastName", alias3LastName);
        db.SetNullableString(command, "alias3Suffix", alias3Suffix);
        db.
          SetNullableString(command, "currentSpouseFir", currentSpouseFirstName);
          
        db.SetNullableString(
          command, "currentSpouseMid", currentSpouseMiddleName);
        db.
          SetNullableString(command, "currentSpouseLas", currentSpouseLastName);
          
        db.SetNullableString(command, "currentSpouseSuf", currentSpouseSuffix);
        db.SetNullableString(command, "occupation", occupation);
        db.SetNullableString(command, "emplAddrLine1", employerAddressLine1);
        db.SetNullableString(command, "emplAddrLine2", employerAddressLine2);
        db.SetNullableString(command, "employerCity", employerCity);
        db.SetNullableString(command, "employerState", employerState);
        db.SetNullableString(command, "emplZipCode5", employerZipCode5);
        db.SetNullableString(command, "emplZipCode4", employerZipCode4);
        db.SetNullableInt32(command, "wageQtr", wageQtr);
        db.SetNullableInt32(command, "wageYear", wageYear);
        db.SetNullableDecimal(command, "wageAmount", wageAmount);
        db.SetNullableString(command, "insCarrierName", insuranceCarrierName);
        db.SetNullableString(command, "insPolicyNbr", insurancePolicyNum);
        db.SetNullableString(command, "lstResAddrLine1", lastResAddressLine1);
        db.SetNullableString(command, "lstResAddrLine2", lastResAddressLine2);
        db.SetNullableString(command, "lastResCity", lastResCity);
        db.SetNullableString(command, "lastResState", lastResState);
        db.SetNullableString(command, "lstResZipCode5", lastResZipCode5);
        db.SetNullableString(command, "lstResZipCode4", lastResZipCode4);
        db.SetNullableDate(command, "lastResAddrDte", lastResAddressDate);
        db.SetNullableString(command, "lstMailAddrLin1", lastMailAddressLine1);
        db.SetNullableString(command, "lstMailAddrLin2", lastMailAddressLine2);
        db.SetNullableString(command, "lastMailCity", lastMailCity);
        db.SetNullableString(command, "lastMailState", lastMailState);
        db.SetNullableString(command, "lstMailZipCode5", lastMailZipCode5);
        db.SetNullableString(command, "lstMailZipCode4", lastMailZipCode4);
        db.SetNullableDate(command, "lastMailAddrDte", lastMailAddressDate);
        db.SetNullableString(command, "lastEmployerName", lastEmployerName);
        db.SetNullableDate(command, "lastEmployerDate", lastEmployerDate);
        db.SetNullableString(
          command, "lstEmplAddrLin1", lastEmployerAddressLine1);
        db.SetNullableString(
          command, "lstEmplAddrLin2", lastEmployerAddressLine2);
        db.SetNullableString(command, "lastEmployerCity", lastEmployerCity);
        db.SetNullableString(command, "lastEmployerStat", lastEmployerState);
        db.SetNullableString(command, "lstEmplZipCode5", lastEmployerZipCode5);
        db.SetNullableString(command, "lstEmplZipCode4", lastEmployerZipCode4);
        db.SetNullableString(command, "professionalLics", professionalLicenses);
        db.SetNullableInt32(command, "workAreaCode", workAreaCode);
        db.SetNullableInt32(command, "homeAreaCode", homeAreaCode);
        db.SetNullableDate(command, "lastEmpEndDate", lastEmployerEndDate);
        db.SetNullableInt32(command, "employerAreaCode", employerAreaCode);
        db.SetNullableString(command, "employer2Name", employer2Name);
        db.SetNullableInt32(command, "employer2Ein", employer2Ein);
        db.SetNullableString(command, "emp2PhoneNumber", employer2PhoneNumber);
        db.SetNullableString(command, "empl2AreaCode", employer2AreaCode);
        db.SetNullableString(command, "emp2AddrLine1", employer2AddressLine1);
        db.SetNullableString(command, "emp2AddrLine2", employer2AddressLine2);
        db.SetNullableString(command, "employer2City", employer2City);
        db.SetNullableString(command, "employer2State", employer2State);
        db.SetNullableInt32(command, "emp2ZipCode5", employer2ZipCode5);
        db.SetNullableInt32(command, "emp2ZipCode4", employer2ZipCode4);
        db.SetNullableString(
          command, "emp2ConfirmedInd", employer2ConfirmedIndicator);
        db.SetNullableDate(command, "emp2EffectiveDt", employer2EffectiveDate);
        db.SetNullableDate(command, "employer2EndDate", employer2EndDate);
        db.SetNullableInt64(command, "emp2WageAmount", employer2WageAmount);
        db.SetNullableInt32(command, "emp2WageQuarter", employer2WageQuarter);
        db.SetNullableInt32(command, "emp2WageYear", employer2WageYear);
        db.SetNullableString(command, "employer3Name", employer3Name);
        db.SetNullableInt32(command, "employer3Ein", employer3Ein);
        db.SetNullableString(command, "emp3PhoneNumber", employer3PhoneNumber);
        db.SetNullableString(command, "emp3AreaCode", employer3AreaCode);
        db.SetNullableString(command, "emp3AddrLine1", employer3AddressLine1);
        db.SetNullableString(command, "emp3AddrLine2", employer3AddressLine2);
        db.SetNullableString(command, "employer3City", employer3City);
        db.SetNullableString(command, "employer3State", employer3State);
        db.SetNullableInt32(command, "emp3ZipCode5", employer3ZipCode5);
        db.SetNullableInt32(command, "emp3ZipCode4", employer3ZipCode4);
        db.SetNullableString(
          command, "emp3ConfirmedInd", employer3ConfirmedIndicator);
        db.SetNullableDate(command, "emp3EffectiveDt", employer3EffectiveDate);
        db.SetNullableDate(command, "employer3EndDate", employer3EndDate);
        db.SetNullableInt64(command, "emp3WageAmount", employer3WageAmount);
        db.SetNullableInt32(command, "emp3WageQuarter", employer3WageQuarter);
        db.SetNullableInt32(command, "emp3WageYear", employer3WageYear);
      });

    entities.InterstateApLocate.CncTransactionDt = cncTransactionDt;
    entities.InterstateApLocate.CncTransSerlNbr = cncTransSerlNbr;
    entities.InterstateApLocate.EmployerEin = employerEin;
    entities.InterstateApLocate.EmployerName = employerName;
    entities.InterstateApLocate.EmployerPhoneNum = employerPhoneNum;
    entities.InterstateApLocate.EmployerEffectiveDate = employerEffectiveDate;
    entities.InterstateApLocate.EmployerEndDate = employerEndDate;
    entities.InterstateApLocate.EmployerConfirmedInd = employerConfirmedInd;
    entities.InterstateApLocate.ResidentialAddressLine1 =
      residentialAddressLine1;
    entities.InterstateApLocate.ResidentialAddressLine2 =
      residentialAddressLine2;
    entities.InterstateApLocate.ResidentialCity = residentialCity;
    entities.InterstateApLocate.ResidentialState = residentialState;
    entities.InterstateApLocate.ResidentialZipCode5 = residentialZipCode5;
    entities.InterstateApLocate.ResidentialZipCode4 = residentialZipCode4;
    entities.InterstateApLocate.MailingAddressLine1 = mailingAddressLine1;
    entities.InterstateApLocate.MailingAddressLine2 = mailingAddressLine2;
    entities.InterstateApLocate.MailingCity = mailingCity;
    entities.InterstateApLocate.MailingState = mailingState;
    entities.InterstateApLocate.MailingZipCode5 = mailingZipCode5;
    entities.InterstateApLocate.MailingZipCode4 = mailingZipCode4;
    entities.InterstateApLocate.ResidentialAddressEffectvDate =
      residentialAddressEffectvDate;
    entities.InterstateApLocate.ResidentialAddressEndDate =
      residentialAddressEndDate;
    entities.InterstateApLocate.ResidentialAddressConfirmInd =
      residentialAddressConfirmInd;
    entities.InterstateApLocate.MailingAddressEffectiveDate =
      mailingAddressEffectiveDate;
    entities.InterstateApLocate.MailingAddressEndDate = mailingAddressEndDate;
    entities.InterstateApLocate.MailingAddressConfirmedInd =
      mailingAddressConfirmedInd;
    entities.InterstateApLocate.HomePhoneNumber = homePhoneNumber;
    entities.InterstateApLocate.WorkPhoneNumber = workPhoneNumber;
    entities.InterstateApLocate.DriversLicState = driversLicState;
    entities.InterstateApLocate.DriversLicenseNum = driversLicenseNum;
    entities.InterstateApLocate.Alias1FirstName = alias1FirstName;
    entities.InterstateApLocate.Alias1MiddleName = alias1MiddleName;
    entities.InterstateApLocate.Alias1LastName = alias1LastName;
    entities.InterstateApLocate.Alias1Suffix = alias1Suffix;
    entities.InterstateApLocate.Alias2FirstName = alias2FirstName;
    entities.InterstateApLocate.Alias2MiddleName = alias2MiddleName;
    entities.InterstateApLocate.Alias2LastName = alias2LastName;
    entities.InterstateApLocate.Alias2Suffix = alias2Suffix;
    entities.InterstateApLocate.Alias3FirstName = alias3FirstName;
    entities.InterstateApLocate.Alias3MiddleName = alias3MiddleName;
    entities.InterstateApLocate.Alias3LastName = alias3LastName;
    entities.InterstateApLocate.Alias3Suffix = alias3Suffix;
    entities.InterstateApLocate.CurrentSpouseFirstName = currentSpouseFirstName;
    entities.InterstateApLocate.CurrentSpouseMiddleName =
      currentSpouseMiddleName;
    entities.InterstateApLocate.CurrentSpouseLastName = currentSpouseLastName;
    entities.InterstateApLocate.CurrentSpouseSuffix = currentSpouseSuffix;
    entities.InterstateApLocate.Occupation = occupation;
    entities.InterstateApLocate.EmployerAddressLine1 = employerAddressLine1;
    entities.InterstateApLocate.EmployerAddressLine2 = employerAddressLine2;
    entities.InterstateApLocate.EmployerCity = employerCity;
    entities.InterstateApLocate.EmployerState = employerState;
    entities.InterstateApLocate.EmployerZipCode5 = employerZipCode5;
    entities.InterstateApLocate.EmployerZipCode4 = employerZipCode4;
    entities.InterstateApLocate.WageQtr = wageQtr;
    entities.InterstateApLocate.WageYear = wageYear;
    entities.InterstateApLocate.WageAmount = wageAmount;
    entities.InterstateApLocate.InsuranceCarrierName = insuranceCarrierName;
    entities.InterstateApLocate.InsurancePolicyNum = insurancePolicyNum;
    entities.InterstateApLocate.LastResAddressLine1 = lastResAddressLine1;
    entities.InterstateApLocate.LastResAddressLine2 = lastResAddressLine2;
    entities.InterstateApLocate.LastResCity = lastResCity;
    entities.InterstateApLocate.LastResState = lastResState;
    entities.InterstateApLocate.LastResZipCode5 = lastResZipCode5;
    entities.InterstateApLocate.LastResZipCode4 = lastResZipCode4;
    entities.InterstateApLocate.LastResAddressDate = lastResAddressDate;
    entities.InterstateApLocate.LastMailAddressLine1 = lastMailAddressLine1;
    entities.InterstateApLocate.LastMailAddressLine2 = lastMailAddressLine2;
    entities.InterstateApLocate.LastMailCity = lastMailCity;
    entities.InterstateApLocate.LastMailState = lastMailState;
    entities.InterstateApLocate.LastMailZipCode5 = lastMailZipCode5;
    entities.InterstateApLocate.LastMailZipCode4 = lastMailZipCode4;
    entities.InterstateApLocate.LastMailAddressDate = lastMailAddressDate;
    entities.InterstateApLocate.LastEmployerName = lastEmployerName;
    entities.InterstateApLocate.LastEmployerDate = lastEmployerDate;
    entities.InterstateApLocate.LastEmployerAddressLine1 =
      lastEmployerAddressLine1;
    entities.InterstateApLocate.LastEmployerAddressLine2 =
      lastEmployerAddressLine2;
    entities.InterstateApLocate.LastEmployerCity = lastEmployerCity;
    entities.InterstateApLocate.LastEmployerState = lastEmployerState;
    entities.InterstateApLocate.LastEmployerZipCode5 = lastEmployerZipCode5;
    entities.InterstateApLocate.LastEmployerZipCode4 = lastEmployerZipCode4;
    entities.InterstateApLocate.ProfessionalLicenses = professionalLicenses;
    entities.InterstateApLocate.WorkAreaCode = workAreaCode;
    entities.InterstateApLocate.HomeAreaCode = homeAreaCode;
    entities.InterstateApLocate.LastEmployerEndDate = lastEmployerEndDate;
    entities.InterstateApLocate.EmployerAreaCode = employerAreaCode;
    entities.InterstateApLocate.Employer2Name = employer2Name;
    entities.InterstateApLocate.Employer2Ein = employer2Ein;
    entities.InterstateApLocate.Employer2PhoneNumber = employer2PhoneNumber;
    entities.InterstateApLocate.Employer2AreaCode = employer2AreaCode;
    entities.InterstateApLocate.Employer2AddressLine1 = employer2AddressLine1;
    entities.InterstateApLocate.Employer2AddressLine2 = employer2AddressLine2;
    entities.InterstateApLocate.Employer2City = employer2City;
    entities.InterstateApLocate.Employer2State = employer2State;
    entities.InterstateApLocate.Employer2ZipCode5 = employer2ZipCode5;
    entities.InterstateApLocate.Employer2ZipCode4 = employer2ZipCode4;
    entities.InterstateApLocate.Employer2ConfirmedIndicator =
      employer2ConfirmedIndicator;
    entities.InterstateApLocate.Employer2EffectiveDate = employer2EffectiveDate;
    entities.InterstateApLocate.Employer2EndDate = employer2EndDate;
    entities.InterstateApLocate.Employer2WageAmount = employer2WageAmount;
    entities.InterstateApLocate.Employer2WageQuarter = employer2WageQuarter;
    entities.InterstateApLocate.Employer2WageYear = employer2WageYear;
    entities.InterstateApLocate.Employer3Name = employer3Name;
    entities.InterstateApLocate.Employer3Ein = employer3Ein;
    entities.InterstateApLocate.Employer3PhoneNumber = employer3PhoneNumber;
    entities.InterstateApLocate.Employer3AreaCode = employer3AreaCode;
    entities.InterstateApLocate.Employer3AddressLine1 = employer3AddressLine1;
    entities.InterstateApLocate.Employer3AddressLine2 = employer3AddressLine2;
    entities.InterstateApLocate.Employer3City = employer3City;
    entities.InterstateApLocate.Employer3State = employer3State;
    entities.InterstateApLocate.Employer3ZipCode5 = employer3ZipCode5;
    entities.InterstateApLocate.Employer3ZipCode4 = employer3ZipCode4;
    entities.InterstateApLocate.Employer3ConfirmedIndicator =
      employer3ConfirmedIndicator;
    entities.InterstateApLocate.Employer3EffectiveDate = employer3EffectiveDate;
    entities.InterstateApLocate.Employer3EndDate = employer3EndDate;
    entities.InterstateApLocate.Employer3WageAmount = employer3WageAmount;
    entities.InterstateApLocate.Employer3WageQuarter = employer3WageQuarter;
    entities.InterstateApLocate.Employer3WageYear = employer3WageYear;
    entities.InterstateApLocate.Populated = true;
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
        entities.InterstateApIdentification.Populated = true;
      });
  }

  private bool ReadInterstateCase()
  {
    entities.InterstateCase.Populated = false;

    return Read("ReadInterstateCase",
      (db, command) =>
      {
        db.SetInt64(
          command, "transSerialNbr", import.InterstateCase.TransSerialNumber);
        db.SetDate(
          command, "transactionDate",
          import.InterstateCase.TransactionDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 0);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 1);
        entities.InterstateCase.Populated = true;
      });
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
    /// A value of InterstateApLocate.
    /// </summary>
    [JsonPropertyName("interstateApLocate")]
    public InterstateApLocate InterstateApLocate
    {
      get => interstateApLocate ??= new();
      set => interstateApLocate = value;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    private InterstateApLocate interstateApLocate;
    private InterstateCase interstateCase;
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
    /// A value of InterstateApIdentification.
    /// </summary>
    [JsonPropertyName("interstateApIdentification")]
    public InterstateApIdentification InterstateApIdentification
    {
      get => interstateApIdentification ??= new();
      set => interstateApIdentification = value;
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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    private InterstateApIdentification interstateApIdentification;
    private InterstateApLocate interstateApLocate;
    private InterstateCase interstateCase;
  }
#endregion
}
