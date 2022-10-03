// Program: OE_READ_MILITARY_SERVICE, ID: 371920135, model: 746.
// Short name: SWE00958
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_READ_MILITARY_SERVICE.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// WRITTEN BY:SID
/// </para>
/// </summary>
[Serializable]
public partial class OeReadMilitaryService: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_READ_MILITARY_SERVICE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeReadMilitaryService(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeReadMilitaryService.
  /// </summary>
  public OeReadMilitaryService(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.CsePerson.Number = import.CsePerson.Number;
    export.MilitaryService.EffectiveDate = import.MilitaryService.EffectiveDate;
    UseOeCabSetMnemonics();

    if (ReadCsePerson())
    {
      export.CsePerson.Number = entities.ExistingCsePerson.Number;
    }
    else
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    if (ReadMilitaryService())
    {
      ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
      export.MilitaryService.Assign(entities.ExistingMilitaryService);
      local.CodeValue.Cdvalue = export.MilitaryService.DutyStatusCode ?? Spaces
        (10);
      UseCabGetCodeValueDescription3();
      local.CodeValue.Cdvalue = export.MilitaryService.BranchCode ?? Spaces(10);
      UseCabGetCodeValueDescription1();
      local.CodeValue.Cdvalue = export.MilitaryService.Rank ?? Spaces(10);
      UseCabGetCodeValueDescription2();
    }
    else
    {
      ExitState = "MILITARY_SERVICE_NF";

      return;
    }

    if (ReadIncomeSource())
    {
      MoveIncomeSource(entities.ExistingIncomeSource, export.IncomeSource);

      if (ReadPersonIncomeHistory())
      {
        MovePersonIncomeHistory(entities.ExistingPersonIncomeHistory,
          export.PersonIncomeHistory);
      }
    }
  }

  private static void MoveIncomeSource(IncomeSource source, IncomeSource target)
  {
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
  }

  private static void MovePersonIncomeHistory(PersonIncomeHistory source,
    PersonIncomeHistory target)
  {
    target.Identifier = source.Identifier;
    target.IncomeAmt = source.IncomeAmt;
    target.CheckMonthlyAmount = source.CheckMonthlyAmount;
    target.MilitaryBaqAllotment = source.MilitaryBaqAllotment;
  }

  private void UseCabGetCodeValueDescription1()
  {
    var useImport = new CabGetCodeValueDescription.Import();
    var useExport = new CabGetCodeValueDescription.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.MilitaryBranch.CodeName;

    Call(CabGetCodeValueDescription.Execute, useImport, useExport);

    export.MilitaryBranch.Description = useExport.CodeValue.Description;
  }

  private void UseCabGetCodeValueDescription2()
  {
    var useImport = new CabGetCodeValueDescription.Import();
    var useExport = new CabGetCodeValueDescription.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.MilitaryRank.CodeName;

    Call(CabGetCodeValueDescription.Execute, useImport, useExport);

    export.MilitaryRank.Description = useExport.CodeValue.Description;
  }

  private void UseCabGetCodeValueDescription3()
  {
    var useImport = new CabGetCodeValueDescription.Import();
    var useExport = new CabGetCodeValueDescription.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.MilitaryDutyStatus.CodeName;

    Call(CabGetCodeValueDescription.Execute, useImport, useExport);

    export.MilitaryDutyStatus.Description = useExport.CodeValue.Description;
  }

  private void UseOeCabSetMnemonics()
  {
    var useImport = new OeCabSetMnemonics.Import();
    var useExport = new OeCabSetMnemonics.Export();

    Call(OeCabSetMnemonics.Execute, useImport, useExport);

    local.MilitaryBranch.CodeName = useExport.MilitaryBranch.CodeName;
    local.MilitaryRank.CodeName = useExport.MilitaryRank.CodeName;
    local.MilitaryDutyStatus.CodeName = useExport.MilitaryDutyStatus.CodeName;
  }

  private bool ReadCsePerson()
  {
    entities.ExistingCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingCsePerson.Populated = true;
      });
  }

  private bool ReadIncomeSource()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingMilitaryService.Populated);
    entities.ExistingIncomeSource.Populated = false;

    return Read("ReadIncomeSource",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "mseEffectiveDate",
          entities.ExistingMilitaryService.EffectiveDate.GetValueOrDefault());
        db.SetNullableString(
          command, "cspSNumber", entities.ExistingMilitaryService.CspNumber);
        db.SetString(command, "cspINumber", entities.ExistingCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingIncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.ExistingIncomeSource.Type1 = db.GetString(reader, 1);
        entities.ExistingIncomeSource.CspINumber = db.GetString(reader, 2);
        entities.ExistingIncomeSource.MseEffectiveDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingIncomeSource.CspSNumber =
          db.GetNullableString(reader, 4);
        entities.ExistingIncomeSource.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.ExistingIncomeSource.Type1);
      });
  }

  private bool ReadMilitaryService()
  {
    entities.ExistingMilitaryService.Populated = false;

    return Read("ReadMilitaryService",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetDate(
          command, "effectiveDate",
          import.MilitaryService.EffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingMilitaryService.EffectiveDate = db.GetDate(reader, 0);
        entities.ExistingMilitaryService.CspNumber = db.GetString(reader, 1);
        entities.ExistingMilitaryService.StartDate =
          db.GetNullableDate(reader, 2);
        entities.ExistingMilitaryService.Street1 =
          db.GetNullableString(reader, 3);
        entities.ExistingMilitaryService.Street2 =
          db.GetNullableString(reader, 4);
        entities.ExistingMilitaryService.City = db.GetNullableString(reader, 5);
        entities.ExistingMilitaryService.State =
          db.GetNullableString(reader, 6);
        entities.ExistingMilitaryService.ZipCode5 =
          db.GetNullableString(reader, 7);
        entities.ExistingMilitaryService.ZipCode4 =
          db.GetNullableString(reader, 8);
        entities.ExistingMilitaryService.Zip3 = db.GetNullableString(reader, 9);
        entities.ExistingMilitaryService.Country =
          db.GetNullableString(reader, 10);
        entities.ExistingMilitaryService.Apo = db.GetNullableString(reader, 11);
        entities.ExistingMilitaryService.ExpectedReturnDateToStates =
          db.GetNullableDate(reader, 12);
        entities.ExistingMilitaryService.OverseasDutyStation =
          db.GetNullableString(reader, 13);
        entities.ExistingMilitaryService.ExpectedDischargeDate =
          db.GetNullableDate(reader, 14);
        entities.ExistingMilitaryService.Phone =
          db.GetNullableInt32(reader, 15);
        entities.ExistingMilitaryService.BranchCode =
          db.GetNullableString(reader, 16);
        entities.ExistingMilitaryService.CommandingOfficerLastName =
          db.GetNullableString(reader, 17);
        entities.ExistingMilitaryService.CommandingOfficerFirstName =
          db.GetNullableString(reader, 18);
        entities.ExistingMilitaryService.CommandingOfficerMi =
          db.GetNullableString(reader, 19);
        entities.ExistingMilitaryService.CurrentUsDutyStation =
          db.GetNullableString(reader, 20);
        entities.ExistingMilitaryService.DutyStatusCode =
          db.GetNullableString(reader, 21);
        entities.ExistingMilitaryService.Rank =
          db.GetNullableString(reader, 22);
        entities.ExistingMilitaryService.EndDate =
          db.GetNullableDate(reader, 23);
        entities.ExistingMilitaryService.PhoneCountryCode =
          db.GetNullableString(reader, 24);
        entities.ExistingMilitaryService.PhoneExt =
          db.GetNullableString(reader, 25);
        entities.ExistingMilitaryService.PhoneAreaCode =
          db.GetNullableInt32(reader, 26);
        entities.ExistingMilitaryService.Populated = true;
      });
  }

  private bool ReadPersonIncomeHistory()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingIncomeSource.Populated);
    entities.ExistingPersonIncomeHistory.Populated = false;

    return Read("ReadPersonIncomeHistory",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetString(
          command, "cspINumber", entities.ExistingIncomeSource.CspINumber);
        db.SetDateTime(
          command, "isrIdentifier",
          entities.ExistingIncomeSource.Identifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingPersonIncomeHistory.CspNumber =
          db.GetString(reader, 0);
        entities.ExistingPersonIncomeHistory.IsrIdentifier =
          db.GetDateTime(reader, 1);
        entities.ExistingPersonIncomeHistory.Identifier =
          db.GetDateTime(reader, 2);
        entities.ExistingPersonIncomeHistory.IncomeAmt =
          db.GetNullableDecimal(reader, 3);
        entities.ExistingPersonIncomeHistory.CheckMonthlyAmount =
          db.GetNullableDecimal(reader, 4);
        entities.ExistingPersonIncomeHistory.CspINumber =
          db.GetString(reader, 5);
        entities.ExistingPersonIncomeHistory.MilitaryBaqAllotment =
          db.GetNullableDecimal(reader, 6);
        entities.ExistingPersonIncomeHistory.Populated = true;
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
    /// A value of MilitaryService.
    /// </summary>
    [JsonPropertyName("militaryService")]
    public MilitaryService MilitaryService
    {
      get => militaryService ??= new();
      set => militaryService = value;
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

    private MilitaryService militaryService;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of PersonIncomeHistory.
    /// </summary>
    [JsonPropertyName("personIncomeHistory")]
    public PersonIncomeHistory PersonIncomeHistory
    {
      get => personIncomeHistory ??= new();
      set => personIncomeHistory = value;
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
    /// A value of MilitaryDutyStatus.
    /// </summary>
    [JsonPropertyName("militaryDutyStatus")]
    public CodeValue MilitaryDutyStatus
    {
      get => militaryDutyStatus ??= new();
      set => militaryDutyStatus = value;
    }

    /// <summary>
    /// A value of MilitaryBranch.
    /// </summary>
    [JsonPropertyName("militaryBranch")]
    public CodeValue MilitaryBranch
    {
      get => militaryBranch ??= new();
      set => militaryBranch = value;
    }

    /// <summary>
    /// A value of MilitaryRank.
    /// </summary>
    [JsonPropertyName("militaryRank")]
    public CodeValue MilitaryRank
    {
      get => militaryRank ??= new();
      set => militaryRank = value;
    }

    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public MilitaryService Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// A value of WorkH.
    /// </summary>
    [JsonPropertyName("workH")]
    public ScrollingAttributes WorkH
    {
      get => workH ??= new();
      set => workH = value;
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

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private PersonIncomeHistory personIncomeHistory;
    private IncomeSource incomeSource;
    private CodeValue militaryDutyStatus;
    private CodeValue militaryBranch;
    private CodeValue militaryRank;
    private MilitaryService next;
    private ScrollingAttributes workH;
    private MilitaryService militaryService;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    /// <summary>
    /// A value of MilitaryBranch.
    /// </summary>
    [JsonPropertyName("militaryBranch")]
    public Code MilitaryBranch
    {
      get => militaryBranch ??= new();
      set => militaryBranch = value;
    }

    /// <summary>
    /// A value of MilitaryRank.
    /// </summary>
    [JsonPropertyName("militaryRank")]
    public Code MilitaryRank
    {
      get => militaryRank ??= new();
      set => militaryRank = value;
    }

    /// <summary>
    /// A value of MilitaryDutyStatus.
    /// </summary>
    [JsonPropertyName("militaryDutyStatus")]
    public Code MilitaryDutyStatus
    {
      get => militaryDutyStatus ??= new();
      set => militaryDutyStatus = value;
    }

    private CodeValue codeValue;
    private Code militaryBranch;
    private Code militaryRank;
    private Code militaryDutyStatus;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingPersonIncomeHistory.
    /// </summary>
    [JsonPropertyName("existingPersonIncomeHistory")]
    public PersonIncomeHistory ExistingPersonIncomeHistory
    {
      get => existingPersonIncomeHistory ??= new();
      set => existingPersonIncomeHistory = value;
    }

    /// <summary>
    /// A value of ExistingIncomeSource.
    /// </summary>
    [JsonPropertyName("existingIncomeSource")]
    public IncomeSource ExistingIncomeSource
    {
      get => existingIncomeSource ??= new();
      set => existingIncomeSource = value;
    }

    /// <summary>
    /// A value of ExistingMilitaryService.
    /// </summary>
    [JsonPropertyName("existingMilitaryService")]
    public MilitaryService ExistingMilitaryService
    {
      get => existingMilitaryService ??= new();
      set => existingMilitaryService = value;
    }

    /// <summary>
    /// A value of ExistingCsePerson.
    /// </summary>
    [JsonPropertyName("existingCsePerson")]
    public CsePerson ExistingCsePerson
    {
      get => existingCsePerson ??= new();
      set => existingCsePerson = value;
    }

    private PersonIncomeHistory existingPersonIncomeHistory;
    private IncomeSource existingIncomeSource;
    private MilitaryService existingMilitaryService;
    private CsePerson existingCsePerson;
  }
#endregion
}
