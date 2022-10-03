// Program: SI_B273_COMPARE_MILITARY_INFO, ID: 371071903, model: 746.
// Short name: SWE01290
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B273_COMPARE_MILITARY_INFO.
/// </summary>
[Serializable]
public partial class SiB273CompareMilitaryInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B273_COMPARE_MILITARY_INFO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB273CompareMilitaryInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB273CompareMilitaryInfo.
  /// </summary>
  public SiB273CompareMilitaryInfo(IContext context, Import import,
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
    export.EabFileHandling.Status = "OK";
    export.EmployerInfoMismatch.Count = import.EmployerInfoMismatch.Count;

    if (ReadEmployerDomesticEmployerAddress())
    {
      if (Equal(import.FcrEmployerAddress.Street1,
        entities.DomesticEmployerAddress.Street1) && Equal
        (import.FcrEmployerAddress.Street2,
        entities.DomesticEmployerAddress.Street2) && Equal
        (import.FcrEmployerAddress.City, entities.DomesticEmployerAddress.City) &&
        Equal
        (import.FcrEmployerAddress.State, entities.DomesticEmployerAddress.State)
        && Equal
        (import.FcrEmployerAddress.ZipCode,
        entities.DomesticEmployerAddress.ZipCode) && Equal
        (import.FcrEmployerAddress.Zip4, entities.DomesticEmployerAddress.Zip4))
      {
      }
      else
      {
        ++export.EmployerInfoMismatch.Count;
        UseSiB273EmployerInfoMismatch();
      }
    }
    else
    {
      ExitState = "INCOME_SOURCE_NF";
    }
  }

  private static void MoveEmployer(Employer source, Employer target)
  {
    target.Ein = source.Ein;
    target.Name = source.Name;
  }

  private void UseSiB273EmployerInfoMismatch()
  {
    var useImport = new SiB273EmployerInfoMismatch.Import();
    var useExport = new SiB273EmployerInfoMismatch.Export();

    MoveEmployer(entities.Employer, useImport.KaecsesEmployer);
    useImport.KaecsesDomesticEmployerAddress.Assign(
      entities.DomesticEmployerAddress);
    MoveEmployer(import.FcrEmployer, useImport.FcrEmployer);
    useImport.FcrEmployerAddress.Assign(import.FcrEmployerAddress);

    Call(SiB273EmployerInfoMismatch.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private bool ReadEmployerDomesticEmployerAddress()
  {
    entities.Employer.Populated = false;
    entities.DomesticEmployerAddress.Populated = false;

    return Read("ReadEmployerDomesticEmployerAddress",
      (db, command) =>
      {
        db.SetDateTime(
          command, "identifier",
          import.Military.Identifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Employer.Identifier = db.GetInt32(reader, 0);
        entities.DomesticEmployerAddress.EmpId = db.GetInt32(reader, 0);
        entities.Employer.Ein = db.GetNullableString(reader, 1);
        entities.Employer.KansasId = db.GetNullableString(reader, 2);
        entities.Employer.Name = db.GetNullableString(reader, 3);
        entities.Employer.CreatedBy = db.GetString(reader, 4);
        entities.Employer.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.Employer.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.Employer.LastUpdatedTstamp = db.GetNullableDateTime(reader, 7);
        entities.Employer.PhoneNo = db.GetNullableString(reader, 8);
        entities.Employer.AreaCode = db.GetNullableInt32(reader, 9);
        entities.DomesticEmployerAddress.LocationType =
          db.GetString(reader, 10);
        entities.DomesticEmployerAddress.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.DomesticEmployerAddress.LastUpdatedBy =
          db.GetNullableString(reader, 12);
        entities.DomesticEmployerAddress.CreatedTimestamp =
          db.GetDateTime(reader, 13);
        entities.DomesticEmployerAddress.CreatedBy = db.GetString(reader, 14);
        entities.DomesticEmployerAddress.Street1 =
          db.GetNullableString(reader, 15);
        entities.DomesticEmployerAddress.Street2 =
          db.GetNullableString(reader, 16);
        entities.DomesticEmployerAddress.City =
          db.GetNullableString(reader, 17);
        entities.DomesticEmployerAddress.Identifier =
          db.GetDateTime(reader, 18);
        entities.DomesticEmployerAddress.State =
          db.GetNullableString(reader, 19);
        entities.DomesticEmployerAddress.ZipCode =
          db.GetNullableString(reader, 20);
        entities.DomesticEmployerAddress.Zip4 =
          db.GetNullableString(reader, 21);
        entities.DomesticEmployerAddress.Zip3 =
          db.GetNullableString(reader, 22);
        entities.DomesticEmployerAddress.County =
          db.GetNullableString(reader, 23);
        entities.DomesticEmployerAddress.Note =
          db.GetNullableString(reader, 24);
        entities.Employer.Populated = true;
        entities.DomesticEmployerAddress.Populated = true;
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
    /// A value of Military.
    /// </summary>
    [JsonPropertyName("military")]
    public IncomeSource Military
    {
      get => military ??= new();
      set => military = value;
    }

    /// <summary>
    /// A value of EmployerInfoMismatch.
    /// </summary>
    [JsonPropertyName("employerInfoMismatch")]
    public Common EmployerInfoMismatch
    {
      get => employerInfoMismatch ??= new();
      set => employerInfoMismatch = value;
    }

    /// <summary>
    /// A value of FcrEmployer.
    /// </summary>
    [JsonPropertyName("fcrEmployer")]
    public Employer FcrEmployer
    {
      get => fcrEmployer ??= new();
      set => fcrEmployer = value;
    }

    /// <summary>
    /// A value of FcrEmployerAddress.
    /// </summary>
    [JsonPropertyName("fcrEmployerAddress")]
    public EmployerAddress FcrEmployerAddress
    {
      get => fcrEmployerAddress ??= new();
      set => fcrEmployerAddress = value;
    }

    private IncomeSource military;
    private Common employerInfoMismatch;
    private Employer fcrEmployer;
    private EmployerAddress fcrEmployerAddress;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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
    /// A value of EmployerInfoMismatch.
    /// </summary>
    [JsonPropertyName("employerInfoMismatch")]
    public Common EmployerInfoMismatch
    {
      get => employerInfoMismatch ??= new();
      set => employerInfoMismatch = value;
    }

    private EabFileHandling eabFileHandling;
    private Common employerInfoMismatch;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Military.
    /// </summary>
    [JsonPropertyName("military")]
    public IncomeSource Military
    {
      get => military ??= new();
      set => military = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of DomesticEmployerAddress.
    /// </summary>
    [JsonPropertyName("domesticEmployerAddress")]
    public EmployerAddress DomesticEmployerAddress
    {
      get => domesticEmployerAddress ??= new();
      set => domesticEmployerAddress = value;
    }

    private IncomeSource military;
    private Employer employer;
    private EmployerAddress domesticEmployerAddress;
  }
#endregion
}
