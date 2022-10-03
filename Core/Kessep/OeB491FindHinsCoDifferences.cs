// Program: OE_B491_FIND_HINS_CO_DIFFERENCES, ID: 371174666, model: 746.
// Short name: SWE02487
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B491_FIND_HINS_CO_DIFFERENCES.
/// </summary>
[Serializable]
public partial class OeB491FindHinsCoDifferences: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B491_FIND_HINS_CO_DIFFERENCES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB491FindHinsCoDifferences(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB491FindHinsCoDifferences.
  /// </summary>
  public OeB491FindHinsCoDifferences(IContext context, Import import,
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
    export.FlagNoDifferences.Flag = "N";
    local.EabFileHandling.Action = "WRITE";

    if (ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress())
    {
      if (!Equal(import.HealthInsuranceCompany.ContactName,
        entities.HealthInsuranceCompany.ContactName))
      {
        return;
      }

      if (!Equal(import.HealthInsuranceCompany.InsurancePolicyCarrier,
        entities.HealthInsuranceCompany.InsurancePolicyCarrier))
      {
        return;
      }

      if (!Equal(import.HealthInsuranceCompany.InsurerFax.GetValueOrDefault(),
        entities.HealthInsuranceCompany.InsurerFax))
      {
        return;
      }

      if (!Equal(import.HealthInsuranceCompany.InsurerFaxAreaCode.
        GetValueOrDefault(),
        entities.HealthInsuranceCompany.InsurerFaxAreaCode))
      {
        return;
      }

      if (!Equal(import.HealthInsuranceCompany.InsurerFaxExt,
        entities.HealthInsuranceCompany.InsurerFaxExt))
      {
        return;
      }

      if (!Equal(import.HealthInsuranceCompany.InsurerPhone.GetValueOrDefault(),
        entities.HealthInsuranceCompany.InsurerPhone))
      {
        return;
      }

      if (!Equal(import.HealthInsuranceCompany.InsurerPhoneAreaCode.
        GetValueOrDefault(),
        entities.HealthInsuranceCompany.InsurerPhoneAreaCode))
      {
        return;
      }

      if (!Equal(import.HealthInsuranceCompany.InsurerPhoneExt,
        entities.HealthInsuranceCompany.InsurerPhoneExt))
      {
        return;
      }

      if (!Equal(import.HealthInsuranceCompanyAddress.City,
        entities.HealthInsuranceCompanyAddress.City))
      {
        return;
      }

      if (!Equal(import.HealthInsuranceCompanyAddress.State,
        entities.HealthInsuranceCompanyAddress.State))
      {
        return;
      }

      if (!Equal(import.HealthInsuranceCompanyAddress.Street1,
        entities.HealthInsuranceCompanyAddress.Street1))
      {
        return;
      }

      if (!Equal(import.HealthInsuranceCompanyAddress.Street2,
        entities.HealthInsuranceCompanyAddress.Street2))
      {
        return;
      }

      if (!Equal(import.HealthInsuranceCompanyAddress.ZipCode5,
        import.HealthInsuranceCompanyAddress.ZipCode5))
      {
        return;
      }

      if (!Equal(import.HealthInsuranceCompanyAddress.ZipCode4,
        entities.HealthInsuranceCompanyAddress.ZipCode4))
      {
        return;
      }

      if (!Equal(import.HealthInsuranceCompanyAddress.Zip3,
        entities.HealthInsuranceCompanyAddress.Zip3))
      {
        return;
      }

      export.FlagNoDifferences.Flag = "Y";
    }
  }

  private bool ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress()
  {
    entities.HealthInsuranceCompany.Populated = false;
    entities.HealthInsuranceCompanyAddress.Populated = false;

    return Read("ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress",
      (db, command) =>
      {
        db.SetNullableString(
          command, "carrierCode", import.HealthInsuranceCompany.CarrierCode ?? ""
          );
      },
      (db, reader) =>
      {
        entities.HealthInsuranceCompany.Identifier = db.GetInt32(reader, 0);
        entities.HealthInsuranceCompanyAddress.HicIdentifier =
          db.GetInt32(reader, 0);
        entities.HealthInsuranceCompany.CarrierCode =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCompany.InsurancePolicyCarrier =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCompany.ContactName =
          db.GetNullableString(reader, 3);
        entities.HealthInsuranceCompany.InsurerPhone =
          db.GetNullableInt32(reader, 4);
        entities.HealthInsuranceCompany.InsurerFax =
          db.GetNullableInt32(reader, 5);
        entities.HealthInsuranceCompany.InsurerFaxExt =
          db.GetNullableString(reader, 6);
        entities.HealthInsuranceCompany.InsurerPhoneExt =
          db.GetNullableString(reader, 7);
        entities.HealthInsuranceCompany.InsurerPhoneAreaCode =
          db.GetNullableInt32(reader, 8);
        entities.HealthInsuranceCompany.InsurerFaxAreaCode =
          db.GetNullableInt32(reader, 9);
        entities.HealthInsuranceCompanyAddress.EffectiveDate =
          db.GetDate(reader, 10);
        entities.HealthInsuranceCompanyAddress.Street1 =
          db.GetNullableString(reader, 11);
        entities.HealthInsuranceCompanyAddress.Street2 =
          db.GetNullableString(reader, 12);
        entities.HealthInsuranceCompanyAddress.City =
          db.GetNullableString(reader, 13);
        entities.HealthInsuranceCompanyAddress.State =
          db.GetNullableString(reader, 14);
        entities.HealthInsuranceCompanyAddress.ZipCode5 =
          db.GetNullableString(reader, 15);
        entities.HealthInsuranceCompanyAddress.ZipCode4 =
          db.GetNullableString(reader, 16);
        entities.HealthInsuranceCompanyAddress.Zip3 =
          db.GetNullableString(reader, 17);
        entities.HealthInsuranceCompany.Populated = true;
        entities.HealthInsuranceCompanyAddress.Populated = true;
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
    /// A value of HealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompany")]
    public HealthInsuranceCompany HealthInsuranceCompany
    {
      get => healthInsuranceCompany ??= new();
      set => healthInsuranceCompany = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCompanyAddress.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompanyAddress")]
    public HealthInsuranceCompanyAddress HealthInsuranceCompanyAddress
    {
      get => healthInsuranceCompanyAddress ??= new();
      set => healthInsuranceCompanyAddress = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    private HealthInsuranceCompany healthInsuranceCompany;
    private HealthInsuranceCompanyAddress healthInsuranceCompanyAddress;
    private ProgramProcessingInfo programProcessingInfo;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of FlagNoDifferences.
    /// </summary>
    [JsonPropertyName("flagNoDifferences")]
    public Common FlagNoDifferences
    {
      get => flagNoDifferences ??= new();
      set => flagNoDifferences = value;
    }

    private Common flagNoDifferences;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
    }

    private EabFileHandling eabFileHandling;
    private EabReportSend neededToWrite;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of HealthInsuranceCompanyAddress.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompanyAddress")]
    public HealthInsuranceCompanyAddress HealthInsuranceCompanyAddress
    {
      get => healthInsuranceCompanyAddress ??= new();
      set => healthInsuranceCompanyAddress = value;
    }

    private HealthInsuranceCompany healthInsuranceCompany;
    private HealthInsuranceCompanyAddress healthInsuranceCompanyAddress;
  }
#endregion
}
