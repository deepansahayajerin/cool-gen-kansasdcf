// Program: FN_DETERMINE_PGM_FOR_DEBT_DETAIL, ID: 372117066, model: 746.
// Short name: SWE02248
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DETERMINE_PGM_FOR_DEBT_DETAIL.
/// </summary>
[Serializable]
public partial class FnDeterminePgmForDebtDetail: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DETERMINE_PGM_FOR_DEBT_DETAIL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDeterminePgmForDebtDetail(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDeterminePgmForDebtDetail.
  /// </summary>
  public FnDeterminePgmForDebtDetail(IContext context, Import import,
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
    // : Set hardcoded values for Program.
    // *************************************************************
    //  Madhu Kumar       Changes for  PRWORA         05/22/00
    //  Madhu Kumar       JJA Enhancements            08/22/00
    // *************************************************************
    local.CollectionDtForNewRule.Date = new DateTime(2000, 10, 1);
    local.HardcodedAf.SystemGeneratedIdentifier = 2;
    local.HardcodedAfi.SystemGeneratedIdentifier = 14;
    local.HardcodedFc.SystemGeneratedIdentifier = 15;
    local.HardcodedFci.SystemGeneratedIdentifier = 16;
    local.HardcodedNa.SystemGeneratedIdentifier = 12;
    local.HardcodedNai.SystemGeneratedIdentifier = 18;
    local.HardcodedNc.SystemGeneratedIdentifier = 13;
    local.HardcodedNf.SystemGeneratedIdentifier = 3;
    local.HardcodedMai.SystemGeneratedIdentifier = 17;

    if (IsEmpty(import.HardcodedAccruing.Classification))
    {
      UseFnHardcodedDebtDistribution();
    }
    else
    {
      local.HardcodedAccruing.Classification =
        import.HardcodedAccruing.Classification;
    }

    // : Set up the Year/Month to use for determining if the Debt Detail is 
    // Current or Arrears.
    if (Equal(import.Collection.Date, local.Null1.Date))
    {
      local.Collection.Date = Now().Date;
    }
    else
    {
      local.Collection.Date = import.Collection.Date;
    }

    if (!Lt(local.Collection.Date, local.CollectionDtForNewRule.Date))
    {
      UseFnDeterminePgmForDbtDtl2();

      return;
    }

    local.Collection.YearMonth = UseCabGetYearMonthFromDate2();

    // : Accruing verses Non-Accruing types of Obligations are processed 
    // differently.  Determine the type and continue processing.
    if (AsChar(import.ObligationType.Classification) == AsChar
      (local.HardcodedAccruing.Classification))
    {
      local.DebtDue.Date = import.DebtDetail.DueDt;
    }
    else
    {
      local.DebtDue.Date = import.DebtDetail.CoveredPrdStartDt;
    }

    local.DebtDue.YearMonth = UseCabGetYearMonthFromDate1();

    // : Do not include any Person Program that has an Effective Date less than 
    // the Date of Emancipation for the Supported Person.
    local.DateOfEmancipation.Date = UseCabSetMaximumDiscontinueDate();

    foreach(var item in ReadCaseRole())
    {
      if (!Equal(entities.ExistingCaseRole.DateOfEmancipation, local.Null1.Date))
        
      {
        local.DateOfEmancipation.Date =
          entities.ExistingCaseRole.DateOfEmancipation;

        break;
      }
    }

    if (!Lt(local.DebtDue.Date, local.Collection.Date))
    {
      if (IsEmpty(import.DebtDetail.PreconversionProgramCode))
      {
        foreach(var item in ReadPersonProgramProgram3())
        {
          if (entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier == local
            .HardcodedFc.SystemGeneratedIdentifier)
          {
            local.HoldKeyOnly.SystemGeneratedIdentifier =
              entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier;

            goto Test;
          }
          else if (local.HoldKeyOnly.SystemGeneratedIdentifier == local
            .HardcodedAf.SystemGeneratedIdentifier)
          {
            if (entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier != local
              .HardcodedFc.SystemGeneratedIdentifier)
            {
              continue;
            }
          }
          else if (local.HoldKeyOnly.SystemGeneratedIdentifier == local
            .HardcodedNc.SystemGeneratedIdentifier)
          {
            if (entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier != local
              .HardcodedAf.SystemGeneratedIdentifier && entities
              .ExistingKeyOnlyProgram.SystemGeneratedIdentifier != local
              .HardcodedFc.SystemGeneratedIdentifier)
            {
              continue;
            }
          }
          else if (local.HoldKeyOnly.SystemGeneratedIdentifier == local
            .HardcodedNf.SystemGeneratedIdentifier)
          {
            if (entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier != local
              .HardcodedAf.SystemGeneratedIdentifier && entities
              .ExistingKeyOnlyProgram.SystemGeneratedIdentifier != local
              .HardcodedFc.SystemGeneratedIdentifier && entities
              .ExistingKeyOnlyProgram.SystemGeneratedIdentifier != local
              .HardcodedNc.SystemGeneratedIdentifier)
            {
              continue;
            }
          }

          local.HoldKeyOnly.SystemGeneratedIdentifier =
            entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier;
        }
      }
      else if (ReadProgram1())
      {
        export.Program.Assign(entities.ExistingProgram);

        return;
      }
      else
      {
        ExitState = "PROGRAM_NF_RB";

        return;
      }
    }
    else if (IsEmpty(import.DebtDetail.PreconversionProgramCode))
    {
      UseFnDeterminePgmUsingMatrix();
    }
    else
    {
      if (ReadProgram1())
      {
        local.HoldKeyOnly.SystemGeneratedIdentifier =
          entities.ExistingProgram.SystemGeneratedIdentifier;
      }
      else
      {
        ExitState = "PROGRAM_NF_RB";

        return;
      }

      UseFnDeterminePgmUsingMatrix();
    }

Test:

    if (local.HoldKeyOnly.SystemGeneratedIdentifier == 0)
    {
      if (ReadPersonProgramProgram1())
      {
        local.HoldKeyOnly.SystemGeneratedIdentifier =
          local.HardcodedNai.SystemGeneratedIdentifier;
      }
      else if (ReadPersonProgramProgram2())
      {
        local.HoldKeyOnly.SystemGeneratedIdentifier =
          local.HardcodedNa.SystemGeneratedIdentifier;
      }
      else if (AsChar(import.Obligation.OrderTypeCode) == 'K')
      {
        if (local.DebtDue.YearMonth >= local.Collection.YearMonth)
        {
          local.HoldKeyOnly.SystemGeneratedIdentifier =
            local.HardcodedNa.SystemGeneratedIdentifier;
        }
        else
        {
          local.HoldKeyOnly.SystemGeneratedIdentifier =
            local.HardcodedAf.SystemGeneratedIdentifier;
        }
      }
      else if (local.DebtDue.YearMonth >= local.Collection.YearMonth)
      {
        local.HoldKeyOnly.SystemGeneratedIdentifier =
          local.HardcodedNai.SystemGeneratedIdentifier;
      }
      else
      {
        local.HoldKeyOnly.SystemGeneratedIdentifier =
          local.HardcodedAfi.SystemGeneratedIdentifier;
      }
    }

    // : For 718B Obligation Types
    //   If we have determined the Program to be "NA", override the Program to 
    // be returned to "AF".
    if (import.ObligationType.SystemGeneratedIdentifier == 18 && local
      .HoldKeyOnly.SystemGeneratedIdentifier == local
      .HardcodedNa.SystemGeneratedIdentifier)
    {
      local.HoldKeyOnly.SystemGeneratedIdentifier =
        local.HardcodedAf.SystemGeneratedIdentifier;
    }

    UseFnDetermineProgramState();

    if (ReadProgram2())
    {
      export.Program.Assign(entities.ExistingProgram);
    }
    else
    {
      ExitState = "PROGRAM_NF_RB";
    }
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.YearMonth = source.YearMonth;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Classification = source.Classification;
  }

  private int UseCabGetYearMonthFromDate1()
  {
    var useImport = new CabGetYearMonthFromDate.Import();
    var useExport = new CabGetYearMonthFromDate.Export();

    useImport.DateWorkArea.Date = local.DebtDue.Date;

    Call(CabGetYearMonthFromDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.YearMonth;
  }

  private int UseCabGetYearMonthFromDate2()
  {
    var useImport = new CabGetYearMonthFromDate.Import();
    var useExport = new CabGetYearMonthFromDate.Export();

    useImport.DateWorkArea.Date = local.Collection.Date;

    Call(CabGetYearMonthFromDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.YearMonth;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnDeterminePgmForDbtDtl2()
  {
    var useImport = new FnDeterminePgmForDbtDtl2.Import();
    var useExport = new FnDeterminePgmForDbtDtl2.Export();

    useImport.SupportedPerson.Number = import.SupportedPerson.Number;
    MoveObligationType(import.ObligationType, useImport.ObligationType);
    useImport.Obligation.OrderTypeCode = import.Obligation.OrderTypeCode;
    useImport.DebtDetail.Assign(import.DebtDetail);
    useImport.HardcodedAccruing.Classification =
      import.HardcodedAccruing.Classification;
    useImport.Collection.Date = import.Collection.Date;

    Call(FnDeterminePgmForDbtDtl2.Execute, useImport, useExport);

    export.Program.Assign(useExport.Program);
    export.DprProgram.ProgramState = useExport.DprProgram.ProgramState;
  }

  private void UseFnDeterminePgmUsingMatrix()
  {
    var useImport = new FnDeterminePgmUsingMatrix.Import();
    var useExport = new FnDeterminePgmUsingMatrix.Export();

    useImport.SupportedPerson.Number = import.SupportedPerson.Number;
    useImport.Obligation.OrderTypeCode = import.Obligation.OrderTypeCode;
    useImport.DebtDetail.Assign(import.DebtDetail);
    useImport.KeyOnly.SystemGeneratedIdentifier =
      local.HoldKeyOnly.SystemGeneratedIdentifier;
    MoveDateWorkArea(local.Collection, useImport.Collection);
    MoveDateWorkArea(local.DebtDue, useImport.DebtDue);
    useImport.DateOfEmancipation.Date = local.DateOfEmancipation.Date;

    Call(FnDeterminePgmUsingMatrix.Execute, useImport, useExport);

    local.HoldKeyOnly.SystemGeneratedIdentifier =
      useExport.KeyOnly.SystemGeneratedIdentifier;
  }

  private void UseFnDetermineProgramState()
  {
    var useImport = new FnDetermineProgramState.Import();
    var useExport = new FnDetermineProgramState.Export();

    useImport.KeyOnly.SystemGeneratedIdentifier =
      local.HoldKeyOnly.SystemGeneratedIdentifier;

    Call(FnDetermineProgramState.Execute, useImport, useExport);

    export.DprProgram.ProgramState = useExport.DprProgram.ProgramState;
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    local.HardcodedAccruing.Classification =
      useExport.OtCAccruingClassification.Classification;
  }

  private IEnumerable<bool> ReadCaseRole()
  {
    entities.ExistingCaseRole.Populated = false;

    return ReadEach("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.SupportedPerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ExistingCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ExistingCaseRole.Type1 = db.GetString(reader, 2);
        entities.ExistingCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ExistingCaseRole.DateOfEmancipation =
          db.GetNullableDate(reader, 4);
        entities.ExistingCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingCaseRole.Type1);

        return true;
      });
  }

  private bool ReadPersonProgramProgram1()
  {
    entities.ExistingKeyOnlyProgram.Populated = false;
    entities.ExistingPersonProgram.Populated = false;

    return Read("ReadPersonProgramProgram1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.SupportedPerson.Number);
        db.SetNullableDate(
          command, "discontinueDate", local.DebtDue.Date.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate",
          local.DateOfEmancipation.Date.GetValueOrDefault());
        db.SetInt32(
          command, "prgGeneratedId",
          local.HardcodedMai.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingPersonProgram.CspNumber = db.GetString(reader, 0);
        entities.ExistingPersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.ExistingPersonProgram.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.ExistingPersonProgram.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.ExistingPersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingKeyOnlyProgram.Populated = true;
        entities.ExistingPersonProgram.Populated = true;
      });
  }

  private bool ReadPersonProgramProgram2()
  {
    entities.ExistingKeyOnlyProgram.Populated = false;
    entities.ExistingPersonProgram.Populated = false;

    return Read("ReadPersonProgramProgram2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.SupportedPerson.Number);
        db.SetDate(
          command, "effectiveDate1", local.DebtDue.Date.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate2",
          local.DateOfEmancipation.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingPersonProgram.CspNumber = db.GetString(reader, 0);
        entities.ExistingPersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.ExistingPersonProgram.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.ExistingPersonProgram.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.ExistingPersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingKeyOnlyProgram.Populated = true;
        entities.ExistingPersonProgram.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPersonProgramProgram3()
  {
    entities.ExistingKeyOnlyProgram.Populated = false;
    entities.ExistingPersonProgram.Populated = false;

    return ReadEach("ReadPersonProgramProgram3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.SupportedPerson.Number);
        db.SetDate(
          command, "effectiveDate1", local.DebtDue.Date.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate2",
          local.DateOfEmancipation.Date.GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedIdentifier1",
          local.HardcodedAf.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier2",
          local.HardcodedFc.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier3",
          local.HardcodedNa.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier4",
          local.HardcodedNf.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier5",
          local.HardcodedNc.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier6",
          local.HardcodedAfi.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier7",
          local.HardcodedFci.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier8",
          local.HardcodedNai.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingPersonProgram.CspNumber = db.GetString(reader, 0);
        entities.ExistingPersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.ExistingPersonProgram.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.ExistingPersonProgram.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.ExistingPersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.ExistingKeyOnlyProgram.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingKeyOnlyProgram.Populated = true;
        entities.ExistingPersonProgram.Populated = true;

        return true;
      });
  }

  private bool ReadProgram1()
  {
    entities.ExistingProgram.Populated = false;

    return Read("ReadProgram1",
      (db, command) =>
      {
        db.SetString(
          command, "code", import.DebtDetail.PreconversionProgramCode ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingProgram.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingProgram.Code = db.GetString(reader, 1);
        entities.ExistingProgram.InterstateIndicator = db.GetString(reader, 2);
        entities.ExistingProgram.Populated = true;
      });
  }

  private bool ReadProgram2()
  {
    entities.ExistingProgram.Populated = false;

    return Read("ReadProgram2",
      (db, command) =>
      {
        db.SetInt32(
          command, "programId", local.HoldKeyOnly.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingProgram.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingProgram.Code = db.GetString(reader, 1);
        entities.ExistingProgram.InterstateIndicator = db.GetString(reader, 2);
        entities.ExistingProgram.Populated = true;
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
    /// A value of SupportedPerson.
    /// </summary>
    [JsonPropertyName("supportedPerson")]
    public CsePerson SupportedPerson
    {
      get => supportedPerson ??= new();
      set => supportedPerson = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of HardcodedAccruing.
    /// </summary>
    [JsonPropertyName("hardcodedAccruing")]
    public ObligationType HardcodedAccruing
    {
      get => hardcodedAccruing ??= new();
      set => hardcodedAccruing = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public DateWorkArea Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    private CsePerson supportedPerson;
    private ObligationType obligationType;
    private Obligation obligation;
    private DebtDetail debtDetail;
    private ObligationType hardcodedAccruing;
    private DateWorkArea collection;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of DprProgram.
    /// </summary>
    [JsonPropertyName("dprProgram")]
    public DprProgram DprProgram
    {
      get => dprProgram ??= new();
      set => dprProgram = value;
    }

    private Program program;
    private DprProgram dprProgram;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CollectionDtForNewRule.
    /// </summary>
    [JsonPropertyName("collectionDtForNewRule")]
    public DateWorkArea CollectionDtForNewRule
    {
      get => collectionDtForNewRule ??= new();
      set => collectionDtForNewRule = value;
    }

    /// <summary>
    /// A value of Hold.
    /// </summary>
    [JsonPropertyName("hold")]
    public PersonProgram Hold
    {
      get => hold ??= new();
      set => hold = value;
    }

    /// <summary>
    /// A value of HoldKeyOnly.
    /// </summary>
    [JsonPropertyName("holdKeyOnly")]
    public Program HoldKeyOnly
    {
      get => holdKeyOnly ??= new();
      set => holdKeyOnly = value;
    }

    /// <summary>
    /// A value of DebtDue.
    /// </summary>
    [JsonPropertyName("debtDue")]
    public DateWorkArea DebtDue
    {
      get => debtDue ??= new();
      set => debtDue = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public DateWorkArea Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of HardcodedAccruing.
    /// </summary>
    [JsonPropertyName("hardcodedAccruing")]
    public ObligationType HardcodedAccruing
    {
      get => hardcodedAccruing ??= new();
      set => hardcodedAccruing = value;
    }

    /// <summary>
    /// A value of HardcodedAf.
    /// </summary>
    [JsonPropertyName("hardcodedAf")]
    public Program HardcodedAf
    {
      get => hardcodedAf ??= new();
      set => hardcodedAf = value;
    }

    /// <summary>
    /// A value of HardcodedAfi.
    /// </summary>
    [JsonPropertyName("hardcodedAfi")]
    public Program HardcodedAfi
    {
      get => hardcodedAfi ??= new();
      set => hardcodedAfi = value;
    }

    /// <summary>
    /// A value of HardcodedFc.
    /// </summary>
    [JsonPropertyName("hardcodedFc")]
    public Program HardcodedFc
    {
      get => hardcodedFc ??= new();
      set => hardcodedFc = value;
    }

    /// <summary>
    /// A value of HardcodedFci.
    /// </summary>
    [JsonPropertyName("hardcodedFci")]
    public Program HardcodedFci
    {
      get => hardcodedFci ??= new();
      set => hardcodedFci = value;
    }

    /// <summary>
    /// A value of HardcodedNa.
    /// </summary>
    [JsonPropertyName("hardcodedNa")]
    public Program HardcodedNa
    {
      get => hardcodedNa ??= new();
      set => hardcodedNa = value;
    }

    /// <summary>
    /// A value of HardcodedNai.
    /// </summary>
    [JsonPropertyName("hardcodedNai")]
    public Program HardcodedNai
    {
      get => hardcodedNai ??= new();
      set => hardcodedNai = value;
    }

    /// <summary>
    /// A value of HardcodedNc.
    /// </summary>
    [JsonPropertyName("hardcodedNc")]
    public Program HardcodedNc
    {
      get => hardcodedNc ??= new();
      set => hardcodedNc = value;
    }

    /// <summary>
    /// A value of HardcodedNf.
    /// </summary>
    [JsonPropertyName("hardcodedNf")]
    public Program HardcodedNf
    {
      get => hardcodedNf ??= new();
      set => hardcodedNf = value;
    }

    /// <summary>
    /// A value of HardcodedMai.
    /// </summary>
    [JsonPropertyName("hardcodedMai")]
    public Program HardcodedMai
    {
      get => hardcodedMai ??= new();
      set => hardcodedMai = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of DateOfEmancipation.
    /// </summary>
    [JsonPropertyName("dateOfEmancipation")]
    public DateWorkArea DateOfEmancipation
    {
      get => dateOfEmancipation ??= new();
      set => dateOfEmancipation = value;
    }

    private DateWorkArea collectionDtForNewRule;
    private PersonProgram hold;
    private Program holdKeyOnly;
    private DateWorkArea debtDue;
    private DateWorkArea collection;
    private ObligationType hardcodedAccruing;
    private Program hardcodedAf;
    private Program hardcodedAfi;
    private Program hardcodedFc;
    private Program hardcodedFci;
    private Program hardcodedNa;
    private Program hardcodedNai;
    private Program hardcodedNc;
    private Program hardcodedNf;
    private Program hardcodedMai;
    private DateWorkArea null1;
    private DateWorkArea dateOfEmancipation;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingProgram.
    /// </summary>
    [JsonPropertyName("existingProgram")]
    public Program ExistingProgram
    {
      get => existingProgram ??= new();
      set => existingProgram = value;
    }

    /// <summary>
    /// A value of ExistingKeyOnlyCsePerson.
    /// </summary>
    [JsonPropertyName("existingKeyOnlyCsePerson")]
    public CsePerson ExistingKeyOnlyCsePerson
    {
      get => existingKeyOnlyCsePerson ??= new();
      set => existingKeyOnlyCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingKeyOnlyProgram.
    /// </summary>
    [JsonPropertyName("existingKeyOnlyProgram")]
    public Program ExistingKeyOnlyProgram
    {
      get => existingKeyOnlyProgram ??= new();
      set => existingKeyOnlyProgram = value;
    }

    /// <summary>
    /// A value of ExistingPersonProgram.
    /// </summary>
    [JsonPropertyName("existingPersonProgram")]
    public PersonProgram ExistingPersonProgram
    {
      get => existingPersonProgram ??= new();
      set => existingPersonProgram = value;
    }

    /// <summary>
    /// A value of ExistingCaseRole.
    /// </summary>
    [JsonPropertyName("existingCaseRole")]
    public CaseRole ExistingCaseRole
    {
      get => existingCaseRole ??= new();
      set => existingCaseRole = value;
    }

    private Program existingProgram;
    private CsePerson existingKeyOnlyCsePerson;
    private Program existingKeyOnlyProgram;
    private PersonProgram existingPersonProgram;
    private CaseRole existingCaseRole;
  }
#endregion
}
