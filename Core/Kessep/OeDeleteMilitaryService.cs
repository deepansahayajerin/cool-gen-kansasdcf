// Program: OE_DELETE_MILITARY_SERVICE, ID: 371920134, model: 746.
// Short name: SWE00903
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_DELETE_MILITARY_SERVICE.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// WRITTEN BY:SID
/// </para>
/// </summary>
[Serializable]
public partial class OeDeleteMilitaryService: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_DELETE_MILITARY_SERVICE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeDeleteMilitaryService(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeDeleteMilitaryService.
  /// </summary>
  public OeDeleteMilitaryService(IContext context, Import import, Export export):
    
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
    // Read military service record related to CSE
    // Person identified and delete that record.
    // ---------------------------------------------
    if (ReadCsePerson())
    {
      if (ReadMilitaryService())
      {
        DeleteMilitaryService();
        ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
      }
      else
      {
        ExitState = "MILITARY_SERVICE_NF";
      }
    }
  }

  private void DeleteMilitaryService()
  {
    Update("DeleteMilitaryService",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.MilitaryService.EffectiveDate.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.MilitaryService.CspNumber);
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

  private bool ReadMilitaryService()
  {
    entities.MilitaryService.Populated = false;

    return Read("ReadMilitaryService",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          import.MilitaryService.EffectiveDate.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.MilitaryService.EffectiveDate = db.GetDate(reader, 0);
        entities.MilitaryService.CspNumber = db.GetString(reader, 1);
        entities.MilitaryService.StartDate = db.GetNullableDate(reader, 2);
        entities.MilitaryService.Street1 = db.GetNullableString(reader, 3);
        entities.MilitaryService.Street2 = db.GetNullableString(reader, 4);
        entities.MilitaryService.City = db.GetNullableString(reader, 5);
        entities.MilitaryService.State = db.GetNullableString(reader, 6);
        entities.MilitaryService.ZipCode5 = db.GetNullableString(reader, 7);
        entities.MilitaryService.ZipCode4 = db.GetNullableString(reader, 8);
        entities.MilitaryService.Zip3 = db.GetNullableString(reader, 9);
        entities.MilitaryService.Country = db.GetNullableString(reader, 10);
        entities.MilitaryService.Apo = db.GetNullableString(reader, 11);
        entities.MilitaryService.ExpectedReturnDateToStates =
          db.GetNullableDate(reader, 12);
        entities.MilitaryService.OverseasDutyStation =
          db.GetNullableString(reader, 13);
        entities.MilitaryService.ExpectedDischargeDate =
          db.GetNullableDate(reader, 14);
        entities.MilitaryService.Phone = db.GetNullableInt32(reader, 15);
        entities.MilitaryService.BranchCode = db.GetNullableString(reader, 16);
        entities.MilitaryService.CommandingOfficerLastName =
          db.GetNullableString(reader, 17);
        entities.MilitaryService.CommandingOfficerFirstName =
          db.GetNullableString(reader, 18);
        entities.MilitaryService.CommandingOfficerMi =
          db.GetNullableString(reader, 19);
        entities.MilitaryService.CurrentUsDutyStation =
          db.GetNullableString(reader, 20);
        entities.MilitaryService.DutyStatusCode =
          db.GetNullableString(reader, 21);
        entities.MilitaryService.Rank = db.GetNullableString(reader, 22);
        entities.MilitaryService.EndDate = db.GetNullableDate(reader, 23);
        entities.MilitaryService.Populated = true;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of MilitaryService.
    /// </summary>
    [JsonPropertyName("militaryService")]
    public MilitaryService MilitaryService
    {
      get => militaryService ??= new();
      set => militaryService = value;
    }

    private CsePerson csePerson;
    private MilitaryService militaryService;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of MilitaryService.
    /// </summary>
    [JsonPropertyName("militaryService")]
    public MilitaryService MilitaryService
    {
      get => militaryService ??= new();
      set => militaryService = value;
    }

    private MilitaryService militaryService;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of MilitaryService.
    /// </summary>
    [JsonPropertyName("militaryService")]
    public MilitaryService MilitaryService
    {
      get => militaryService ??= new();
      set => militaryService = value;
    }

    private IncomeSource military;
    private CsePerson csePerson;
    private MilitaryService militaryService;
  }
#endregion
}
