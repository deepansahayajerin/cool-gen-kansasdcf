// Program: LE_CHECK_GOOD_CAUSE_FOR_DEBT, ID: 372661532, model: 746.
// Short name: SWE00728
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
/// A program: LE_CHECK_GOOD_CAUSE_FOR_DEBT.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block returns Good Cause indicator and Interstate indicator.
///     Good Cause indicator returned will be:
///          Y	if PD or GC found
/// 	 N	otherwise
/// </para>
/// </summary>
[Serializable]
public partial class LeCheckGoodCauseForDebt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_CHECK_GOOD_CAUSE_FOR_DEBT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeCheckGoodCauseForDebt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeCheckGoodCauseForDebt.
  /// </summary>
  public LeCheckGoodCauseForDebt(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -----------------------------------------------------------------------
    // MAINTENANCE LOG
    // DATE    DEVELOPER       DESCRIPTION
    // 04/19/96   H HOOKS    INITIAL DEVELOPMENT
    // 12/12/96   govind	Modified to return incoming interstate
    // indicator also
    // 04/30/97   govind     Removed the logic to determine the
    // interstate ind using INTERSTATE REQUEST since it won't
    // work.
    // 01/08/99	P McElderry
    // Removed persistent views.
    // 04/13/99	PMcElderry
    // Fixed read on good cause for a non SP/SAJ obligation type;
    // added import views of obligor_cse_person, obligation_id;
    // added effective date qualifier on good_cause.
    // 06/18/99	PMcElderry
    // Recoded program.
    // 07/5/99	PMcElderry
    // Performance enhancements - used system generated ids
    // instead of SP/SAJ; added date qualifiers back into
    // READ/READ EACHes
    // 02/21/2001	WO000269	EShirk
    // Recoded program.   Removed unecessary read against supported person, 
    // obligation type, case, child  and applicant receipient.   Also changed
    // the read qualification from current date to PPI date.
    // 02/01/2002	PR134680	EShirk
    // Modified reads against good cause to read each descending and to treat PD
    // and GC as an active good cause situation.
    // **************************************************************************************
    // ** 12/29/2014    CQ42515   DDupree
    //  Added requirment on ar case role must be currenttly active.
    // **************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    export.GoodCauseFoundForDebt.Flag = "N";

    // **************************************************************************************
    // ** Get the supported person tied to the debt
    // **************************************************************************************
    if (ReadCsePerson())
    {
      // Continue
    }
    else
    {
      ExitState = "FN0000_SUPP_PERSON_ACCT_NF";

      return;
    }

    // **************************************************************************************
    // ** Is the obligation a spousal (SP)  or spousal arrears judgement (SAJ) 
    // type.
    // **************************************************************************************
    if (import.ObligationType.SystemGeneratedIdentifier == 2 || import
      .ObligationType.SystemGeneratedIdentifier == 17)
    {
      // **************************************************************************************
      // ** Added requirment on ar case role must be currenttly active. cq42515 
      // 12/29/2014
      // **************************************************************************************
      foreach(var item in ReadCaseRole2())
      {
        // **************************************************************************************
        // ** Determine if AR marked AP for good cause.
        // **************************************************************************************
        if (ReadGoodCause())
        {
          if (Equal(entities.GoodCause.Code, "PD") || Equal
            (entities.GoodCause.Code, "GC"))
          {
            export.GoodCauseFoundForDebt.Flag = "Y";

            return;
          }
        }
      }
    }
    else
    {
      // **************************************************************************************
      // ** Read each AR that is on the same case as the SP and AP.
      // **************************************************************************************
      // **************************************************************************************
      // ** Added requirment on ar case role must be currenttly active. cq42515 
      // 12/29/2014
      // **************************************************************************************
      foreach(var item in ReadCaseRole1())
      {
        // **************************************************************************************
        // ** Determine if AR marked AP for good cause.
        // **************************************************************************************
        if (ReadGoodCause())
        {
          if (Equal(entities.GoodCause.Code, "PD") || Equal
            (entities.GoodCause.Code, "GC"))
          {
            export.GoodCauseFoundForDebt.Flag = "Y";

            return;
          }
        }
      }
    }
  }

  private IEnumerable<bool> ReadCaseRole1()
  {
    entities.Ar.Populated = false;

    return ReadEach("ReadCaseRole1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", entities.SupportedCsePerson.Number);
        db.SetNullableDate(
          command, "startDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetString(command, "cspNumber2", import.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.Ar.CasNumber = db.GetString(reader, 0);
        entities.Ar.CspNumber = db.GetString(reader, 1);
        entities.Ar.Type1 = db.GetString(reader, 2);
        entities.Ar.Identifier = db.GetInt32(reader, 3);
        entities.Ar.StartDate = db.GetNullableDate(reader, 4);
        entities.Ar.EndDate = db.GetNullableDate(reader, 5);
        entities.Ar.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Ar.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRole2()
  {
    entities.Ar.Populated = false;

    return ReadEach("ReadCaseRole2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", entities.SupportedCsePerson.Number);
        db.SetString(command, "cspNumber2", import.Obligor.Number);
        db.SetNullableDate(
          command, "startDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Ar.CasNumber = db.GetString(reader, 0);
        entities.Ar.CspNumber = db.GetString(reader, 1);
        entities.Ar.Type1 = db.GetString(reader, 2);
        entities.Ar.Identifier = db.GetInt32(reader, 3);
        entities.Ar.StartDate = db.GetNullableDate(reader, 4);
        entities.Ar.EndDate = db.GetNullableDate(reader, 5);
        entities.Ar.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Ar.Type1);

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.SupportedCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetInt32(command, "obTrnId", import.Debt.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "obgGeneratedId",
          import.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Obligor.Number);
        db.SetInt32(
          command, "otyType", import.ObligationType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.SupportedCsePerson.Number = db.GetString(reader, 0);
        entities.SupportedCsePerson.Populated = true;
      });
  }

  private bool ReadGoodCause()
  {
    System.Diagnostics.Debug.Assert(entities.Ar.Populated);
    entities.GoodCause.Populated = false;

    return Read("ReadGoodCause",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Ar.CasNumber);
        db.SetString(command, "cspNumber", entities.Ar.CspNumber);
        db.SetString(command, "croType", entities.Ar.Type1);
        db.SetInt32(command, "croIdentifier", entities.Ar.Identifier);
        db.SetNullableString(command, "cspNumber1", import.Obligor.Number);
        db.SetNullableDate(
          command, "effectiveDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.GoodCause.Code = db.GetNullableString(reader, 0);
        entities.GoodCause.EffectiveDate = db.GetNullableDate(reader, 1);
        entities.GoodCause.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.GoodCause.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.GoodCause.CasNumber = db.GetString(reader, 4);
        entities.GoodCause.CspNumber = db.GetString(reader, 5);
        entities.GoodCause.CroType = db.GetString(reader, 6);
        entities.GoodCause.CroIdentifier = db.GetInt32(reader, 7);
        entities.GoodCause.CasNumber1 = db.GetNullableString(reader, 8);
        entities.GoodCause.CspNumber1 = db.GetNullableString(reader, 9);
        entities.GoodCause.CroType1 = db.GetNullableString(reader, 10);
        entities.GoodCause.CroIdentifier1 = db.GetNullableInt32(reader, 11);
        entities.GoodCause.Populated = true;
        CheckValid<GoodCause>("CroType", entities.GoodCause.CroType);
        CheckValid<GoodCause>("CroType1", entities.GoodCause.CroType1);
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public CsePersonAccount Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private CsePersonAccount zdel;
    private CsePerson obligor;
    private Obligation obligation;
    private ObligationType obligationType;
    private ObligationTransaction debt;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of GoodCauseFoundForDebt.
    /// </summary>
    [JsonPropertyName("goodCauseFoundForDebt")]
    public Common GoodCauseFoundForDebt
    {
      get => goodCauseFoundForDebt ??= new();
      set => goodCauseFoundForDebt = value;
    }

    private Common goodCauseFoundForDebt;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CaseRole Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CaseRole Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of Ch.
    /// </summary>
    [JsonPropertyName("ch")]
    public CaseRole Ch
    {
      get => ch ??= new();
      set => ch = value;
    }

    /// <summary>
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
    }

    /// <summary>
    /// A value of SupportedCsePersonAccount.
    /// </summary>
    [JsonPropertyName("supportedCsePersonAccount")]
    public CsePersonAccount SupportedCsePersonAccount
    {
      get => supportedCsePersonAccount ??= new();
      set => supportedCsePersonAccount = value;
    }

    /// <summary>
    /// A value of ObligorCsePersonAccount.
    /// </summary>
    [JsonPropertyName("obligorCsePersonAccount")]
    public CsePersonAccount ObligorCsePersonAccount
    {
      get => obligorCsePersonAccount ??= new();
      set => obligorCsePersonAccount = value;
    }

    /// <summary>
    /// A value of GoodCause.
    /// </summary>
    [JsonPropertyName("goodCause")]
    public GoodCause GoodCause
    {
      get => goodCause ??= new();
      set => goodCause = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of ObligorCsePerson.
    /// </summary>
    [JsonPropertyName("obligorCsePerson")]
    public CsePerson ObligorCsePerson
    {
      get => obligorCsePerson ??= new();
      set => obligorCsePerson = value;
    }

    /// <summary>
    /// A value of SupportedCsePerson.
    /// </summary>
    [JsonPropertyName("supportedCsePerson")]
    public CsePerson SupportedCsePerson
    {
      get => supportedCsePerson ??= new();
      set => supportedCsePerson = value;
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

    private CaseRole ar;
    private CaseRole ap;
    private CaseRole ch;
    private ObligationTransaction obligationTransaction;
    private CsePersonAccount supportedCsePersonAccount;
    private CsePersonAccount obligorCsePersonAccount;
    private GoodCause goodCause;
    private Obligation obligation;
    private ObligationType obligationType;
    private CsePerson obligorCsePerson;
    private CsePerson supportedCsePerson;
    private Case1 case1;
  }
#endregion
}
