// Program: FN_DELETE_OBLIGATION_PYMNT_SCH, ID: 372041915, model: 746.
// Short name: SWE00421
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_DELETE_OBLIGATION_PYMNT_SCH.
/// </para>
/// <para>
/// RESP: FINANCE
/// This process results in the deletion of an inactive obligation payment 
/// schedule. An  Inactive payment schedule has effective date greater than
/// current date.  Thus, it has not yet become effective.
/// </para>
/// </summary>
[Serializable]
public partial class FnDeleteObligationPymntSch: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DELETE_OBLIGATION_PYMNT_SCH program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDeleteObligationPymntSch(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDeleteObligationPymntSch.
  /// </summary>
  public FnDeleteObligationPymntSch(IContext context, Import import,
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
    // -----------------------------------------------
    // Every initial development and change to that
    // development needs to be documented.
    // -----------------------------------------------
    // -----------------------------------------------------------------------
    // Date	   Programmer		Reason #	Description
    // 01/22/95   Holly Kennedy-MTW			Modify initial code.
    // 07/15/97   Paul R. Egger-MTW                    Changed to allow deletion
    // of a record that has a start date of the current date.
    // -----------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    UseFnHardcodedDebtDistribution();

    if (ReadObligationPaymentSchedule())
    {
      // Check for Active Obligation Payment Schedule.
      if (Lt(entities.ObligationPaymentSchedule.StartDt, Now().Date) && !
        Lt(entities.ObligationPaymentSchedule.EndDt, Now().Date) && Lt
        (Date(entities.ObligationPaymentSchedule.CreatedTmst), Now().Date))
      {
        ExitState = "FN0000_OBLIG_PYMNT_SCH_ACTIVE";

        return;
      }

      if (Lt(entities.ObligationPaymentSchedule.StartDt, Now().Date) && Lt
        (entities.ObligationPaymentSchedule.EndDt, Now().Date))
      {
        ExitState = "FN0000_OBLIG_PYMNT_SCH_DISCONTD";

        return;
      }

      DeleteObligationPaymentSchedule();
    }
    else
    {
      ExitState = "FN0000_OBLIG_PYMNT_SCH_NF";
    }
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    local.HardcodeObligor.Type1 = useExport.CpaObligor.Type1;
  }

  private void DeleteObligationPaymentSchedule()
  {
    Update("DeleteObligationPaymentSchedule",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyType", entities.ObligationPaymentSchedule.OtyType);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ObligationPaymentSchedule.ObgGeneratedId);
        db.SetString(
          command, "obgCspNumber",
          entities.ObligationPaymentSchedule.ObgCspNumber);
        db.SetString(
          command, "obgCpaType", entities.ObligationPaymentSchedule.ObgCpaType);
          
        db.SetDate(
          command, "startDt",
          entities.ObligationPaymentSchedule.StartDt.GetValueOrDefault());
      });
  }

  private bool ReadObligationPaymentSchedule()
  {
    entities.ObligationPaymentSchedule.Populated = false;

    return Read("ReadObligationPaymentSchedule",
      (db, command) =>
      {
        db.SetDate(
          command, "startDt",
          import.ObligationPaymentSchedule.StartDt.GetValueOrDefault());
        db.SetInt32(
          command, "obgGeneratedId",
          import.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "obgCpaType", local.HardcodeObligor.Type1);
        db.SetString(command, "obgCspNumber", import.CsePerson.Number);
        db.SetInt32(
          command, "otyType", import.ObligationType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ObligationPaymentSchedule.OtyType = db.GetInt32(reader, 0);
        entities.ObligationPaymentSchedule.ObgGeneratedId =
          db.GetInt32(reader, 1);
        entities.ObligationPaymentSchedule.ObgCspNumber =
          db.GetString(reader, 2);
        entities.ObligationPaymentSchedule.ObgCpaType = db.GetString(reader, 3);
        entities.ObligationPaymentSchedule.StartDt = db.GetDate(reader, 4);
        entities.ObligationPaymentSchedule.Amount =
          db.GetNullableDecimal(reader, 5);
        entities.ObligationPaymentSchedule.EndDt =
          db.GetNullableDate(reader, 6);
        entities.ObligationPaymentSchedule.CreatedBy = db.GetString(reader, 7);
        entities.ObligationPaymentSchedule.CreatedTmst =
          db.GetDateTime(reader, 8);
        entities.ObligationPaymentSchedule.LastUpdateBy =
          db.GetNullableString(reader, 9);
        entities.ObligationPaymentSchedule.LastUpdateTmst =
          db.GetNullableDateTime(reader, 10);
        entities.ObligationPaymentSchedule.Populated = true;
        CheckValid<ObligationPaymentSchedule>("ObgCpaType",
          entities.ObligationPaymentSchedule.ObgCpaType);
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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

    /// <summary>
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
    }

    private ObligationType obligationType;
    private CsePerson csePerson;
    private Obligation obligation;
    private ObligationPaymentSchedule obligationPaymentSchedule;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of HardcodeObligor.
    /// </summary>
    [JsonPropertyName("hardcodeObligor")]
    public CsePersonAccount HardcodeObligor
    {
      get => hardcodeObligor ??= new();
      set => hardcodeObligor = value;
    }

    private CsePersonAccount hardcodeObligor;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
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
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
    }

    private ObligationType obligationType;
    private CsePerson csePerson;
    private CsePersonAccount obligor;
    private Obligation obligation;
    private ObligationPaymentSchedule obligationPaymentSchedule;
  }
#endregion
}
