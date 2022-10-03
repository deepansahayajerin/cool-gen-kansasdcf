// Program: LE_FDSO_CHECK_CHILD_18_OR_OVER, ID: 372665627, model: 746.
// Short name: SWE01658
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_FDSO_CHECK_CHILD_18_OR_OVER.
/// </summary>
[Serializable]
public partial class LeFdsoCheckChild18OrOver: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_FDSO_CHECK_CHILD_18_OR_OVER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeFdsoCheckChild18OrOver(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeFdsoCheckChild18OrOver.
  /// </summary>
  public LeFdsoCheckChild18OrOver(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *********************************************
    // *               MAINTENANCE LOG             *
    // *
    // 
    // *
    // *   DATE    DEVELOPER       DESCRIPTION     *
    // * 04/19/96   H HOOKS    INITIAL DEVELOPMENT *
    // * 04/11/01   Raj - PR# 114414 - Check for child (Active or Inactive), 18 
    // years old or *Emancipated whichever is earlier.
    // *********************************************
    export.Child18OrOver.Flag = "N";

    if (!Lt(local.ProgramProcessingInfo.ProcessDate,
      import.ProgramProcessingInfo.ProcessDate))
    {
      local.ProgramProcessingInfo.ProcessDate = Now().Date;
    }
    else
    {
      local.ProgramProcessingInfo.ProcessDate =
        import.ProgramProcessingInfo.ProcessDate;
    }

    if (ReadSupportedCsePerson())
    {
      if (ReadObligationType())
      {
        if (Equal(entities.ObligationType.Code, "SP") || Equal
          (entities.ObligationType.Code, "SAJ"))
        {
          // --- We are not interested in spousal support related debts.
        }
        else
        {
          // ----------------------------------------------------------
          // PR#114414
          // Following Read each statement is commented
          // because we don't have to check whether child is
          // active. The new read-each will look for active and inactive
          // child.
          // -----------------------------------------------------------
          foreach(var item in ReadChildCase())
          {
            if (ReadAbsentParentCsePerson())
            {
              // --- A cse case found with the obligor as the AP and the 
              // supported person as CH. Now check if the child is 18 or over
              local.Child.Number = entities.ExistingSupported.Number;
              UseSiReadCsePersonBatch();

              if (!IsEmpty(local.Child.FormattedName))
              {
                // ----------------------------------------------------------
                // PR#114414
                // Since DOB is not mandatory field, following IF
                // statement is commented.
                // -----------------------------------------------------------
                if (!Lt(local.ProgramProcessingInfo.ProcessDate,
                  AddYears(local.Child.Dob, 18)))
                {
                  export.Child18OrOver.Flag = "Y";

                  return;
                }
                else
                {
                  // ----------------------------------------------------------
                  // PR#114414
                  // Check for Emancipation date. If it is less than
                  // current date, that means child is emancipated before
                  // 18 years, set the flag and go back to calling cab.
                  // -----------------------------------------------------------
                  if (Lt(local.Null1.Dob, entities.Child.DateOfEmancipation))
                  {
                    if (!Lt(local.ProgramProcessingInfo.ProcessDate,
                      entities.Child.DateOfEmancipation))
                    {
                      export.Child18OrOver.Flag = "Y";

                      return;
                    }
                  }
                }
              }

              // --- Found the child but the child is not 18 or over.
              return;
            }
          }
        }
      }
    }
  }

  private void UseSiReadCsePersonBatch()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.Child.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    local.Child.Assign(useExport.CsePersonsWorkSet);
  }

  private bool ReadAbsentParentCsePerson()
  {
    System.Diagnostics.Debug.Assert(import.PersistantObligor.Populated);
    entities.ExistingObligorCsePerson.Populated = false;
    entities.AbsentParent.Populated = false;

    return Read("ReadAbsentParentCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", import.PersistantObligor.CspNumber);
        db.SetNullableDate(
          command, "startDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.AbsentParent.CasNumber = db.GetString(reader, 0);
        entities.AbsentParent.CspNumber = db.GetString(reader, 1);
        entities.ExistingObligorCsePerson.Number = db.GetString(reader, 1);
        entities.AbsentParent.Type1 = db.GetString(reader, 2);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 3);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 4);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 5);
        entities.ExistingObligorCsePerson.Populated = true;
        entities.AbsentParent.Populated = true;
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);
      });
  }

  private IEnumerable<bool> ReadChildCase()
  {
    entities.Case1.Populated = false;
    entities.Child.Populated = false;

    return ReadEach("ReadChildCase",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingSupported.Number);
      },
      (db, reader) =>
      {
        entities.Child.CasNumber = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Child.CspNumber = db.GetString(reader, 1);
        entities.Child.Type1 = db.GetString(reader, 2);
        entities.Child.Identifier = db.GetInt32(reader, 3);
        entities.Child.StartDate = db.GetNullableDate(reader, 4);
        entities.Child.EndDate = db.GetNullableDate(reader, 5);
        entities.Child.DateOfEmancipation = db.GetNullableDate(reader, 6);
        entities.Case1.Populated = true;
        entities.Child.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Child.Type1);

        return true;
      });
  }

  private bool ReadObligationType()
  {
    System.Diagnostics.Debug.Assert(import.PersistantDebt.Populated);
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(command, "debtTypId", import.PersistantDebt.OtyType);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Populated = true;
      });
  }

  private bool ReadSupportedCsePerson()
  {
    System.Diagnostics.Debug.Assert(import.PersistantDebt.Populated);
    entities.ExistingSupported.Populated = false;
    entities.Supported.Populated = false;

    return Read("ReadSupportedCsePerson",
      (db, command) =>
      {
        db.SetString(command, "type", import.PersistantDebt.CpaSupType ?? "");
        db.SetString(
          command, "cspNumber", import.PersistantDebt.CspSupNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Supported.CspNumber = db.GetString(reader, 0);
        entities.ExistingSupported.Number = db.GetString(reader, 0);
        entities.Supported.Type1 = db.GetString(reader, 1);
        entities.ExistingSupported.Populated = true;
        entities.Supported.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Supported.Type1);
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of PersistantObligor.
    /// </summary>
    [JsonPropertyName("persistantObligor")]
    public CsePersonAccount PersistantObligor
    {
      get => persistantObligor ??= new();
      set => persistantObligor = value;
    }

    /// <summary>
    /// A value of PersistantDebt.
    /// </summary>
    [JsonPropertyName("persistantDebt")]
    public ObligationTransaction PersistantDebt
    {
      get => persistantDebt ??= new();
      set => persistantDebt = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private CsePersonAccount persistantObligor;
    private ObligationTransaction persistantDebt;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Child18OrOver.
    /// </summary>
    [JsonPropertyName("child18OrOver")]
    public Common Child18OrOver
    {
      get => child18OrOver ??= new();
      set => child18OrOver = value;
    }

    private Common child18OrOver;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public CsePersonsWorkSet Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CsePersonsWorkSet Child
    {
      get => child ??= new();
      set => child = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private CsePersonsWorkSet null1;
    private CsePersonsWorkSet child;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of ApplicantRecipient.
    /// </summary>
    [JsonPropertyName("applicantRecipient")]
    public CaseRole ApplicantRecipient
    {
      get => applicantRecipient ??= new();
      set => applicantRecipient = value;
    }

    /// <summary>
    /// A value of ExistingObligorCsePerson.
    /// </summary>
    [JsonPropertyName("existingObligorCsePerson")]
    public CsePerson ExistingObligorCsePerson
    {
      get => existingObligorCsePerson ??= new();
      set => existingObligorCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingSupported.
    /// </summary>
    [JsonPropertyName("existingSupported")]
    public CsePerson ExistingSupported
    {
      get => existingSupported ??= new();
      set => existingSupported = value;
    }

    /// <summary>
    /// A value of ExistingObligorCase.
    /// </summary>
    [JsonPropertyName("existingObligorCase")]
    public Case1 ExistingObligorCase
    {
      get => existingObligorCase ??= new();
      set => existingObligorCase = value;
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
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePersonAccount Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    /// <summary>
    /// A value of AbsentParent.
    /// </summary>
    [JsonPropertyName("absentParent")]
    public CaseRole AbsentParent
    {
      get => absentParent ??= new();
      set => absentParent = value;
    }

    /// <summary>
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CaseRole Child
    {
      get => child ??= new();
      set => child = value;
    }

    private Obligation obligation;
    private ObligationType obligationType;
    private CaseRole applicantRecipient;
    private CsePerson existingObligorCsePerson;
    private CsePerson existingSupported;
    private Case1 existingObligorCase;
    private Case1 case1;
    private CsePersonAccount supported;
    private CaseRole absentParent;
    private CaseRole child;
  }
#endregion
}
