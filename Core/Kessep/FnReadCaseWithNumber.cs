// Program: FN_READ_CASE_WITH_NUMBER, ID: 372063205, model: 746.
// Short name: SWE00530
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_READ_CASE_WITH_NUMBER.
/// </para>
/// <para>
/// RESP: FINCLMGMNT	
/// This CAB will read the CASE entity type using an input case number.  Its 
/// original purpose is to valid a case # input on a screen.
/// </para>
/// </summary>
[Serializable]
public partial class FnReadCaseWithNumber: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_READ_CASE_WITH_NUMBER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReadCaseWithNumber(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReadCaseWithNumber.
  /// </summary>
  public FnReadCaseWithNumber(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadCase())
    {
      export.Case1.Assign(entities.Case1);
    }
    else
    {
      ExitState = "CASE_NF";
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
        entities.Case1.FullServiceWithoutMedInd =
          db.GetNullableString(reader, 0);
        entities.Case1.ManagementArea = db.GetNullableString(reader, 1);
        entities.Case1.ManagementRegion = db.GetNullableString(reader, 2);
        entities.Case1.FullServiceWithMedInd = db.GetNullableString(reader, 3);
        entities.Case1.LocateInd = db.GetNullableString(reader, 4);
        entities.Case1.ClosureReason = db.GetNullableString(reader, 5);
        entities.Case1.Number = db.GetString(reader, 6);
        entities.Case1.InformationRequestNumber =
          db.GetNullableInt64(reader, 7);
        entities.Case1.StationId = db.GetNullableString(reader, 8);
        entities.Case1.ApplicantLastName = db.GetNullableString(reader, 9);
        entities.Case1.ApplicantFirstName = db.GetNullableString(reader, 10);
        entities.Case1.ApplicantMiddleInitial =
          db.GetNullableString(reader, 11);
        entities.Case1.ApplicationSentDate = db.GetNullableDate(reader, 12);
        entities.Case1.ApplicationRequestDate = db.GetNullableDate(reader, 13);
        entities.Case1.ApplicationReturnDate = db.GetNullableDate(reader, 14);
        entities.Case1.DeniedRequestDate = db.GetNullableDate(reader, 15);
        entities.Case1.DeniedRequestCode = db.GetNullableString(reader, 16);
        entities.Case1.DeniedRequestReason = db.GetNullableString(reader, 17);
        entities.Case1.Status = db.GetNullableString(reader, 18);
        entities.Case1.KsFipsCode = db.GetNullableString(reader, 19);
        entities.Case1.ValidApplicationReceivedDate =
          db.GetNullableDate(reader, 20);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 21);
        entities.Case1.Potential = db.GetNullableString(reader, 22);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 23);
        entities.Case1.OfficeIdentifier = db.GetNullableInt32(reader, 24);
        entities.Case1.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 25);
        entities.Case1.LastUpdatedBy = db.GetNullableString(reader, 26);
        entities.Case1.CreatedTimestamp = db.GetDateTime(reader, 27);
        entities.Case1.CreatedBy = db.GetString(reader, 28);
        entities.Case1.Note = db.GetNullableString(reader, 29);
        entities.Case1.Populated = true;
        CheckValid<Case1>("Potential", entities.Case1.Potential);
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private Case1 case1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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

    private Case1 case1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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

    private Case1 case1;
  }
#endregion
}
