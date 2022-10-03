// Program: SI_VERIFY_AND_CREATE_CSE_PERSON, ID: 371785563, model: 746.
// Short name: SWE01265
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_VERIFY_AND_CREATE_CSE_PERSON.
/// </para>
/// <para>
/// RESP: SRVINIT	
/// This action block checks whether a person is on ADABAS or DB2.  If not, 
/// create in appropriate place(s).
/// </para>
/// </summary>
[Serializable]
public partial class SiVerifyAndCreateCsePerson: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_VERIFY_AND_CREATE_CSE_PERSON program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiVerifyAndCreateCsePerson(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiVerifyAndCreateCsePerson.
  /// </summary>
  public SiVerifyAndCreateCsePerson(IContext context, Import import,
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
    // -----------------------------------------------------------
    //                 M A I N T E N A N C E   L O G
    //   Date      Developer       Request #       Description
    // 09/02/95  Helen Sharland                   Initial Development
    // ----------------------------------------------------------
    // 01/29/99 W.Campbell        Added an import view
    //                            (import_cse ief_supplied flag)
    //                            to allow for the input of a flag
    //                            to indicate that the person being
    //                            passed is a non-case related
    //                            CSE person (flag = N).  See notes
    //                            below. Work done on IDCR477.
    // ------------------------------------------------
    // 01/29/99 W.Campbell        Added logic to use
    //                            SI_ALTS_CAB_UPDATE_ALIAS
    //                            in order to make a non_case
    //                            related CSE person into a case
    //                            related CSE person, if needed.
    //                            Work done on IDCR477.
    // ------------------------------------------------
    // 02/15/99 W.Campbell        Added statement to
    //                            set local cse_person number to export
    //                            cse_person_work_set number. This was
    //                            done in order to pass the cse_person
    //                            number to CAB
    //                            si_regi_copy_adabas_person_pgms.
    // ------------------------------------------------
    // 06/24/99 M.Lachowicz      Change property of READ
    //                           (Select Only or Cursor Only)
    // ------------------------------------------------------------
    // 09/09/99 W.Campbell        Changes made to
    //                            accomodate the import_cse
    //                            ief_supplied flag = spaces
    //                            (to include the non CSE persons
    //                            on ADABAS so that they will
    //                            become known to CSE
    //                            and have their person-
    //                            programs copied to KESSEP).
    //                            This was done on PR#H72775.
    // ------------------------------------------------
    // 01/26/00 M.Lachowicz      Fixed PR #74943
    // ------------------------------------------------------------
    export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);
    local.CsePerson.Number = export.CsePersonsWorkSet.Number;

    if (IsEmpty(export.CsePersonsWorkSet.Number))
    {
      UseCabCreateAdabasPerson();

      if (!IsEmpty(export.AbendData.Type1))
      {
        return;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      local.CsePerson.Type1 = "C";
      UseSiCreateCsePerson();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      MoveCsePersonsWorkSet(export.CsePersonsWorkSet, local.Alias);
      local.Alias.UniqueKey = "";
      UseSiAltsCabUpdateAlias();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // ------------------------------------------------
      // 02/15/99 W.Campbell - Added statement to
      // set local cse_person number to export
      // cse_person_work_set number.  This was
      // done in order to pass the cse_person number to
      // CAB si_regi_copy_adabas_person_pgms.
      // ------------------------------------------------
      local.CsePerson.Number = export.CsePersonsWorkSet.Number;
      UseSiRegiCopyAdabasPersonPgms();

      if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
        ("CHILD_PERSON_PROG_NF"))
      {
        ExitState = "ACO_NN0000_ALL_OK";
      }
      else
      {
      }
    }
    else
    {
      // 06/24/99 M.L
      //              Change property of the following READ to generate
      //              SELECT ONLY
      if (ReadCsePerson())
      {
        // ------------------------------------------------
        // No need to add to system
        // ------------------------------------------------
        // ------------------------------------------------
        // 01/29/99 W.Campbell - Added logic here to
        // use SI_ALTS_CAB_UPDATE_ALIAS in order
        // to make this non_case related CSE person
        // into a case related CSE person.
        // Work done on IDCR477.
        // ------------------------------------------------
        // ------------------------------------------------
        // However, we must now see if this
        // is a non-case related CSE person
        // (import_cse ief_supplied flag = N),
        // and if it is, then we must use
        // SI_ALTS_CAB_UPDATE_ALIAS
        // to get the ownership changed to a case
        // related CSE person (flag = Y).
        // ------------------------------------------------
        // ------------------------------------------------
        // 09/09/99 W.Campbell - Added or clause
        // to the following IF stmt to also check
        // the import_cse ief_supplied flag for
        // spaces (to include the non CSE persons
        // on ADABAS so that they will become
        // known to CSE.  This was done for
        // PR#H72775.
        // ------------------------------------------------
        if (AsChar(import.Cse.Flag) == 'N' || IsEmpty(import.Cse.Flag))
        {
          MoveCsePersonsWorkSet(export.CsePersonsWorkSet, local.Alias);
          local.Alias.UniqueKey = "";
          UseSiAltsCabUpdateAlias();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }

        // ------------------------------------------------
        // 01/29/99 W.Campbell - End of logic added
        // to use SI_ALTS_CAB_UPDATE_ALIAS in order
        // to make this non_case related CSE person
        // into a case related CSE person.
        // Work done on IDCR477.
        // ------------------------------------------------
        // ------------------------------------------------
        // 09/09/99 W.Campbell - Added the following
        // IF stmt, and logic therein, to check the
        // import_cse ief_supplied flag = spaces
        // so that we also get the person-programs
        // for this person.
        // This was done for PR#H72775.
        // ------------------------------------------------
        // 01/26/00 M.L - Start
        UseSiRegiCopyAdabasPersonPgms();

        if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
          ("CHILD_PERSON_PROG_NF"))
        {
          ExitState = "ACO_NN0000_ALL_OK";
        }
        else
        {
        }
      }
      else
      {
        local.CsePerson.Type1 = "C";
        UseSiCreateCsePerson();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        MoveCsePersonsWorkSet(export.CsePersonsWorkSet, local.Alias);
        local.Alias.UniqueKey = "";
        UseSiAltsCabUpdateAlias();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        UseSiRegiCopyAdabasPersonPgms();

        if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
          ("CHILD_PERSON_PROG_NF"))
        {
          ExitState = "ACO_NN0000_ALL_OK";
        }
        else
        {
        }
      }
    }
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private void UseCabCreateAdabasPerson()
  {
    var useImport = new CabCreateAdabasPerson.Import();
    var useExport = new CabCreateAdabasPerson.Export();

    useImport.CsePersonsWorkSet.Assign(export.CsePersonsWorkSet);

    Call(CabCreateAdabasPerson.Execute, useImport, useExport);

    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    export.AbendData.Assign(useExport.AbendData);
  }

  private void UseSiAltsCabUpdateAlias()
  {
    var useImport = new SiAltsCabUpdateAlias.Import();
    var useExport = new SiAltsCabUpdateAlias.Export();

    useImport.CsePersonsWorkSet.Assign(local.Alias);

    Call(SiAltsCabUpdateAlias.Execute, useImport, useExport);
  }

  private void UseSiCreateCsePerson()
  {
    var useImport = new SiCreateCsePerson.Import();
    var useExport = new SiCreateCsePerson.Export();

    MoveCsePerson(local.CsePerson, useImport.CsePerson);
    useImport.CsePersonsWorkSet.Assign(export.CsePersonsWorkSet);

    Call(SiCreateCsePerson.Execute, useImport, useExport);
  }

  private void UseSiRegiCopyAdabasPersonPgms()
  {
    var useImport = new SiRegiCopyAdabasPersonPgms.Import();
    var useExport = new SiRegiCopyAdabasPersonPgms.Export();

    useImport.CseReferralReceived.Date = import.RetreivePersonProgram.Date;
    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(SiRegiCopyAdabasPersonPgms.Execute, useImport, useExport);
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", export.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
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
    /// A value of Cse.
    /// </summary>
    [JsonPropertyName("cse")]
    public Common Cse
    {
      get => cse ??= new();
      set => cse = value;
    }

    /// <summary>
    /// A value of RetreivePersonProgram.
    /// </summary>
    [JsonPropertyName("retreivePersonProgram")]
    public DateWorkArea RetreivePersonProgram
    {
      get => retreivePersonProgram ??= new();
      set => retreivePersonProgram = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private Common cse;
    private DateWorkArea retreivePersonProgram;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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

    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private AbendData abendData;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Alias.
    /// </summary>
    [JsonPropertyName("alias")]
    public CsePersonsWorkSet Alias
    {
      get => alias ??= new();
      set => alias = value;
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

    private CsePersonsWorkSet alias;
    private CsePerson csePerson;
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

    private CsePerson csePerson;
  }
#endregion
}
