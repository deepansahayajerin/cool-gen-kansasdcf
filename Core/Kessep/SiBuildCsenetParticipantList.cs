// Program: SI_BUILD_CSENET_PARTICIPANT_LIST, ID: 372514468, model: 746.
// Short name: SWE01102
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
/// A program: SI_BUILD_CSENET_PARTICIPANT_LIST.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiBuildCsenetParticipantList: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_BUILD_CSENET_PARTICIPANT_LIST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiBuildCsenetParticipantList(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiBuildCsenetParticipantList.
  /// </summary>
  public SiBuildCsenetParticipantList(IContext context, Import import,
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
    //         M A I N T E N A N C E   L O G
    //  Date	 Developer   Description
    // 5-22-95  Ken Evans   Initial development
    // 3/1/99  P. Sharp  Added new attributes to views for Version 3.0
    // ---------------------------------------------
    // *********************************************
    // * This PAB populates both the Children/AP   *
    // * and the AR fields for display on the      *
    // * Participant screen.                       *
    // *********************************************
    // *********************************************
    // Find the AR
    // *********************************************
    if (ReadInterstateParticipant1())
    {
      local.CsePersonsWorkSet.FirstName =
        entities.InterstateParticipant.NameFirst ?? Spaces(12);
      local.CsePersonsWorkSet.LastName =
        entities.InterstateParticipant.NameLast ?? Spaces(17);
      local.CsePersonsWorkSet.MiddleInitial =
        entities.InterstateParticipant.NameMiddle ?? Spaces(1);
      UseSiFormatCsePersonName();
      export.ArName.Text33 = local.CsePersonsWorkSet.FormattedName;
      export.Ar.Assign(entities.InterstateParticipant);
    }
    else
    {
      ExitState = "AR_PARTICIPNT_NF";

      return;
    }

    // *********************************************
    // Find the additional AP(s)
    // *********************************************
    export.List.Index = -1;

    foreach(var item in ReadInterstateParticipant2())
    {
      local.CsePersonsWorkSet.FirstName =
        entities.InterstateParticipant.NameFirst ?? Spaces(12);
      local.CsePersonsWorkSet.LastName =
        entities.InterstateParticipant.NameLast ?? Spaces(17);
      local.CsePersonsWorkSet.MiddleInitial =
        entities.InterstateParticipant.NameMiddle ?? Spaces(1);
      UseSiFormatCsePersonName();

      ++export.List.Index;
      export.List.CheckSize();

      export.List.Update.Gnames.Text33 = local.CsePersonsWorkSet.FormattedName;
      MoveInterstateParticipant(entities.InterstateParticipant,
        export.List.Update.G);

      if (export.List.Index + 1 >= Export.ListGroup.Capacity)
      {
        return;
      }
    }

    // *********************************************
    // Find the Children
    // *********************************************
    foreach(var item in ReadInterstateParticipant3())
    {
      local.CsePersonsWorkSet.FirstName =
        entities.InterstateParticipant.NameFirst ?? Spaces(12);
      local.CsePersonsWorkSet.LastName =
        entities.InterstateParticipant.NameLast ?? Spaces(17);
      local.CsePersonsWorkSet.MiddleInitial =
        entities.InterstateParticipant.NameMiddle ?? Spaces(1);
      UseSiFormatCsePersonName();

      ++export.List.Index;
      export.List.CheckSize();

      export.List.Update.Gnames.Text33 = local.CsePersonsWorkSet.FormattedName;
      MoveInterstateParticipant(entities.InterstateParticipant,
        export.List.Update.G);

      if (export.List.Index + 1 >= Export.ListGroup.Capacity)
      {
        return;
      }
    }

    if (export.List.IsEmpty)
    {
      ExitState = "CSENET_REFERRAL_CHILDREN_NF";
    }
  }

  private static void MoveInterstateParticipant(InterstateParticipant source,
    InterstateParticipant target)
  {
    target.NameLast = source.NameLast;
    target.NameFirst = source.NameFirst;
    target.NameMiddle = source.NameMiddle;
    target.DateOfBirth = source.DateOfBirth;
    target.Ssn = source.Ssn;
    target.Sex = source.Sex;
    target.Race = source.Race;
    target.Relationship = source.Relationship;
    target.Status = source.Status;
    target.DependentRelationCp = source.DependentRelationCp;
    target.PlaceOfBirth = source.PlaceOfBirth;
    target.ChildStateOfResidence = source.ChildStateOfResidence;
    target.ChildPaternityStatus = source.ChildPaternityStatus;
  }

  private void UseSiFormatCsePersonName()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    local.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private bool ReadInterstateParticipant1()
  {
    entities.InterstateParticipant.Populated = false;

    return Read("ReadInterstateParticipant1",
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
        entities.InterstateParticipant.CcaTransactionDt = db.GetDate(reader, 0);
        entities.InterstateParticipant.SystemGeneratedSequenceNum =
          db.GetInt32(reader, 1);
        entities.InterstateParticipant.CcaTransSerNum = db.GetInt64(reader, 2);
        entities.InterstateParticipant.NameLast =
          db.GetNullableString(reader, 3);
        entities.InterstateParticipant.NameFirst =
          db.GetNullableString(reader, 4);
        entities.InterstateParticipant.NameMiddle =
          db.GetNullableString(reader, 5);
        entities.InterstateParticipant.NameSuffix =
          db.GetNullableString(reader, 6);
        entities.InterstateParticipant.DateOfBirth =
          db.GetNullableDate(reader, 7);
        entities.InterstateParticipant.Ssn = db.GetNullableString(reader, 8);
        entities.InterstateParticipant.Sex = db.GetNullableString(reader, 9);
        entities.InterstateParticipant.Race = db.GetNullableString(reader, 10);
        entities.InterstateParticipant.Relationship =
          db.GetNullableString(reader, 11);
        entities.InterstateParticipant.Status =
          db.GetNullableString(reader, 12);
        entities.InterstateParticipant.DependentRelationCp =
          db.GetNullableString(reader, 13);
        entities.InterstateParticipant.AddressLine1 =
          db.GetNullableString(reader, 14);
        entities.InterstateParticipant.AddressLine2 =
          db.GetNullableString(reader, 15);
        entities.InterstateParticipant.City = db.GetNullableString(reader, 16);
        entities.InterstateParticipant.State = db.GetNullableString(reader, 17);
        entities.InterstateParticipant.ZipCode5 =
          db.GetNullableString(reader, 18);
        entities.InterstateParticipant.ZipCode4 =
          db.GetNullableString(reader, 19);
        entities.InterstateParticipant.EmployerAddressLine1 =
          db.GetNullableString(reader, 20);
        entities.InterstateParticipant.EmployerAddressLine2 =
          db.GetNullableString(reader, 21);
        entities.InterstateParticipant.EmployerCity =
          db.GetNullableString(reader, 22);
        entities.InterstateParticipant.EmployerState =
          db.GetNullableString(reader, 23);
        entities.InterstateParticipant.EmployerZipCode5 =
          db.GetNullableString(reader, 24);
        entities.InterstateParticipant.EmployerZipCode4 =
          db.GetNullableString(reader, 25);
        entities.InterstateParticipant.EmployerName =
          db.GetNullableString(reader, 26);
        entities.InterstateParticipant.EmployerEin =
          db.GetNullableInt32(reader, 27);
        entities.InterstateParticipant.AddressVerifiedDate =
          db.GetNullableDate(reader, 28);
        entities.InterstateParticipant.EmployerVerifiedDate =
          db.GetNullableDate(reader, 29);
        entities.InterstateParticipant.WorkPhone =
          db.GetNullableString(reader, 30);
        entities.InterstateParticipant.WorkAreaCode =
          db.GetNullableString(reader, 31);
        entities.InterstateParticipant.PlaceOfBirth =
          db.GetNullableString(reader, 32);
        entities.InterstateParticipant.ChildStateOfResidence =
          db.GetNullableString(reader, 33);
        entities.InterstateParticipant.ChildPaternityStatus =
          db.GetNullableString(reader, 34);
        entities.InterstateParticipant.EmployerConfirmedInd =
          db.GetNullableString(reader, 35);
        entities.InterstateParticipant.AddressConfirmedInd =
          db.GetNullableString(reader, 36);
        entities.InterstateParticipant.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInterstateParticipant2()
  {
    entities.InterstateParticipant.Populated = false;

    return ReadEach("ReadInterstateParticipant2",
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
        entities.InterstateParticipant.CcaTransactionDt = db.GetDate(reader, 0);
        entities.InterstateParticipant.SystemGeneratedSequenceNum =
          db.GetInt32(reader, 1);
        entities.InterstateParticipant.CcaTransSerNum = db.GetInt64(reader, 2);
        entities.InterstateParticipant.NameLast =
          db.GetNullableString(reader, 3);
        entities.InterstateParticipant.NameFirst =
          db.GetNullableString(reader, 4);
        entities.InterstateParticipant.NameMiddle =
          db.GetNullableString(reader, 5);
        entities.InterstateParticipant.NameSuffix =
          db.GetNullableString(reader, 6);
        entities.InterstateParticipant.DateOfBirth =
          db.GetNullableDate(reader, 7);
        entities.InterstateParticipant.Ssn = db.GetNullableString(reader, 8);
        entities.InterstateParticipant.Sex = db.GetNullableString(reader, 9);
        entities.InterstateParticipant.Race = db.GetNullableString(reader, 10);
        entities.InterstateParticipant.Relationship =
          db.GetNullableString(reader, 11);
        entities.InterstateParticipant.Status =
          db.GetNullableString(reader, 12);
        entities.InterstateParticipant.DependentRelationCp =
          db.GetNullableString(reader, 13);
        entities.InterstateParticipant.AddressLine1 =
          db.GetNullableString(reader, 14);
        entities.InterstateParticipant.AddressLine2 =
          db.GetNullableString(reader, 15);
        entities.InterstateParticipant.City = db.GetNullableString(reader, 16);
        entities.InterstateParticipant.State = db.GetNullableString(reader, 17);
        entities.InterstateParticipant.ZipCode5 =
          db.GetNullableString(reader, 18);
        entities.InterstateParticipant.ZipCode4 =
          db.GetNullableString(reader, 19);
        entities.InterstateParticipant.EmployerAddressLine1 =
          db.GetNullableString(reader, 20);
        entities.InterstateParticipant.EmployerAddressLine2 =
          db.GetNullableString(reader, 21);
        entities.InterstateParticipant.EmployerCity =
          db.GetNullableString(reader, 22);
        entities.InterstateParticipant.EmployerState =
          db.GetNullableString(reader, 23);
        entities.InterstateParticipant.EmployerZipCode5 =
          db.GetNullableString(reader, 24);
        entities.InterstateParticipant.EmployerZipCode4 =
          db.GetNullableString(reader, 25);
        entities.InterstateParticipant.EmployerName =
          db.GetNullableString(reader, 26);
        entities.InterstateParticipant.EmployerEin =
          db.GetNullableInt32(reader, 27);
        entities.InterstateParticipant.AddressVerifiedDate =
          db.GetNullableDate(reader, 28);
        entities.InterstateParticipant.EmployerVerifiedDate =
          db.GetNullableDate(reader, 29);
        entities.InterstateParticipant.WorkPhone =
          db.GetNullableString(reader, 30);
        entities.InterstateParticipant.WorkAreaCode =
          db.GetNullableString(reader, 31);
        entities.InterstateParticipant.PlaceOfBirth =
          db.GetNullableString(reader, 32);
        entities.InterstateParticipant.ChildStateOfResidence =
          db.GetNullableString(reader, 33);
        entities.InterstateParticipant.ChildPaternityStatus =
          db.GetNullableString(reader, 34);
        entities.InterstateParticipant.EmployerConfirmedInd =
          db.GetNullableString(reader, 35);
        entities.InterstateParticipant.AddressConfirmedInd =
          db.GetNullableString(reader, 36);
        entities.InterstateParticipant.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadInterstateParticipant3()
  {
    entities.InterstateParticipant.Populated = false;

    return ReadEach("ReadInterstateParticipant3",
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
        entities.InterstateParticipant.CcaTransactionDt = db.GetDate(reader, 0);
        entities.InterstateParticipant.SystemGeneratedSequenceNum =
          db.GetInt32(reader, 1);
        entities.InterstateParticipant.CcaTransSerNum = db.GetInt64(reader, 2);
        entities.InterstateParticipant.NameLast =
          db.GetNullableString(reader, 3);
        entities.InterstateParticipant.NameFirst =
          db.GetNullableString(reader, 4);
        entities.InterstateParticipant.NameMiddle =
          db.GetNullableString(reader, 5);
        entities.InterstateParticipant.NameSuffix =
          db.GetNullableString(reader, 6);
        entities.InterstateParticipant.DateOfBirth =
          db.GetNullableDate(reader, 7);
        entities.InterstateParticipant.Ssn = db.GetNullableString(reader, 8);
        entities.InterstateParticipant.Sex = db.GetNullableString(reader, 9);
        entities.InterstateParticipant.Race = db.GetNullableString(reader, 10);
        entities.InterstateParticipant.Relationship =
          db.GetNullableString(reader, 11);
        entities.InterstateParticipant.Status =
          db.GetNullableString(reader, 12);
        entities.InterstateParticipant.DependentRelationCp =
          db.GetNullableString(reader, 13);
        entities.InterstateParticipant.AddressLine1 =
          db.GetNullableString(reader, 14);
        entities.InterstateParticipant.AddressLine2 =
          db.GetNullableString(reader, 15);
        entities.InterstateParticipant.City = db.GetNullableString(reader, 16);
        entities.InterstateParticipant.State = db.GetNullableString(reader, 17);
        entities.InterstateParticipant.ZipCode5 =
          db.GetNullableString(reader, 18);
        entities.InterstateParticipant.ZipCode4 =
          db.GetNullableString(reader, 19);
        entities.InterstateParticipant.EmployerAddressLine1 =
          db.GetNullableString(reader, 20);
        entities.InterstateParticipant.EmployerAddressLine2 =
          db.GetNullableString(reader, 21);
        entities.InterstateParticipant.EmployerCity =
          db.GetNullableString(reader, 22);
        entities.InterstateParticipant.EmployerState =
          db.GetNullableString(reader, 23);
        entities.InterstateParticipant.EmployerZipCode5 =
          db.GetNullableString(reader, 24);
        entities.InterstateParticipant.EmployerZipCode4 =
          db.GetNullableString(reader, 25);
        entities.InterstateParticipant.EmployerName =
          db.GetNullableString(reader, 26);
        entities.InterstateParticipant.EmployerEin =
          db.GetNullableInt32(reader, 27);
        entities.InterstateParticipant.AddressVerifiedDate =
          db.GetNullableDate(reader, 28);
        entities.InterstateParticipant.EmployerVerifiedDate =
          db.GetNullableDate(reader, 29);
        entities.InterstateParticipant.WorkPhone =
          db.GetNullableString(reader, 30);
        entities.InterstateParticipant.WorkAreaCode =
          db.GetNullableString(reader, 31);
        entities.InterstateParticipant.PlaceOfBirth =
          db.GetNullableString(reader, 32);
        entities.InterstateParticipant.ChildStateOfResidence =
          db.GetNullableString(reader, 33);
        entities.InterstateParticipant.ChildPaternityStatus =
          db.GetNullableString(reader, 34);
        entities.InterstateParticipant.EmployerConfirmedInd =
          db.GetNullableString(reader, 35);
        entities.InterstateParticipant.AddressConfirmedInd =
          db.GetNullableString(reader, 36);
        entities.InterstateParticipant.Populated = true;

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
    /// <summary>A ListGroup group.</summary>
    [Serializable]
    public class ListGroup
    {
      /// <summary>
      /// A value of Gnames.
      /// </summary>
      [JsonPropertyName("gnames")]
      public WorkArea Gnames
      {
        get => gnames ??= new();
        set => gnames = value;
      }

      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public InterstateParticipant G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private WorkArea gnames;
      private InterstateParticipant g;
    }

    /// <summary>
    /// A value of ArName.
    /// </summary>
    [JsonPropertyName("arName")]
    public WorkArea ArName
    {
      get => arName ??= new();
      set => arName = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public InterstateParticipant Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// Gets a value of List.
    /// </summary>
    [JsonIgnore]
    public Array<ListGroup> List => list ??= new(ListGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of List for json serialization.
    /// </summary>
    [JsonPropertyName("list")]
    [Computed]
    public IList<ListGroup> List_Json
    {
      get => list;
      set => List.Assign(value);
    }

    private WorkArea arName;
    private InterstateParticipant ar;
    private Array<ListGroup> list;
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
    /// A value of InterstateParticipant.
    /// </summary>
    [JsonPropertyName("interstateParticipant")]
    public InterstateParticipant InterstateParticipant
    {
      get => interstateParticipant ??= new();
      set => interstateParticipant = value;
    }

    private InterstateCase interstateCase;
    private InterstateParticipant interstateParticipant;
  }
#endregion
}
