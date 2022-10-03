// Program: OE_LIST_ALL_HEALTH_INSU_COVERAGE, ID: 371855976, model: 746.
// Short name: SWE00941
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
/// A program: OE_LIST_ALL_HEALTH_INSU_COVERAGE.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// WRITTEN BY:SID
/// </para>
/// </summary>
[Serializable]
public partial class OeListAllHealthInsuCoverage: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_LIST_ALL_HEALTH_INSU_COVERAGE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeListAllHealthInsuCoverage(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeListAllHealthInsuCoverage.
  /// </summary>
  public OeListAllHealthInsuCoverage(IContext context, Import import,
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
    // ---------------------------------------------
    // Date      Author          Reason
    // Jan 1995  Rebecca Grimes  Initial Development
    // 02/08/95  Sid             Rework
    // ---------------------------------------------
    export.Case1.Number = import.Case1.Number;
    export.CsePerson.Number = import.CsePerson.Number;
    local.WorkDataRead.Flag = "";

    // ---------------------------------------------
    // Check if any health insurance coverage exists
    // for the CSE Person.
    // ---------------------------------------------
    if (!IsEmpty(import.CsePerson.Number))
    {
      if (ReadCsePerson())
      {
        export.CsePerson.Number = entities.CsePerson.Number;
      }
      else
      {
        ExitState = "CSE_PERSON_NF";

        return;
      }

      if (ReadHealthInsuranceCoverage2())
      {
        local.WorkDataRead.Flag = "Y";
      }

      if (AsChar(local.WorkDataRead.Flag) != 'Y')
      {
        if (ReadHealthInsuranceCoverage1())
        {
          local.WorkDataRead.Flag = "Y";
        }
      }

      if (AsChar(local.WorkDataRead.Flag) != 'Y')
      {
        ExitState = "HEALTH_INSURANCE_COVERAGE_NF_RB";

        return;
      }
      else
      {
        local.WorkDataRead.Flag = "";
      }
    }

    // ---------------------------------------------
    // If the case number is left blank and the
    // person number is entered, all the insurance
    // records for a given person are displayed
    // including all cases he is on with insurance
    // coverage identified.  The sort sequence is
    // policy holder and active flag ("Y" for active
    // or "N" for not-active).
    // ---------------------------------------------
    if (IsEmpty(import.Case1.Number) && !IsEmpty(import.CsePerson.Number))
    {
      if (ReadCsePerson())
      {
        export.CsePerson.Number = entities.CsePerson.Number;
      }
      else
      {
        ExitState = "OE0000_CASE_MEMBER_NE";

        return;
      }

      if (ReadHealthInsuranceCoverage2())
      {
        // ---------------------------------------------
        // The entered CSE Person is a Policy holder.
        // Set flag to "Y".
        // ---------------------------------------------
        local.WorkDataRead.Flag = "Y";
      }

      if (AsChar(local.WorkDataRead.Flag) == 'Y')
      {
        export.Export1.Index = 0;
        export.Export1.Clear();

        foreach(var item in ReadHealthInsuranceCompanyHealthInsuranceCoverage2())
          
        {
          export.Export1.Update.DetCase.Number = entities.Case1.Number;
          export.Export1.Update.DetHealthInsuranceCompany.Assign(
            entities.HealthInsuranceCompany);
          export.Export1.Update.DetPolicyHolder.Number =
            entities.CsePerson.Number;
          export.Export1.Update.DetHealthInsuranceCoverage.Assign(
            entities.HealthInsuranceCoverage);
          export.Export1.Update.DetCovered.Number = entities.Covered.Number;

          if (!Lt(Now().Date, entities.PersonalHealthInsurance.CoverageBeginDate)
            && !
            Lt(entities.PersonalHealthInsurance.CoverageEndDate, Now().Date))
          {
            export.Export1.Update.DetInsurancActive.Flag = "Y";
          }
          else
          {
            export.Export1.Update.DetInsurancActive.Flag = "N";
          }

          export.Export1.Next();
        }

        export.Export1.Index = export.Export1.Count;
        export.Export1.CheckIndex();

        foreach(var item in ReadHealthInsuranceCoverageHealthInsuranceCompany())
        {
          export.Export1.Update.DetHealthInsuranceCompany.Assign(
            entities.HealthInsuranceCompany);
          export.Export1.Update.DetPolicyHolder.Number =
            entities.CsePerson.Number;
          export.Export1.Update.DetHealthInsuranceCoverage.Assign(
            entities.HealthInsuranceCoverage);
          export.Export1.Next();
        }
      }

      // ---------------------------------------------
      // The entered CSE Person is not a Policy
      // holder. Check if he is a covered person.
      // ---------------------------------------------
      export.Export1.Index = export.Export1.Count;
      export.Export1.CheckIndex();

      foreach(var item in ReadHealthInsuranceCompanyHealthInsuranceCoverage4())
      {
        export.Export1.Update.DetCase.Number = entities.Case1.Number;
        export.Export1.Update.DetHealthInsuranceCompany.Assign(
          entities.HealthInsuranceCompany);
        export.Export1.Update.DetHealthInsuranceCoverage.Assign(
          entities.HealthInsuranceCoverage);
        export.Export1.Update.DetCovered.Number = entities.CsePerson.Number;
        export.Export1.Update.DetPolicyHolder.Number =
          entities.PolicyHolderCsePerson.Number;

        if (!Lt(Now().Date, entities.PersonalHealthInsurance.CoverageBeginDate) &&
          !Lt(entities.PersonalHealthInsurance.CoverageEndDate, Now().Date))
        {
          export.Export1.Update.DetInsurancActive.Flag = "Y";
        }
        else
        {
          export.Export1.Update.DetInsurancActive.Flag = "N";
        }

        export.Export1.Next();
      }

      return;
    }

    // ---------------------------------------------
    // If the case number and person number are
    // entered, only the insurance records relating
    // to that person and case number are displayed.
    // ---------------------------------------------
    if (!IsEmpty(import.Case1.Number) && !IsEmpty(import.CsePerson.Number))
    {
      if (ReadCsePersonCase())
      {
        export.CsePerson.Number = entities.CsePerson.Number;
        export.Case1.Number = entities.Case1.Number;
      }
      else
      {
        ExitState = "OE0000_CASE_MEMBER_NE";

        return;
      }

      if (ReadHealthInsuranceCoverage2())
      {
        // ---------------------------------------------
        // The entered CSE Person is a Policy holder.
        // Set flag to "Y".
        // ---------------------------------------------
        local.WorkDataRead.Flag = "Y";
      }

      if (AsChar(local.WorkDataRead.Flag) == 'Y')
      {
        export.Export1.Index = 0;
        export.Export1.Clear();

        foreach(var item in ReadHealthInsuranceCompanyHealthInsuranceCoverage1())
          
        {
          export.Export1.Update.DetCase.Number = entities.Case1.Number;
          export.Export1.Update.DetHealthInsuranceCompany.Assign(
            entities.HealthInsuranceCompany);
          export.Export1.Update.DetPolicyHolder.Number =
            entities.CsePerson.Number;
          export.Export1.Update.DetHealthInsuranceCoverage.Assign(
            entities.HealthInsuranceCoverage);
          export.Export1.Update.DetCovered.Number = entities.Covered.Number;

          if (!Lt(Now().Date, entities.PersonalHealthInsurance.CoverageBeginDate)
            && !
            Lt(entities.PersonalHealthInsurance.CoverageEndDate, Now().Date))
          {
            export.Export1.Update.DetInsurancActive.Flag = "Y";
          }
          else
          {
            export.Export1.Update.DetInsurancActive.Flag = "N";
          }

          export.Export1.Next();
        }

        export.Export1.Index = export.Export1.Count;
        export.Export1.CheckIndex();

        foreach(var item in ReadHealthInsuranceCoverageHealthInsuranceCompany())
        {
          export.Export1.Update.DetHealthInsuranceCompany.Assign(
            entities.HealthInsuranceCompany);
          export.Export1.Update.DetPolicyHolder.Number =
            entities.CsePerson.Number;
          export.Export1.Update.DetHealthInsuranceCoverage.Assign(
            entities.HealthInsuranceCoverage);
          export.Export1.Next();
        }
      }

      // ---------------------------------------------
      // The entered CSE Person is not a Policy
      // holder. Check if he is a covered person.
      // ---------------------------------------------
      export.Export1.Index = export.Export1.Count;
      export.Export1.CheckIndex();

      foreach(var item in ReadHealthInsuranceCompanyHealthInsuranceCoverage3())
      {
        export.Export1.Update.DetCase.Number = entities.Case1.Number;
        export.Export1.Update.DetHealthInsuranceCompany.Assign(
          entities.HealthInsuranceCompany);
        export.Export1.Update.DetHealthInsuranceCoverage.Assign(
          entities.HealthInsuranceCoverage);
        export.Export1.Update.DetCovered.Number = entities.CsePerson.Number;
        export.Export1.Update.DetPolicyHolder.Number =
          entities.PolicyHolderCsePerson.Number;

        if (!Lt(Now().Date, entities.PersonalHealthInsurance.CoverageBeginDate) &&
          !Lt(entities.PersonalHealthInsurance.CoverageEndDate, Now().Date))
        {
          export.Export1.Update.DetInsurancActive.Flag = "Y";
        }
        else
        {
          export.Export1.Update.DetInsurancActive.Flag = "N";
        }

        export.Export1.Next();
      }

      return;
    }

    // --------------------------------------------
    // If the case number is entered and the person
    // number is left blank, all the people covered
    // by an insurance policy are displayed for a
    // given case number.
    // ---------------------------------------------
    if (!IsEmpty(import.Case1.Number) && IsEmpty(import.CsePerson.Number))
    {
      if (ReadCase())
      {
        export.Case1.Number = entities.Case1.Number;
      }
      else
      {
        ExitState = "OE0000_CASE_MEMBER_NE";

        return;
      }

      // ---------------------------------------------
      // Read each case member for the CURRENT case
      // and list the insurance coverage for them.
      // ---------------------------------------------
      export.Export1.Index = 0;
      export.Export1.Clear();

      foreach(var item in ReadCsePersonPersonalHealthInsuranceHealthInsuranceCoverage())
        
      {
        export.Export1.Update.DetCase.Number = entities.Case1.Number;
        export.Export1.Update.DetHealthInsuranceCompany.Assign(
          entities.HealthInsuranceCompany);
        export.Export1.Update.DetHealthInsuranceCoverage.Assign(
          entities.HealthInsuranceCoverage);
        export.Export1.Update.DetCovered.Number = entities.Covered.Number;
        export.Export1.Update.DetPolicyHolder.Number =
          entities.PolicyHolderCsePerson.Number;

        if (!Lt(Now().Date, entities.PersonalHealthInsurance.CoverageBeginDate) &&
          !Lt(entities.PersonalHealthInsurance.CoverageEndDate, Now().Date))
        {
          export.Export1.Update.DetInsurancActive.Flag = "Y";
        }
        else
        {
          export.Export1.Update.DetInsurancActive.Flag = "N";
        }

        export.Export1.Next();
      }
    }
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
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

  private bool ReadCsePersonCase()
  {
    entities.CsePerson.Populated = false;
    entities.Case1.Populated = false;

    return Read("ReadCsePersonCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
        db.SetString(command, "casNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.CsePerson.Populated = true;
        entities.Case1.Populated = true;
      });
  }

  private IEnumerable<bool>
    ReadCsePersonPersonalHealthInsuranceHealthInsuranceCoverage()
  {
    return ReadEach(
      "ReadCsePersonPersonalHealthInsuranceHealthInsuranceCoverage",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.Covered.Number = db.GetString(reader, 0);
        entities.PersonalHealthInsurance.HcvId = db.GetInt64(reader, 1);
        entities.HealthInsuranceCoverage.Identifier = db.GetInt64(reader, 1);
        entities.PersonalHealthInsurance.CspNumber = db.GetString(reader, 2);
        entities.PersonalHealthInsurance.CoverageBeginDate =
          db.GetNullableDate(reader, 3);
        entities.PersonalHealthInsurance.CoverageEndDate =
          db.GetNullableDate(reader, 4);
        entities.HealthInsuranceCoverage.InsuranceGroupNumber =
          db.GetNullableString(reader, 5);
        entities.HealthInsuranceCoverage.InsurancePolicyNumber =
          db.GetNullableString(reader, 6);
        entities.HealthInsuranceCoverage.PolicyExpirationDate =
          db.GetNullableDate(reader, 7);
        entities.HealthInsuranceCoverage.PolicyEffectiveDate =
          db.GetNullableDate(reader, 8);
        entities.HealthInsuranceCoverage.CspNumber =
          db.GetNullableString(reader, 9);
        entities.PolicyHolderCsePerson.Number = db.GetString(reader, 9);
        entities.HealthInsuranceCoverage.HicIdentifier =
          db.GetNullableInt32(reader, 10);
        entities.HealthInsuranceCompany.Identifier = db.GetInt32(reader, 10);
        entities.HealthInsuranceCompany.CarrierCode =
          db.GetNullableString(reader, 11);
        entities.HealthInsuranceCompany.InsurancePolicyCarrier =
          db.GetNullableString(reader, 12);
        entities.PersonalHealthInsurance.Populated = true;
        entities.PolicyHolderCsePerson.Populated = true;
        entities.HealthInsuranceCompany.Populated = true;
        entities.HealthInsuranceCoverage.Populated = true;
        entities.Covered.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadHealthInsuranceCompanyHealthInsuranceCoverage1()
  {
    return ReadEach("ReadHealthInsuranceCompanyHealthInsuranceCoverage1",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", entities.CsePerson.Number);
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.HealthInsuranceCompany.Identifier = db.GetInt32(reader, 0);
        entities.HealthInsuranceCoverage.HicIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.HealthInsuranceCompany.CarrierCode =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCompany.InsurancePolicyCarrier =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCoverage.Identifier = db.GetInt64(reader, 3);
        entities.PersonalHealthInsurance.HcvId = db.GetInt64(reader, 3);
        entities.HealthInsuranceCoverage.InsuranceGroupNumber =
          db.GetNullableString(reader, 4);
        entities.HealthInsuranceCoverage.InsurancePolicyNumber =
          db.GetNullableString(reader, 5);
        entities.HealthInsuranceCoverage.PolicyExpirationDate =
          db.GetNullableDate(reader, 6);
        entities.HealthInsuranceCoverage.PolicyEffectiveDate =
          db.GetNullableDate(reader, 7);
        entities.HealthInsuranceCoverage.CspNumber =
          db.GetNullableString(reader, 8);
        entities.PersonalHealthInsurance.CspNumber = db.GetString(reader, 9);
        entities.Covered.Number = db.GetString(reader, 9);
        entities.PersonalHealthInsurance.CoverageBeginDate =
          db.GetNullableDate(reader, 10);
        entities.PersonalHealthInsurance.CoverageEndDate =
          db.GetNullableDate(reader, 11);
        entities.PersonalHealthInsurance.Populated = true;
        entities.HealthInsuranceCompany.Populated = true;
        entities.HealthInsuranceCoverage.Populated = true;
        entities.Covered.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadHealthInsuranceCompanyHealthInsuranceCoverage2()
  {
    return ReadEach("ReadHealthInsuranceCompanyHealthInsuranceCoverage2",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.HealthInsuranceCompany.Identifier = db.GetInt32(reader, 0);
        entities.HealthInsuranceCoverage.HicIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.HealthInsuranceCompany.CarrierCode =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCompany.InsurancePolicyCarrier =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCoverage.Identifier = db.GetInt64(reader, 3);
        entities.PersonalHealthInsurance.HcvId = db.GetInt64(reader, 3);
        entities.HealthInsuranceCoverage.InsuranceGroupNumber =
          db.GetNullableString(reader, 4);
        entities.HealthInsuranceCoverage.InsurancePolicyNumber =
          db.GetNullableString(reader, 5);
        entities.HealthInsuranceCoverage.PolicyExpirationDate =
          db.GetNullableDate(reader, 6);
        entities.HealthInsuranceCoverage.PolicyEffectiveDate =
          db.GetNullableDate(reader, 7);
        entities.HealthInsuranceCoverage.CspNumber =
          db.GetNullableString(reader, 8);
        entities.PersonalHealthInsurance.CspNumber = db.GetString(reader, 9);
        entities.Covered.Number = db.GetString(reader, 9);
        entities.PersonalHealthInsurance.CoverageBeginDate =
          db.GetNullableDate(reader, 10);
        entities.PersonalHealthInsurance.CoverageEndDate =
          db.GetNullableDate(reader, 11);
        entities.Case1.Number = db.GetString(reader, 12);
        entities.PersonalHealthInsurance.Populated = true;
        entities.HealthInsuranceCompany.Populated = true;
        entities.HealthInsuranceCoverage.Populated = true;
        entities.Case1.Populated = true;
        entities.Covered.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadHealthInsuranceCompanyHealthInsuranceCoverage3()
  {
    return ReadEach("ReadHealthInsuranceCompanyHealthInsuranceCoverage3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.HealthInsuranceCompany.Identifier = db.GetInt32(reader, 0);
        entities.HealthInsuranceCoverage.HicIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.HealthInsuranceCompany.CarrierCode =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCompany.InsurancePolicyCarrier =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCoverage.Identifier = db.GetInt64(reader, 3);
        entities.PersonalHealthInsurance.HcvId = db.GetInt64(reader, 3);
        entities.HealthInsuranceCoverage.InsuranceGroupNumber =
          db.GetNullableString(reader, 4);
        entities.HealthInsuranceCoverage.InsurancePolicyNumber =
          db.GetNullableString(reader, 5);
        entities.HealthInsuranceCoverage.PolicyExpirationDate =
          db.GetNullableDate(reader, 6);
        entities.HealthInsuranceCoverage.PolicyEffectiveDate =
          db.GetNullableDate(reader, 7);
        entities.HealthInsuranceCoverage.CspNumber =
          db.GetNullableString(reader, 8);
        entities.PolicyHolderCsePerson.Number = db.GetString(reader, 8);
        entities.PersonalHealthInsurance.CspNumber = db.GetString(reader, 9);
        entities.PersonalHealthInsurance.CoverageBeginDate =
          db.GetNullableDate(reader, 10);
        entities.PersonalHealthInsurance.CoverageEndDate =
          db.GetNullableDate(reader, 11);
        entities.PersonalHealthInsurance.Populated = true;
        entities.PolicyHolderCsePerson.Populated = true;
        entities.HealthInsuranceCompany.Populated = true;
        entities.HealthInsuranceCoverage.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadHealthInsuranceCompanyHealthInsuranceCoverage4()
  {
    return ReadEach("ReadHealthInsuranceCompanyHealthInsuranceCoverage4",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.HealthInsuranceCompany.Identifier = db.GetInt32(reader, 0);
        entities.HealthInsuranceCoverage.HicIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.HealthInsuranceCompany.CarrierCode =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCompany.InsurancePolicyCarrier =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCoverage.Identifier = db.GetInt64(reader, 3);
        entities.PersonalHealthInsurance.HcvId = db.GetInt64(reader, 3);
        entities.HealthInsuranceCoverage.InsuranceGroupNumber =
          db.GetNullableString(reader, 4);
        entities.HealthInsuranceCoverage.InsurancePolicyNumber =
          db.GetNullableString(reader, 5);
        entities.HealthInsuranceCoverage.PolicyExpirationDate =
          db.GetNullableDate(reader, 6);
        entities.HealthInsuranceCoverage.PolicyEffectiveDate =
          db.GetNullableDate(reader, 7);
        entities.HealthInsuranceCoverage.CspNumber =
          db.GetNullableString(reader, 8);
        entities.PolicyHolderCsePerson.Number = db.GetString(reader, 8);
        entities.PersonalHealthInsurance.CspNumber = db.GetString(reader, 9);
        entities.PersonalHealthInsurance.CoverageBeginDate =
          db.GetNullableDate(reader, 10);
        entities.PersonalHealthInsurance.CoverageEndDate =
          db.GetNullableDate(reader, 11);
        entities.Case1.Number = db.GetString(reader, 12);
        entities.PersonalHealthInsurance.Populated = true;
        entities.PolicyHolderCsePerson.Populated = true;
        entities.HealthInsuranceCompany.Populated = true;
        entities.HealthInsuranceCoverage.Populated = true;
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadHealthInsuranceCoverage1()
  {
    entities.HealthInsuranceCoverage.Populated = false;

    return Read("ReadHealthInsuranceCoverage1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.HealthInsuranceCoverage.Identifier = db.GetInt64(reader, 0);
        entities.HealthInsuranceCoverage.InsuranceGroupNumber =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCoverage.InsurancePolicyNumber =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCoverage.PolicyExpirationDate =
          db.GetNullableDate(reader, 3);
        entities.HealthInsuranceCoverage.PolicyEffectiveDate =
          db.GetNullableDate(reader, 4);
        entities.HealthInsuranceCoverage.CspNumber =
          db.GetNullableString(reader, 5);
        entities.HealthInsuranceCoverage.HicIdentifier =
          db.GetNullableInt32(reader, 6);
        entities.HealthInsuranceCoverage.Populated = true;
      });
  }

  private bool ReadHealthInsuranceCoverage2()
  {
    entities.HealthInsuranceCoverage.Populated = false;

    return Read("ReadHealthInsuranceCoverage2",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.HealthInsuranceCoverage.Identifier = db.GetInt64(reader, 0);
        entities.HealthInsuranceCoverage.InsuranceGroupNumber =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCoverage.InsurancePolicyNumber =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCoverage.PolicyExpirationDate =
          db.GetNullableDate(reader, 3);
        entities.HealthInsuranceCoverage.PolicyEffectiveDate =
          db.GetNullableDate(reader, 4);
        entities.HealthInsuranceCoverage.CspNumber =
          db.GetNullableString(reader, 5);
        entities.HealthInsuranceCoverage.HicIdentifier =
          db.GetNullableInt32(reader, 6);
        entities.HealthInsuranceCoverage.Populated = true;
      });
  }

  private IEnumerable<bool> ReadHealthInsuranceCoverageHealthInsuranceCompany()
  {
    return ReadEach("ReadHealthInsuranceCoverageHealthInsuranceCompany",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.HealthInsuranceCoverage.Identifier = db.GetInt64(reader, 0);
        entities.HealthInsuranceCoverage.InsuranceGroupNumber =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCoverage.InsurancePolicyNumber =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCoverage.PolicyExpirationDate =
          db.GetNullableDate(reader, 3);
        entities.HealthInsuranceCoverage.PolicyEffectiveDate =
          db.GetNullableDate(reader, 4);
        entities.HealthInsuranceCoverage.CspNumber =
          db.GetNullableString(reader, 5);
        entities.HealthInsuranceCoverage.HicIdentifier =
          db.GetNullableInt32(reader, 6);
        entities.HealthInsuranceCompany.Identifier = db.GetInt32(reader, 6);
        entities.HealthInsuranceCompany.CarrierCode =
          db.GetNullableString(reader, 7);
        entities.HealthInsuranceCompany.InsurancePolicyCarrier =
          db.GetNullableString(reader, 8);
        entities.HealthInsuranceCompany.Populated = true;
        entities.HealthInsuranceCoverage.Populated = true;

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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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

    private Case1 case1;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of DetHealthInsuranceCompany.
      /// </summary>
      [JsonPropertyName("detHealthInsuranceCompany")]
      public HealthInsuranceCompany DetHealthInsuranceCompany
      {
        get => detHealthInsuranceCompany ??= new();
        set => detHealthInsuranceCompany = value;
      }

      /// <summary>
      /// A value of DetWork.
      /// </summary>
      [JsonPropertyName("detWork")]
      public Common DetWork
      {
        get => detWork ??= new();
        set => detWork = value;
      }

      /// <summary>
      /// A value of DetHealthInsuranceCoverage.
      /// </summary>
      [JsonPropertyName("detHealthInsuranceCoverage")]
      public HealthInsuranceCoverage DetHealthInsuranceCoverage
      {
        get => detHealthInsuranceCoverage ??= new();
        set => detHealthInsuranceCoverage = value;
      }

      /// <summary>
      /// A value of DetPolicyHolder.
      /// </summary>
      [JsonPropertyName("detPolicyHolder")]
      public CsePerson DetPolicyHolder
      {
        get => detPolicyHolder ??= new();
        set => detPolicyHolder = value;
      }

      /// <summary>
      /// A value of PolicyHolderName.
      /// </summary>
      [JsonPropertyName("policyHolderName")]
      public OeWorkGroup PolicyHolderName
      {
        get => policyHolderName ??= new();
        set => policyHolderName = value;
      }

      /// <summary>
      /// A value of DetInsurancActive.
      /// </summary>
      [JsonPropertyName("detInsurancActive")]
      public Common DetInsurancActive
      {
        get => detInsurancActive ??= new();
        set => detInsurancActive = value;
      }

      /// <summary>
      /// A value of DetCase.
      /// </summary>
      [JsonPropertyName("detCase")]
      public Case1 DetCase
      {
        get => detCase ??= new();
        set => detCase = value;
      }

      /// <summary>
      /// A value of DetCovered.
      /// </summary>
      [JsonPropertyName("detCovered")]
      public CsePerson DetCovered
      {
        get => detCovered ??= new();
        set => detCovered = value;
      }

      /// <summary>
      /// A value of CoveredPersonName.
      /// </summary>
      [JsonPropertyName("coveredPersonName")]
      public OeWorkGroup CoveredPersonName
      {
        get => coveredPersonName ??= new();
        set => coveredPersonName = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 30;

      private HealthInsuranceCompany detHealthInsuranceCompany;
      private Common detWork;
      private HealthInsuranceCoverage detHealthInsuranceCoverage;
      private CsePerson detPolicyHolder;
      private OeWorkGroup policyHolderName;
      private Common detInsurancActive;
      private Case1 detCase;
      private CsePerson detCovered;
      private OeWorkGroup coveredPersonName;
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
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 => export1 ??= new(ExportGroup.Capacity);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    private Case1 case1;
    private CsePerson csePerson;
    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of WorkDataRead.
    /// </summary>
    [JsonPropertyName("workDataRead")]
    public Common WorkDataRead
    {
      get => workDataRead ??= new();
      set => workDataRead = value;
    }

    private Common workDataRead;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of PolicyHolderCaseRole.
    /// </summary>
    [JsonPropertyName("policyHolderCaseRole")]
    public CaseRole PolicyHolderCaseRole
    {
      get => policyHolderCaseRole ??= new();
      set => policyHolderCaseRole = value;
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
    /// A value of PersonalHealthInsurance.
    /// </summary>
    [JsonPropertyName("personalHealthInsurance")]
    public PersonalHealthInsurance PersonalHealthInsurance
    {
      get => personalHealthInsurance ??= new();
      set => personalHealthInsurance = value;
    }

    /// <summary>
    /// A value of PolicyHolderCsePerson.
    /// </summary>
    [JsonPropertyName("policyHolderCsePerson")]
    public CsePerson PolicyHolderCsePerson
    {
      get => policyHolderCsePerson ??= new();
      set => policyHolderCsePerson = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompany")]
    public HealthInsuranceCompany HealthInsuranceCompany
    {
      get => healthInsuranceCompany ??= new();
      set => healthInsuranceCompany = value;
    }

    /// <summary>
    /// A value of CoveredPerson.
    /// </summary>
    [JsonPropertyName("coveredPerson")]
    public CaseRole CoveredPerson
    {
      get => coveredPerson ??= new();
      set => coveredPerson = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("healthInsuranceCoverage")]
    public HealthInsuranceCoverage HealthInsuranceCoverage
    {
      get => healthInsuranceCoverage ??= new();
      set => healthInsuranceCoverage = value;
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

    /// <summary>
    /// A value of Covered.
    /// </summary>
    [JsonPropertyName("covered")]
    public CsePerson Covered
    {
      get => covered ??= new();
      set => covered = value;
    }

    private CaseRole caseRole;
    private CaseRole policyHolderCaseRole;
    private CsePerson csePerson;
    private PersonalHealthInsurance personalHealthInsurance;
    private CsePerson policyHolderCsePerson;
    private HealthInsuranceCompany healthInsuranceCompany;
    private CaseRole coveredPerson;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private Case1 case1;
    private CsePerson covered;
  }
#endregion
}
