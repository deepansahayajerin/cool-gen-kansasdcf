// Program: FN_PRC_COLL_PROT_HIST_AND_ALRTS, ID: 373381406, model: 746.
// Short name: SWE02625
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_PRC_COLL_PROT_HIST_AND_ALRTS.
/// </summary>
[Serializable]
public partial class FnPrcCollProtHistAndAlrts: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_PRC_COLL_PROT_HIST_AND_ALRTS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnPrcCollProtHistAndAlrts(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnPrcCollProtHistAndAlrts.
  /// </summary>
  public FnPrcCollProtHistAndAlrts(IContext context, Import import,
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
    // * * * * * * * * * * * * * * * *
    // * * Set up the information to create the INFRASTRUCTURE Record
    // * * * * * * * * * * * * * * * *
    local.Infrastructure.EventId = 10500;
    local.Infrastructure.BusinessObjectCd = "OBL";
    local.Infrastructure.InitiatingStateCode = "KS";
    local.Infrastructure.CsePersonNumber = import.Obligor.Number;
    local.Infrastructure.UserId = global.UserId;

    switch(AsChar(import.CollProtAction.Text1))
    {
      case 'A':
        local.Infrastructure.ReasonCode = "COLLPROTADD";
        local.Infrastructure.Detail = "Coll Prot Added for AP: " + import
          .Obligor.Number + ", OB ID: " + NumberToString
          (import.Obligation.SystemGeneratedIdentifier, 13, 3);

        break;
      case 'D':
        local.Infrastructure.ReasonCode = "COLLPROTDAC";
        local.Infrastructure.Detail = "Coll Prot Deact for AP: " + import
          .Obligor.Number + ", OB ID: " + NumberToString
          (import.Obligation.SystemGeneratedIdentifier, 13, 3);

        break;
      default:
        local.Infrastructure.ReasonCode = "COLLPROTCHG";
        local.Infrastructure.Detail = "Coll Prot Chg for AP: " + import
          .Obligor.Number + ", OB ID: " + NumberToString
          (import.Obligation.SystemGeneratedIdentifier, 13, 3);

        break;
    }

    if (!IsEmpty(import.LegalAction.StandardNumber))
    {
      local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + ", Crt Ord: " +
        (import.LegalAction.StandardNumber ?? "");
    }

    foreach(var item in ReadCaseCaseUnit())
    {
      // * * * * * * * * * * * * * * * *
      // * * Only ONE Alert/Hist rec per Case
      // * * * * * * * * * * * * * * * *
      if (Equal(entities.ExistingCase.Number, local.Hold.Number))
      {
        continue;
      }

      local.Hold.Number = entities.ExistingCase.Number;
      local.Infrastructure.CaseNumber = entities.ExistingCase.Number;
      local.Infrastructure.CaseUnitNumber = entities.ExistingCaseUnit.CuNumber;
      local.Infrastructure.ProcessStatus = "Q";
      UseSpCabCreateInfrastructure();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NN0000_ALL_OK";
      }
    }
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadCaseCaseUnit()
  {
    entities.ExistingCase.Populated = false;
    entities.ExistingCaseUnit.Populated = false;

    return ReadEach("ReadCaseCaseUnit",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNoAp", import.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingCaseUnit.CasNo = db.GetString(reader, 0);
        entities.ExistingCase.Status = db.GetNullableString(reader, 1);
        entities.ExistingCaseUnit.CuNumber = db.GetInt32(reader, 2);
        entities.ExistingCaseUnit.ClosureDate = db.GetNullableDate(reader, 3);
        entities.ExistingCaseUnit.CspNoAp = db.GetNullableString(reader, 4);
        entities.ExistingCase.Populated = true;
        entities.ExistingCaseUnit.Populated = true;

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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of CollProtAction.
    /// </summary>
    [JsonPropertyName("collProtAction")]
    public TextWorkArea CollProtAction
    {
      get => collProtAction ??= new();
      set => collProtAction = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of ObligCollProtectionHist.
    /// </summary>
    [JsonPropertyName("obligCollProtectionHist")]
    public ObligCollProtectionHist ObligCollProtectionHist
    {
      get => obligCollProtectionHist ??= new();
      set => obligCollProtectionHist = value;
    }

    private CsePerson obligor;
    private TextWorkArea collProtAction;
    private Obligation obligation;
    private LegalAction legalAction;
    private ObligCollProtectionHist obligCollProtectionHist;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of Hold.
    /// </summary>
    [JsonPropertyName("hold")]
    public Case1 Hold
    {
      get => hold ??= new();
      set => hold = value;
    }

    private Infrastructure infrastructure;
    private Case1 hold;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingObligor.
    /// </summary>
    [JsonPropertyName("existingObligor")]
    public CsePerson ExistingObligor
    {
      get => existingObligor ??= new();
      set => existingObligor = value;
    }

    /// <summary>
    /// A value of ExistingCase.
    /// </summary>
    [JsonPropertyName("existingCase")]
    public Case1 ExistingCase
    {
      get => existingCase ??= new();
      set => existingCase = value;
    }

    /// <summary>
    /// A value of ExistingCaseUnit.
    /// </summary>
    [JsonPropertyName("existingCaseUnit")]
    public CaseUnit ExistingCaseUnit
    {
      get => existingCaseUnit ??= new();
      set => existingCaseUnit = value;
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
    /// A value of ExistingOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("existingOfficeServiceProvider")]
    public OfficeServiceProvider ExistingOfficeServiceProvider
    {
      get => existingOfficeServiceProvider ??= new();
      set => existingOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingCaseAssignment.
    /// </summary>
    [JsonPropertyName("existingCaseAssignment")]
    public CaseAssignment ExistingCaseAssignment
    {
      get => existingCaseAssignment ??= new();
      set => existingCaseAssignment = value;
    }

    private CsePerson existingObligor;
    private Case1 existingCase;
    private CaseUnit existingCaseUnit;
    private ServiceProvider existingServiceProvider;
    private OfficeServiceProvider existingOfficeServiceProvider;
    private CaseAssignment existingCaseAssignment;
  }
#endregion
}
