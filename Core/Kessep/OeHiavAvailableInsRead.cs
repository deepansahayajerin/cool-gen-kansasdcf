// Program: OE_HIAV_AVAILABLE_INS_READ, ID: 371857740, model: 746.
// Short name: SWE02301
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_HIAV_AVAILABLE_INS_READ.
/// </summary>
[Serializable]
public partial class OeHiavAvailableInsRead: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_HIAV_AVAILABLE_INS_READ program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeHiavAvailableInsRead(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeHiavAvailableInsRead.
  /// </summary>
  public OeHiavAvailableInsRead(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ******** MAINTENANCE LOG ********************
    // AUTHOR    	 DATE 		DESCRIPTION
    // C. Chhun	01/05/99	Initial Code
    // ******** END MAINTENANCE LOG ****************
    export.More.Flag = "N";
    local.Common.Count = 0;

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        foreach(var item in ReadHealthInsuranceAvailability3())
        {
          ++local.Common.Count;

          if (local.Common.Count > 1)
          {
            export.More.Flag = "Y";

            break;
          }

          export.HealthInsuranceAvailability.Assign(
            entities.HealthInsuranceAvailability);
        }

        break;
      case "NEXT":
        foreach(var item in ReadHealthInsuranceAvailability1())
        {
          ++local.Common.Count;

          if (local.Common.Count > 1)
          {
            export.More.Flag = "Y";

            break;
          }

          export.HealthInsuranceAvailability.Assign(
            entities.HealthInsuranceAvailability);
        }

        break;
      case "PREV":
        foreach(var item in ReadHealthInsuranceAvailability2())
        {
          ++local.Common.Count;

          if (local.Common.Count > 1)
          {
            export.More.Flag = "Y";

            break;
          }

          export.HealthInsuranceAvailability.Assign(
            entities.HealthInsuranceAvailability);
        }

        break;
      default:
        break;
    }

    if (local.Common.Count == 0)
    {
      ExitState = "OE0189_AVAILABLE_HEALTH_INS_NF";
    }
  }

  private IEnumerable<bool> ReadHealthInsuranceAvailability1()
  {
    entities.HealthInsuranceAvailability.Populated = false;

    return ReadEach("ReadHealthInsuranceAvailability1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetInt32(
          command, "insuranceId",
          import.HealthInsuranceAvailability.InsuranceId);
      },
      (db, reader) =>
      {
        entities.HealthInsuranceAvailability.InsuranceId =
          db.GetInt32(reader, 0);
        entities.HealthInsuranceAvailability.InsurancePolicyNumber =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceAvailability.InsuranceGroupNumber =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceAvailability.InsuranceName =
          db.GetString(reader, 3);
        entities.HealthInsuranceAvailability.Street1 =
          db.GetNullableString(reader, 4);
        entities.HealthInsuranceAvailability.Street2 =
          db.GetNullableString(reader, 5);
        entities.HealthInsuranceAvailability.City =
          db.GetNullableString(reader, 6);
        entities.HealthInsuranceAvailability.State =
          db.GetNullableString(reader, 7);
        entities.HealthInsuranceAvailability.Zip5 =
          db.GetNullableString(reader, 8);
        entities.HealthInsuranceAvailability.Zip4 =
          db.GetNullableString(reader, 9);
        entities.HealthInsuranceAvailability.Cost =
          db.GetNullableDecimal(reader, 10);
        entities.HealthInsuranceAvailability.CostFrequency =
          db.GetNullableString(reader, 11);
        entities.HealthInsuranceAvailability.VerifiedDate =
          db.GetNullableDate(reader, 12);
        entities.HealthInsuranceAvailability.EndDate =
          db.GetNullableDate(reader, 13);
        entities.HealthInsuranceAvailability.EmployerName =
          db.GetString(reader, 14);
        entities.HealthInsuranceAvailability.EmpStreet1 =
          db.GetNullableString(reader, 15);
        entities.HealthInsuranceAvailability.EmpStreet2 =
          db.GetNullableString(reader, 16);
        entities.HealthInsuranceAvailability.EmpCity =
          db.GetNullableString(reader, 17);
        entities.HealthInsuranceAvailability.EmpState =
          db.GetNullableString(reader, 18);
        entities.HealthInsuranceAvailability.EmpZip5 =
          db.GetNullableString(reader, 19);
        entities.HealthInsuranceAvailability.EmpZip4 =
          db.GetNullableString(reader, 20);
        entities.HealthInsuranceAvailability.EmpPhoneAreaCode =
          db.GetInt32(reader, 21);
        entities.HealthInsuranceAvailability.EmpPhoneNo =
          db.GetNullableInt32(reader, 22);
        entities.HealthInsuranceAvailability.LastUpdatedBy =
          db.GetString(reader, 23);
        entities.HealthInsuranceAvailability.LastUpdatedTimestamp =
          db.GetDateTime(reader, 24);
        entities.HealthInsuranceAvailability.CspNumber =
          db.GetString(reader, 25);
        entities.HealthInsuranceAvailability.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadHealthInsuranceAvailability2()
  {
    entities.HealthInsuranceAvailability.Populated = false;

    return ReadEach("ReadHealthInsuranceAvailability2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetInt32(
          command, "insuranceId",
          import.HealthInsuranceAvailability.InsuranceId);
      },
      (db, reader) =>
      {
        entities.HealthInsuranceAvailability.InsuranceId =
          db.GetInt32(reader, 0);
        entities.HealthInsuranceAvailability.InsurancePolicyNumber =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceAvailability.InsuranceGroupNumber =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceAvailability.InsuranceName =
          db.GetString(reader, 3);
        entities.HealthInsuranceAvailability.Street1 =
          db.GetNullableString(reader, 4);
        entities.HealthInsuranceAvailability.Street2 =
          db.GetNullableString(reader, 5);
        entities.HealthInsuranceAvailability.City =
          db.GetNullableString(reader, 6);
        entities.HealthInsuranceAvailability.State =
          db.GetNullableString(reader, 7);
        entities.HealthInsuranceAvailability.Zip5 =
          db.GetNullableString(reader, 8);
        entities.HealthInsuranceAvailability.Zip4 =
          db.GetNullableString(reader, 9);
        entities.HealthInsuranceAvailability.Cost =
          db.GetNullableDecimal(reader, 10);
        entities.HealthInsuranceAvailability.CostFrequency =
          db.GetNullableString(reader, 11);
        entities.HealthInsuranceAvailability.VerifiedDate =
          db.GetNullableDate(reader, 12);
        entities.HealthInsuranceAvailability.EndDate =
          db.GetNullableDate(reader, 13);
        entities.HealthInsuranceAvailability.EmployerName =
          db.GetString(reader, 14);
        entities.HealthInsuranceAvailability.EmpStreet1 =
          db.GetNullableString(reader, 15);
        entities.HealthInsuranceAvailability.EmpStreet2 =
          db.GetNullableString(reader, 16);
        entities.HealthInsuranceAvailability.EmpCity =
          db.GetNullableString(reader, 17);
        entities.HealthInsuranceAvailability.EmpState =
          db.GetNullableString(reader, 18);
        entities.HealthInsuranceAvailability.EmpZip5 =
          db.GetNullableString(reader, 19);
        entities.HealthInsuranceAvailability.EmpZip4 =
          db.GetNullableString(reader, 20);
        entities.HealthInsuranceAvailability.EmpPhoneAreaCode =
          db.GetInt32(reader, 21);
        entities.HealthInsuranceAvailability.EmpPhoneNo =
          db.GetNullableInt32(reader, 22);
        entities.HealthInsuranceAvailability.LastUpdatedBy =
          db.GetString(reader, 23);
        entities.HealthInsuranceAvailability.LastUpdatedTimestamp =
          db.GetDateTime(reader, 24);
        entities.HealthInsuranceAvailability.CspNumber =
          db.GetString(reader, 25);
        entities.HealthInsuranceAvailability.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadHealthInsuranceAvailability3()
  {
    entities.HealthInsuranceAvailability.Populated = false;

    return ReadEach("ReadHealthInsuranceAvailability3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.HealthInsuranceAvailability.InsuranceId =
          db.GetInt32(reader, 0);
        entities.HealthInsuranceAvailability.InsurancePolicyNumber =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceAvailability.InsuranceGroupNumber =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceAvailability.InsuranceName =
          db.GetString(reader, 3);
        entities.HealthInsuranceAvailability.Street1 =
          db.GetNullableString(reader, 4);
        entities.HealthInsuranceAvailability.Street2 =
          db.GetNullableString(reader, 5);
        entities.HealthInsuranceAvailability.City =
          db.GetNullableString(reader, 6);
        entities.HealthInsuranceAvailability.State =
          db.GetNullableString(reader, 7);
        entities.HealthInsuranceAvailability.Zip5 =
          db.GetNullableString(reader, 8);
        entities.HealthInsuranceAvailability.Zip4 =
          db.GetNullableString(reader, 9);
        entities.HealthInsuranceAvailability.Cost =
          db.GetNullableDecimal(reader, 10);
        entities.HealthInsuranceAvailability.CostFrequency =
          db.GetNullableString(reader, 11);
        entities.HealthInsuranceAvailability.VerifiedDate =
          db.GetNullableDate(reader, 12);
        entities.HealthInsuranceAvailability.EndDate =
          db.GetNullableDate(reader, 13);
        entities.HealthInsuranceAvailability.EmployerName =
          db.GetString(reader, 14);
        entities.HealthInsuranceAvailability.EmpStreet1 =
          db.GetNullableString(reader, 15);
        entities.HealthInsuranceAvailability.EmpStreet2 =
          db.GetNullableString(reader, 16);
        entities.HealthInsuranceAvailability.EmpCity =
          db.GetNullableString(reader, 17);
        entities.HealthInsuranceAvailability.EmpState =
          db.GetNullableString(reader, 18);
        entities.HealthInsuranceAvailability.EmpZip5 =
          db.GetNullableString(reader, 19);
        entities.HealthInsuranceAvailability.EmpZip4 =
          db.GetNullableString(reader, 20);
        entities.HealthInsuranceAvailability.EmpPhoneAreaCode =
          db.GetInt32(reader, 21);
        entities.HealthInsuranceAvailability.EmpPhoneNo =
          db.GetNullableInt32(reader, 22);
        entities.HealthInsuranceAvailability.LastUpdatedBy =
          db.GetString(reader, 23);
        entities.HealthInsuranceAvailability.LastUpdatedTimestamp =
          db.GetDateTime(reader, 24);
        entities.HealthInsuranceAvailability.CspNumber =
          db.GetString(reader, 25);
        entities.HealthInsuranceAvailability.Populated = true;

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
    /// A value of HealthInsuranceAvailability.
    /// </summary>
    [JsonPropertyName("healthInsuranceAvailability")]
    public HealthInsuranceAvailability HealthInsuranceAvailability
    {
      get => healthInsuranceAvailability ??= new();
      set => healthInsuranceAvailability = value;
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

    private HealthInsuranceAvailability healthInsuranceAvailability;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of HealthInsuranceAvailability.
    /// </summary>
    [JsonPropertyName("healthInsuranceAvailability")]
    public HealthInsuranceAvailability HealthInsuranceAvailability
    {
      get => healthInsuranceAvailability ??= new();
      set => healthInsuranceAvailability = value;
    }

    /// <summary>
    /// A value of More.
    /// </summary>
    [JsonPropertyName("more")]
    public Common More
    {
      get => more ??= new();
      set => more = value;
    }

    private HealthInsuranceAvailability healthInsuranceAvailability;
    private Common more;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private Common common;
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
    /// A value of HealthInsuranceAvailability.
    /// </summary>
    [JsonPropertyName("healthInsuranceAvailability")]
    public HealthInsuranceAvailability HealthInsuranceAvailability
    {
      get => healthInsuranceAvailability ??= new();
      set => healthInsuranceAvailability = value;
    }

    private CsePerson csePerson;
    private HealthInsuranceAvailability healthInsuranceAvailability;
  }
#endregion
}
