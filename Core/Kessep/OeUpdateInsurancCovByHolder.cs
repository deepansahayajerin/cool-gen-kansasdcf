// Program: OE_UPDATE_INSURANC_COV_BY_HOLDER, ID: 371853939, model: 746.
// Short name: SWE00970
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
/// A program: OE_UPDATE_INSURANC_COV_BY_HOLDER.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// </para>
/// </summary>
[Serializable]
public partial class OeUpdateInsurancCovByHolder: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_UPDATE_INSURANC_COV_BY_HOLDER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeUpdateInsurancCovByHolder(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeUpdateInsurancCovByHolder.
  /// </summary>
  public OeUpdateInsurancCovByHolder(IContext context, Import import,
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
    // Date		Author		Change #	Reason
    // Jan 1995	Rebecca Grimes			Initial Development
    // 02/11/95	Sid				Rework
    // 01/09/2009	JHuss		CQ 7786		Send INSUINFO letter when policy is end-dated.
    // ---------------------------------------------
    MoveHealthInsuranceCoverage(import.HealthInsuranceCoverage,
      export.HealthInsuranceCoverage);
    export.Contact.Assign(import.Contact);
    export.CsePerson.Number = import.CsePerson.Number;
    local.Current.Date = Now().Date;

    // ---------------------------------------------
    //       Read the CSE Person Contact .
    // ---------------------------------------------
    ReadCsePerson1();

    if (import.Contact.ContactNumber != 0)
    {
      if (ReadContact())
      {
        export.HealthInsuranceCoverage.PolicyPaidByCsePersonInd = "N";
        export.Contact.Assign(entities.Contact);
      }
    }

    if (Equal(export.HealthInsuranceCoverage.VerifiedDate, null))
    {
      export.HealthInsuranceCoverage.VerifiedUserId = "";
    }
    else
    {
      export.HealthInsuranceCoverage.VerifiedUserId = global.UserId;
    }

    // ---------------------------------------------
    // Read the health insurance coverage record
    // associated to the CSE Person on a particular
    // case by its identifier in order to update the
    // record with the most current information.
    // ---------------------------------------------
    if (ReadHealthInsuranceCoverage1())
    {
      // ---------------------------------------------
      // Update the health insurance coverage record.
      // ---------------------------------------------
      if (Equal(entities.HealthInsuranceCoverage.InsuranceGroupNumber,
        import.HealthInsuranceCoverage.InsuranceGroupNumber) && Equal
        (entities.HealthInsuranceCoverage.InsurancePolicyNumber,
        import.HealthInsuranceCoverage.InsurancePolicyNumber))
      {
      }
      else
      {
        // ****************************************************************
        // Check to ensure that is not a duplicate record for combination of 
        // CSE_PERSON,CARRIER_CODE,POLICY #, AND GROUP ID.  These fields are not
        // IDENTIFIERS in the database.
        // ****************************************************************
        if (ReadHealthInsuranceCoverage2())
        {
          ExitState = "HEALTH_INSURANCE_COVERAGE_AE_RB";

          return;
        }
      }

      try
      {
        UpdateHealthInsuranceCoverage();
        export.HealthInsuranceCoverage.Assign(entities.HealthInsuranceCoverage);

        if (entities.Contact.Populated)
        {
          AssociateHealthInsuranceCoverage1();
        }

        if (!Equal(import.IncomeSource.Identifier, local.Blank.Identifier))
        {
          if (AsChar(import.DissasociateIncomeSourc.Flag) == 'Y')
          {
            if (ReadIncomeSource2())
            {
              DisassociateIncomeSource();
            }
            else
            {
              ExitState = "INCOME_SOURCE_NF";
            }
          }
          else if (ReadIncomeSource1())
          {
            AssociateHealthInsuranceCoverage2();
          }
          else
          {
            ExitState = "INCOME_SOURCE_NF";
          }
        }

        if (AsChar(import.EndDateCoverage.Flag) == 'Y')
        {
          foreach(var item in ReadPersonalHealthInsurance())
          {
            try
            {
              UpdatePersonalHealthInsurance();
            }
            catch(Exception e1)
            {
              switch(GetErrorCode(e1))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "PERSONAL_HEALTH_INSURANCE_NU_RB";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "PERSONAL_HEALTH_INSURANCE_PV_RB";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }

            // ********************************************************************************************
            // 1/09/2009	JHuss	Send INSUINFO letter when policy is end-dated.
            // Find each open case that the child is active on.  Then find the 
            // active AR for those cases
            // that are not organizations and are not the policy holders.  Send 
            // the INSUINFO letters to those ARs.
            // As per CSE business, all "active" determinations are based on the
            // current date.
            // ********************************************************************************************
            foreach(var item1 in ReadCaseCsePerson())
            {
              if (ReadCsePerson2())
              {
                local.Document.Name = "INSUINFO";
                local.SpDocKey.KeyCase = entities.Case1.Number;
                local.SpDocKey.KeyHealthInsCoverage =
                  import.HealthInsuranceCoverage.Identifier;
                local.SpDocKey.KeyChild = entities.ChildCsePerson.Number;
                local.FindDoc.Flag = "Y";
                UseSpCabDetermineInterstateDoc();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }
              }
            }
          }
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "HEALTH_INSURANCE_COVERAGE_NU_RB";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "HEALTH_INSURANCE_COVERAGE_PV_RB";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      // ---------------------------------------------
      // If the policy is held by a third party then
      // read and update corresponding CONTACT record
      // ---------------------------------------------
    }
    else
    {
      ExitState = "HEALTH_INSURANCE_COVERAGE_NF_RB";
    }
  }

  private static void MoveHealthInsuranceCoverage(
    HealthInsuranceCoverage source, HealthInsuranceCoverage target)
  {
    target.Identifier = source.Identifier;
    target.PolicyPaidByCsePersonInd = source.PolicyPaidByCsePersonInd;
    target.InsuranceGroupNumber = source.InsuranceGroupNumber;
    target.VerifiedDate = source.VerifiedDate;
    target.InsurancePolicyNumber = source.InsurancePolicyNumber;
    target.PolicyExpirationDate = source.PolicyExpirationDate;
    target.CoverageCode1 = source.CoverageCode1;
    target.CoverageCode2 = source.CoverageCode2;
    target.CoverageCode3 = source.CoverageCode3;
    target.CoverageCode4 = source.CoverageCode4;
    target.CoverageCode5 = source.CoverageCode5;
    target.CoverageCode6 = source.CoverageCode6;
    target.CoverageCode7 = source.CoverageCode7;
    target.PolicyEffectiveDate = source.PolicyEffectiveDate;
    target.OtherCoveredPersons = source.OtherCoveredPersons;
  }

  private static void MoveSpDocKey(SpDocKey source, SpDocKey target)
  {
    target.KeyCase = source.KeyCase;
    target.KeyChild = source.KeyChild;
    target.KeyHealthInsCoverage = source.KeyHealthInsCoverage;
  }

  private void UseSpCabDetermineInterstateDoc()
  {
    var useImport = new SpCabDetermineInterstateDoc.Import();
    var useExport = new SpCabDetermineInterstateDoc.Export();

    useImport.Document.Name = local.Document.Name;
    useImport.FindDoc.Flag = local.FindDoc.Flag;
    MoveSpDocKey(local.SpDocKey, useImport.SpDocKey);

    Call(SpCabDetermineInterstateDoc.Execute, useImport, useExport);
  }

  private void AssociateHealthInsuranceCoverage1()
  {
    System.Diagnostics.Debug.Assert(entities.Contact.Populated);

    var cspHNumber = entities.Contact.CspNumber;
    var conHNumber = entities.Contact.ContactNumber;

    entities.HealthInsuranceCoverage.Populated = false;
    Update("AssociateHealthInsuranceCoverage1",
      (db, command) =>
      {
        db.SetNullableString(command, "cspHNumber", cspHNumber);
        db.SetNullableInt32(command, "conHNumber", conHNumber);
        db.SetInt64(
          command, "identifier", entities.HealthInsuranceCoverage.Identifier);
      });

    entities.HealthInsuranceCoverage.CspHNumber = cspHNumber;
    entities.HealthInsuranceCoverage.ConHNumber = conHNumber;
    entities.HealthInsuranceCoverage.Populated = true;
  }

  private void AssociateHealthInsuranceCoverage2()
  {
    System.Diagnostics.Debug.Assert(entities.IncomeSource.Populated);

    var cseNumber = entities.IncomeSource.CspINumber;
    var isrIdentifier = entities.IncomeSource.Identifier;

    entities.HealthInsuranceCoverage.Populated = false;
    Update("AssociateHealthInsuranceCoverage2",
      (db, command) =>
      {
        db.SetNullableString(command, "cseNumber", cseNumber);
        db.SetNullableDateTime(command, "isrIdentifier", isrIdentifier);
        db.SetInt64(
          command, "identifier", entities.HealthInsuranceCoverage.Identifier);
      });

    entities.HealthInsuranceCoverage.CseNumber = cseNumber;
    entities.HealthInsuranceCoverage.IsrIdentifier = isrIdentifier;
    entities.HealthInsuranceCoverage.Populated = true;
  }

  private void DisassociateIncomeSource()
  {
    entities.HealthInsuranceCoverage.Populated = false;
    Update("DisassociateIncomeSource",
      (db, command) =>
      {
        db.SetInt64(
          command, "identifier", entities.HealthInsuranceCoverage.Identifier);
      });

    entities.HealthInsuranceCoverage.CseNumber = null;
    entities.HealthInsuranceCoverage.IsrIdentifier = null;
    entities.HealthInsuranceCoverage.Populated = true;
  }

  private IEnumerable<bool> ReadCaseCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.PersonalHealthInsurance.Populated);
    entities.ChildCsePerson.Populated = false;
    entities.Case1.Populated = false;

    return ReadEach("ReadCaseCsePerson",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", entities.PersonalHealthInsurance.CspNumber);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.ChildCsePerson.Number = db.GetString(reader, 2);
        entities.ChildCsePerson.Populated = true;
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadContact()
  {
    entities.Contact.Populated = false;

    return Read("ReadContact",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetInt32(command, "contactNumber", import.Contact.ContactNumber);
      },
      (db, reader) =>
      {
        entities.Contact.CspNumber = db.GetString(reader, 0);
        entities.Contact.ContactNumber = db.GetInt32(reader, 1);
        entities.Contact.RelationshipToCsePerson =
          db.GetNullableString(reader, 2);
        entities.Contact.NameLast = db.GetNullableString(reader, 3);
        entities.Contact.NameFirst = db.GetNullableString(reader, 4);
        entities.Contact.MiddleInitial = db.GetNullableString(reader, 5);
        entities.Contact.Populated = true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
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

  private bool ReadCsePerson2()
  {
    entities.ArCsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ArCsePerson.Number = db.GetString(reader, 0);
        entities.ArCsePerson.Type1 = db.GetString(reader, 1);
        entities.ArCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ArCsePerson.Type1);
      });
  }

  private bool ReadHealthInsuranceCoverage1()
  {
    entities.HealthInsuranceCoverage.Populated = false;

    return Read("ReadHealthInsuranceCoverage1",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", entities.CsePerson.Number);
        db.SetInt64(
          command, "identifier", import.HealthInsuranceCoverage.Identifier);
      },
      (db, reader) =>
      {
        entities.HealthInsuranceCoverage.Identifier = db.GetInt64(reader, 0);
        entities.HealthInsuranceCoverage.PolicyPaidByCsePersonInd =
          db.GetString(reader, 1);
        entities.HealthInsuranceCoverage.InsuranceGroupNumber =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCoverage.VerifiedDate =
          db.GetNullableDate(reader, 3);
        entities.HealthInsuranceCoverage.VerifiedUserId =
          db.GetNullableString(reader, 4);
        entities.HealthInsuranceCoverage.InsurancePolicyNumber =
          db.GetNullableString(reader, 5);
        entities.HealthInsuranceCoverage.PolicyExpirationDate =
          db.GetNullableDate(reader, 6);
        entities.HealthInsuranceCoverage.CoverageCode1 =
          db.GetNullableString(reader, 7);
        entities.HealthInsuranceCoverage.CoverageCode2 =
          db.GetNullableString(reader, 8);
        entities.HealthInsuranceCoverage.CoverageCode3 =
          db.GetNullableString(reader, 9);
        entities.HealthInsuranceCoverage.CoverageCode4 =
          db.GetNullableString(reader, 10);
        entities.HealthInsuranceCoverage.CoverageCode5 =
          db.GetNullableString(reader, 11);
        entities.HealthInsuranceCoverage.CoverageCode6 =
          db.GetNullableString(reader, 12);
        entities.HealthInsuranceCoverage.CoverageCode7 =
          db.GetNullableString(reader, 13);
        entities.HealthInsuranceCoverage.PolicyEffectiveDate =
          db.GetNullableDate(reader, 14);
        entities.HealthInsuranceCoverage.CreatedBy = db.GetString(reader, 15);
        entities.HealthInsuranceCoverage.CreatedTimestamp =
          db.GetDateTime(reader, 16);
        entities.HealthInsuranceCoverage.LastUpdatedBy =
          db.GetString(reader, 17);
        entities.HealthInsuranceCoverage.LastUpdatedTimestamp =
          db.GetDateTime(reader, 18);
        entities.HealthInsuranceCoverage.CspHNumber =
          db.GetNullableString(reader, 19);
        entities.HealthInsuranceCoverage.ConHNumber =
          db.GetNullableInt32(reader, 20);
        entities.HealthInsuranceCoverage.CspNumber =
          db.GetNullableString(reader, 21);
        entities.HealthInsuranceCoverage.HicIdentifier =
          db.GetNullableInt32(reader, 22);
        entities.HealthInsuranceCoverage.OtherCoveredPersons =
          db.GetNullableString(reader, 23);
        entities.HealthInsuranceCoverage.CseNumber =
          db.GetNullableString(reader, 24);
        entities.HealthInsuranceCoverage.IsrIdentifier =
          db.GetNullableDateTime(reader, 25);
        entities.HealthInsuranceCoverage.Populated = true;
      });
  }

  private bool ReadHealthInsuranceCoverage2()
  {
    System.Diagnostics.Debug.Assert(entities.HealthInsuranceCoverage.Populated);
    entities.Existing.Populated = false;

    return Read("ReadHealthInsuranceCoverage2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "groupNumber",
          import.HealthInsuranceCoverage.InsuranceGroupNumber ?? "");
        db.SetNullableString(
          command, "policyNumber",
          import.HealthInsuranceCoverage.InsurancePolicyNumber ?? "");
        db.SetNullableString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableInt32(
          command, "hicIdentifier",
          entities.HealthInsuranceCoverage.HicIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Existing.Identifier = db.GetInt64(reader, 0);
        entities.Existing.InsuranceGroupNumber =
          db.GetNullableString(reader, 1);
        entities.Existing.InsurancePolicyNumber =
          db.GetNullableString(reader, 2);
        entities.Existing.CspNumber = db.GetNullableString(reader, 3);
        entities.Existing.HicIdentifier = db.GetNullableInt32(reader, 4);
        entities.Existing.Populated = true;
      });
  }

  private bool ReadIncomeSource1()
  {
    entities.IncomeSource.Populated = false;

    return Read("ReadIncomeSource1",
      (db, command) =>
      {
        db.SetDateTime(
          command, "identifier",
          import.IncomeSource.Identifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.CspINumber = db.GetString(reader, 1);
        entities.IncomeSource.Populated = true;
      });
  }

  private bool ReadIncomeSource2()
  {
    System.Diagnostics.Debug.Assert(entities.HealthInsuranceCoverage.Populated);
    entities.IncomeSource.Populated = false;

    return Read("ReadIncomeSource2",
      (db, command) =>
      {
        db.SetDateTime(
          command, "identifier",
          entities.HealthInsuranceCoverage.IsrIdentifier.GetValueOrDefault());
        db.SetString(
          command, "cspINumber", entities.HealthInsuranceCoverage.CseNumber ?? ""
          );
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.CspINumber = db.GetString(reader, 1);
        entities.IncomeSource.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPersonalHealthInsurance()
  {
    entities.PersonalHealthInsurance.Populated = false;

    return ReadEach("ReadPersonalHealthInsurance",
      (db, command) =>
      {
        db.SetInt64(
          command, "hcvId", entities.HealthInsuranceCoverage.Identifier);
        db.SetNullableDate(
          command, "coverEndDate",
          import.HealthInsuranceCoverage.PolicyExpirationDate.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonalHealthInsurance.HcvId = db.GetInt64(reader, 0);
        entities.PersonalHealthInsurance.CspNumber = db.GetString(reader, 1);
        entities.PersonalHealthInsurance.CoverageEndDate =
          db.GetNullableDate(reader, 2);
        entities.PersonalHealthInsurance.LastUpdatedBy =
          db.GetString(reader, 3);
        entities.PersonalHealthInsurance.LastUpdatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.PersonalHealthInsurance.Populated = true;

        return true;
      });
  }

  private void UpdateHealthInsuranceCoverage()
  {
    var policyPaidByCsePersonInd =
      export.HealthInsuranceCoverage.PolicyPaidByCsePersonInd;
    var insuranceGroupNumber =
      export.HealthInsuranceCoverage.InsuranceGroupNumber ?? "";
    var verifiedDate = export.HealthInsuranceCoverage.VerifiedDate;
    var verifiedUserId = export.HealthInsuranceCoverage.VerifiedUserId ?? "";
    var insurancePolicyNumber =
      export.HealthInsuranceCoverage.InsurancePolicyNumber ?? "";
    var policyExpirationDate =
      export.HealthInsuranceCoverage.PolicyExpirationDate;
    var coverageCode1 = export.HealthInsuranceCoverage.CoverageCode1 ?? "";
    var coverageCode2 = export.HealthInsuranceCoverage.CoverageCode2 ?? "";
    var coverageCode3 = export.HealthInsuranceCoverage.CoverageCode3 ?? "";
    var coverageCode4 = export.HealthInsuranceCoverage.CoverageCode4 ?? "";
    var coverageCode5 = export.HealthInsuranceCoverage.CoverageCode5 ?? "";
    var coverageCode6 = export.HealthInsuranceCoverage.CoverageCode6 ?? "";
    var coverageCode7 = export.HealthInsuranceCoverage.CoverageCode7 ?? "";
    var policyEffectiveDate =
      export.HealthInsuranceCoverage.PolicyEffectiveDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var otherCoveredPersons =
      export.HealthInsuranceCoverage.OtherCoveredPersons ?? "";

    entities.HealthInsuranceCoverage.Populated = false;
    Update("UpdateHealthInsuranceCoverage",
      (db, command) =>
      {
        db.SetString(command, "paidByCsepInd", policyPaidByCsePersonInd);
        db.SetNullableString(command, "groupNumber", insuranceGroupNumber);
        db.SetNullableDate(command, "verifiedDate", verifiedDate);
        db.SetNullableString(command, "verifiedUserId", verifiedUserId);
        db.SetNullableString(command, "policyNumber", insurancePolicyNumber);
        db.SetNullableDate(command, "policyExpDate", policyExpirationDate);
        db.SetNullableString(command, "coverageCode1", coverageCode1);
        db.SetNullableString(command, "coverageCode2", coverageCode2);
        db.SetNullableString(command, "coverageCode3", coverageCode3);
        db.SetNullableString(command, "coverageCode4", coverageCode4);
        db.SetNullableString(command, "coverageCode5", coverageCode5);
        db.SetNullableString(command, "coverageCode6", coverageCode6);
        db.SetNullableString(command, "coverageCode7", coverageCode7);
        db.SetNullableDate(command, "policyEffDate", policyEffectiveDate);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetNullableString(command, "othCovPersons", otherCoveredPersons);
        db.SetInt64(
          command, "identifier", entities.HealthInsuranceCoverage.Identifier);
      });

    entities.HealthInsuranceCoverage.PolicyPaidByCsePersonInd =
      policyPaidByCsePersonInd;
    entities.HealthInsuranceCoverage.InsuranceGroupNumber =
      insuranceGroupNumber;
    entities.HealthInsuranceCoverage.VerifiedDate = verifiedDate;
    entities.HealthInsuranceCoverage.VerifiedUserId = verifiedUserId;
    entities.HealthInsuranceCoverage.InsurancePolicyNumber =
      insurancePolicyNumber;
    entities.HealthInsuranceCoverage.PolicyExpirationDate =
      policyExpirationDate;
    entities.HealthInsuranceCoverage.CoverageCode1 = coverageCode1;
    entities.HealthInsuranceCoverage.CoverageCode2 = coverageCode2;
    entities.HealthInsuranceCoverage.CoverageCode3 = coverageCode3;
    entities.HealthInsuranceCoverage.CoverageCode4 = coverageCode4;
    entities.HealthInsuranceCoverage.CoverageCode5 = coverageCode5;
    entities.HealthInsuranceCoverage.CoverageCode6 = coverageCode6;
    entities.HealthInsuranceCoverage.CoverageCode7 = coverageCode7;
    entities.HealthInsuranceCoverage.PolicyEffectiveDate = policyEffectiveDate;
    entities.HealthInsuranceCoverage.LastUpdatedBy = lastUpdatedBy;
    entities.HealthInsuranceCoverage.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.HealthInsuranceCoverage.OtherCoveredPersons = otherCoveredPersons;
    entities.HealthInsuranceCoverage.Populated = true;
  }

  private void UpdatePersonalHealthInsurance()
  {
    System.Diagnostics.Debug.Assert(entities.PersonalHealthInsurance.Populated);

    var coverageEndDate = import.HealthInsuranceCoverage.PolicyExpirationDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.PersonalHealthInsurance.Populated = false;
    Update("UpdatePersonalHealthInsurance",
      (db, command) =>
      {
        db.SetNullableDate(command, "coverEndDate", coverageEndDate);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetInt64(command, "hcvId", entities.PersonalHealthInsurance.HcvId);
        db.SetString(
          command, "cspNumber", entities.PersonalHealthInsurance.CspNumber);
      });

    entities.PersonalHealthInsurance.CoverageEndDate = coverageEndDate;
    entities.PersonalHealthInsurance.LastUpdatedBy = lastUpdatedBy;
    entities.PersonalHealthInsurance.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.PersonalHealthInsurance.Populated = true;
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
    /// A value of EndDateCoverage.
    /// </summary>
    [JsonPropertyName("endDateCoverage")]
    public Common EndDateCoverage
    {
      get => endDateCoverage ??= new();
      set => endDateCoverage = value;
    }

    /// <summary>
    /// A value of DissasociateIncomeSourc.
    /// </summary>
    [JsonPropertyName("dissasociateIncomeSourc")]
    public Common DissasociateIncomeSourc
    {
      get => dissasociateIncomeSourc ??= new();
      set => dissasociateIncomeSourc = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of HealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("healthInsuranceCoverage")]
    public HealthInsuranceCoverage HealthInsuranceCoverage
    {
      get => healthInsuranceCoverage ??= new();
      set => healthInsuranceCoverage = value;
    }

    /// <summary>
    /// A value of Contact.
    /// </summary>
    [JsonPropertyName("contact")]
    public Contact Contact
    {
      get => contact ??= new();
      set => contact = value;
    }

    private Common endDateCoverage;
    private Common dissasociateIncomeSourc;
    private IncomeSource incomeSource;
    private CsePerson csePerson;
    private Case1 case1;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private Contact contact;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of Contact.
    /// </summary>
    [JsonPropertyName("contact")]
    public Contact Contact
    {
      get => contact ??= new();
      set => contact = value;
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

    private HealthInsuranceCoverage healthInsuranceCoverage;
    private Contact contact;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public IncomeSource Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

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
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    /// <summary>
    /// A value of FindDoc.
    /// </summary>
    [JsonPropertyName("findDoc")]
    public Common FindDoc
    {
      get => findDoc ??= new();
      set => findDoc = value;
    }

    /// <summary>
    /// A value of ProgramId.
    /// </summary>
    [JsonPropertyName("programId")]
    public WorkArea ProgramId
    {
      get => programId ??= new();
      set => programId = value;
    }

    /// <summary>
    /// A value of SpDocKey.
    /// </summary>
    [JsonPropertyName("spDocKey")]
    public SpDocKey SpDocKey
    {
      get => spDocKey ??= new();
      set => spDocKey = value;
    }

    private IncomeSource blank;
    private DateWorkArea current;
    private Document document;
    private Common findDoc;
    private WorkArea programId;
    private SpDocKey spDocKey;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ArCsePerson.
    /// </summary>
    [JsonPropertyName("arCsePerson")]
    public CsePerson ArCsePerson
    {
      get => arCsePerson ??= new();
      set => arCsePerson = value;
    }

    /// <summary>
    /// A value of ChildCsePerson.
    /// </summary>
    [JsonPropertyName("childCsePerson")]
    public CsePerson ChildCsePerson
    {
      get => childCsePerson ??= new();
      set => childCsePerson = value;
    }

    /// <summary>
    /// A value of ArCaseRole.
    /// </summary>
    [JsonPropertyName("arCaseRole")]
    public CaseRole ArCaseRole
    {
      get => arCaseRole ??= new();
      set => arCaseRole = value;
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
    /// A value of ChildCaseRole.
    /// </summary>
    [JsonPropertyName("childCaseRole")]
    public CaseRole ChildCaseRole
    {
      get => childCaseRole ??= new();
      set => childCaseRole = value;
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
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
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
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public HealthInsuranceCoverage Existing
    {
      get => existing ??= new();
      set => existing = value;
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
    /// A value of HealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("healthInsuranceCoverage")]
    public HealthInsuranceCoverage HealthInsuranceCoverage
    {
      get => healthInsuranceCoverage ??= new();
      set => healthInsuranceCoverage = value;
    }

    /// <summary>
    /// A value of Contact.
    /// </summary>
    [JsonPropertyName("contact")]
    public Contact Contact
    {
      get => contact ??= new();
      set => contact = value;
    }

    private CsePerson arCsePerson;
    private CsePerson childCsePerson;
    private CaseRole arCaseRole;
    private Case1 case1;
    private CaseRole childCaseRole;
    private PersonalHealthInsurance personalHealthInsurance;
    private IncomeSource incomeSource;
    private HealthInsuranceCompany healthInsuranceCompany;
    private HealthInsuranceCoverage existing;
    private CsePerson csePerson;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private Contact contact;
  }
#endregion
}
