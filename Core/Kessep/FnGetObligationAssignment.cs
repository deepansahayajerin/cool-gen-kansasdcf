// Program: FN_GET_OBLIGATION_ASSIGNMENT, ID: 372084595, model: 746.
// Short name: SWE02033
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_GET_OBLIGATION_ASSIGNMENT.
/// </para>
/// <para>
/// This CAB will return the Assigned to User id and name. The name is returned 
/// as :-
///   firstname, middle name and last name
/// or a single attribute view - formatted name.
/// </para>
/// </summary>
[Serializable]
public partial class FnGetObligationAssignment: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_GET_OBLIGATION_ASSIGNMENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnGetObligationAssignment(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnGetObligationAssignment.
  /// </summary>
  public FnGetObligationAssignment(IContext context, Import import,
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
    // ***-----
    // ***--- Written By - Sumanta - MTW - 05/13/97
    // ***--- This CAB will return the Assigned to ID and Name
    // ***--- The name is returned formatted as well as
    // ***--- unformatted (first, middle, last name)
    // ***-----
    // ------------------------------------------------------------------
    // Change Log:
    // Date         Developer         ref      Description
    // 06/171997    Holly Kennedy-MTW          Current date logic was not
    //                                         
    // populating the current date
    //                                         
    // view.  Added set statement.
    // ------------------------------------------------------------------
    // *** 09/01/98  Bud Adams	   imported current-date	***
    if (!ReadObligation())
    {
      ExitState = "FN0000_OBLIGATION_NF";

      return;
    }

    if (ReadServiceProvider())
    {
      export.ServiceProvider.Assign(entities.ExistingServiceProvider);

      if (!IsEmpty(export.ServiceProvider.MiddleInitial))
      {
        export.CsePersonsWorkSet.FormattedName =
          TrimEnd(export.ServiceProvider.FirstName) + " " + export
          .ServiceProvider.MiddleInitial + " " + TrimEnd
          (export.ServiceProvider.LastName);
      }
      else
      {
        export.CsePersonsWorkSet.FormattedName =
          TrimEnd(export.ServiceProvider.FirstName) + " " + TrimEnd
          (export.ServiceProvider.LastName);
      }
    }
  }

  private bool ReadObligation()
  {
    entities.ExistingObligation.Populated = false;

    return Read("ReadObligation",
      (db, command) =>
      {
        db.
          SetInt32(command, "obId", import.Obligation.SystemGeneratedIdentifier);
          
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetString(command, "cpaType", import.CsePersonAccount.Type1);
        db.SetInt32(
          command, "dtyGeneratedId",
          import.ObligationType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingObligation.CpaType = db.GetString(reader, 0);
        entities.ExistingObligation.CspNumber = db.GetString(reader, 1);
        entities.ExistingObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ExistingObligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.ExistingObligation.CpaType);
      });
  }

  private bool ReadServiceProvider()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);
    entities.ExistingServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
        db.SetInt32(
          command, "obgId",
          entities.ExistingObligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNo", entities.ExistingObligation.CspNumber);
        db.SetString(command, "cpaType", entities.ExistingObligation.CpaType);
        db.
          SetInt32(command, "otyId", entities.ExistingObligation.DtyGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingServiceProvider.UserId = db.GetString(reader, 1);
        entities.ExistingServiceProvider.LastName = db.GetString(reader, 2);
        entities.ExistingServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ExistingServiceProvider.MiddleInitial =
          db.GetString(reader, 4);
        entities.ExistingServiceProvider.Populated = true;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    private DateWorkArea current;
    private ObligationType obligationType;
    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
    private Obligation obligation;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private ServiceProvider serviceProvider;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    private DateWorkArea max;
    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("existingOfficeServiceProvider")]
    public OfficeServiceProvider ExistingOfficeServiceProvider
    {
      get => existingOfficeServiceProvider ??= new();
      set => existingOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingObligationAssignment.
    /// </summary>
    [JsonPropertyName("existingObligationAssignment")]
    public ObligationAssignment ExistingObligationAssignment
    {
      get => existingObligationAssignment ??= new();
      set => existingObligationAssignment = value;
    }

    /// <summary>
    /// A value of ExistingServiceProvider.
    /// </summary>
    [JsonPropertyName("existingServiceProvider")]
    public ServiceProvider ExistingServiceProvider
    {
      get => existingServiceProvider ??= new();
      set => existingServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingObligationType.
    /// </summary>
    [JsonPropertyName("existingObligationType")]
    public ObligationType ExistingObligationType
    {
      get => existingObligationType ??= new();
      set => existingObligationType = value;
    }

    /// <summary>
    /// A value of ExistingCsePersonAccount.
    /// </summary>
    [JsonPropertyName("existingCsePersonAccount")]
    public CsePersonAccount ExistingCsePersonAccount
    {
      get => existingCsePersonAccount ??= new();
      set => existingCsePersonAccount = value;
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

    /// <summary>
    /// A value of ExistingObligation.
    /// </summary>
    [JsonPropertyName("existingObligation")]
    public Obligation ExistingObligation
    {
      get => existingObligation ??= new();
      set => existingObligation = value;
    }

    private OfficeServiceProvider existingOfficeServiceProvider;
    private ObligationAssignment existingObligationAssignment;
    private ServiceProvider existingServiceProvider;
    private ObligationType existingObligationType;
    private CsePersonAccount existingCsePersonAccount;
    private CsePerson existingCsePerson;
    private Obligation existingObligation;
  }
#endregion
}
