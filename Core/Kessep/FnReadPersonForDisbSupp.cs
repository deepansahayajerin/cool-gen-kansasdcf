// Program: FN_READ_PERSON_FOR_DISB_SUPP, ID: 371751818, model: 746.
// Short name: SWE00580
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_READ_PERSON_FOR_DISB_SUPP.
/// </para>
/// <para>
/// RESP : FINANCE
/// This cab will read a cse person and cse person account for FN MNT PERSON 
/// DISB SUPPRESSION
/// </para>
/// </summary>
[Serializable]
public partial class FnReadPersonForDisbSupp: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_READ_PERSON_FOR_DISB_SUPP program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReadPersonForDisbSupp(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReadPersonForDisbSupp.
  /// </summary>
  public FnReadPersonForDisbSupp(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ******************************************************************
    //                  Developed for KESSEP by MTW
    //                   D. M. Nilsen  09/06/95
    // *******************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    export.CsePerson.Number = import.CsePerson.Number;
    export.CsePersonAccount.Type1 = import.CsePersonAccount.Type1;

    if (ReadCsePerson())
    {
      if (AsChar(entities.CsePerson.Type1) == 'C')
      {
        // Client
        local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
        UseCabReadAdabasPerson();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
        else
        {
          // use format person name
          export.CsePerson.Number = entities.CsePerson.Number;
        }
      }
      else
      {
        // organization
        export.CsePersonsWorkSet.FormattedName =
          entities.CsePerson.OrganizationName ?? Spaces(33);
      }

      if (ReadCsePersonAccount())
      {
        export.CsePersonAccount.Type1 = entities.CsePersonAccount.Type1;
      }
      else
      {
        try
        {
          CreateCsePersonAccount();
          export.CsePersonAccount.Type1 = entities.CsePersonAccount.Type1;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_CSE_PERSON_ACCOUNT_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_CSE_PERSON_ACCOUNT_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }
    else
    {
      ExitState = "CSE_PERSON_NF";
    }
  }

  private void UseCabReadAdabasPerson()
  {
    var useImport = new CabReadAdabasPerson.Import();
    var useExport = new CabReadAdabasPerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(CabReadAdabasPerson.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void CreateCsePersonAccount()
  {
    var cspNumber = entities.CsePerson.Number;
    var type1 = import.CsePersonAccount.Type1;
    var createdBy = global.UserId;
    var createdTmst = Now();

    CheckValid<CsePersonAccount>("Type1", type1);
    entities.CsePersonAccount.Populated = false;
    Update("CreateCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "type", type1);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetNullableDate(command, "recompBalFromDt", default(DateTime));
        db.SetNullableDecimal(command, "stdTotGiftColl", 0M);
        db.SetNullableString(command, "triggerType", "");
      });

    entities.CsePersonAccount.CspNumber = cspNumber;
    entities.CsePersonAccount.Type1 = type1;
    entities.CsePersonAccount.CreatedBy = createdBy;
    entities.CsePersonAccount.CreatedTmst = createdTmst;
    entities.CsePersonAccount.Populated = true;
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
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePersonAccount()
  {
    entities.CsePersonAccount.Populated = false;

    return Read("ReadCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetString(command, "type", import.CsePersonAccount.Type1);
      },
      (db, reader) =>
      {
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 0);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 1);
        entities.CsePersonAccount.CreatedBy = db.GetString(reader, 2);
        entities.CsePersonAccount.CreatedTmst = db.GetDateTime(reader, 3);
        entities.CsePersonAccount.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.CsePersonAccount.Type1);
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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

    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private CsePersonAccount csePersonAccount;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
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

    private AbendData abendData;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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

    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
  }
#endregion
}
