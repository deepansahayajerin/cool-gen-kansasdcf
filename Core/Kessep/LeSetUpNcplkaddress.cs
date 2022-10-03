// Program: LE_SET_UP_NCPLKADDRESS, ID: 1902493504, model: 746.
// Short name: SWE03748
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_SET_UP_NCPLKADDRESS.
/// </summary>
[Serializable]
public partial class LeSetUpNcplkaddress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_SET_UP_NCPLKADDRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeSetUpNcplkaddress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeSetUpNcplkaddress.
  /// </summary>
  public LeSetUpNcplkaddress(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.Infrastructure.ReasonCode = "NCPLKADDRESS";
    local.Infrastructure.EventId = 52;
    local.Infrastructure.CsenetInOutCode = "";
    local.Infrastructure.ProcessStatus = "Q";
    local.Infrastructure.UserId = "INCS";
    local.Infrastructure.BusinessObjectCd = "CAS";
    local.Infrastructure.ReferenceDate = import.TodayDate.Date;
    local.Infrastructure.CreatedBy = global.UserId;
    local.Infrastructure.CsePersonNumber = import.CsePerson.Number;
    local.Infrastructure.Detail =
      "See CSLN for last know address received from: " + (
        import.Employer.Name ?? "");

    foreach(var item in ReadCase())
    {
      local.Infrastructure.CaseNumber = entities.Case1.Number;
      local.Infrastructure.CreatedTimestamp = Now();
      local.Infrastructure.SituationNumber = 0;
      UseSpCabCreateInfrastructure();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      local.NarrativeDetail.InfrastructureId =
        local.Infrastructure.SystemGeneratedIdentifier;
      local.NarrativeDetail.CaseNumber = entities.Case1.Number;
      local.NarrativeDetail.CreatedBy = global.UserId;
      local.NarrativeDetail.CreatedTimestamp =
        local.Infrastructure.CreatedTimestamp;
      local.NarrativeDetail.LineNumber = 1;
      local.NarrativeDetail.NarrativeText =
        "Last known address received from: " + (import.Employer.Name ?? "");
      UseSpCabCreateNarrativeDetail();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      local.NarrativeDetail.LineNumber = 2;

      if (!IsEmpty(import.NcpLast.Street2))
      {
        local.NarrativeDetail.NarrativeText =
          TrimEnd(import.NcpLast.Street1) + ", " + (
            import.NcpLast.Street2 ?? "");
      }
      else
      {
        local.NarrativeDetail.NarrativeText = import.NcpLast.Street1 ?? "";
      }

      UseSpCabCreateNarrativeDetail();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      local.NarrativeDetail.LineNumber = 3;
      local.NarrativeDetail.NarrativeText = TrimEnd(import.NcpLast.City) + "  " +
        (import.NcpLast.State ?? "") + "  " + (import.NcpLast.ZipCode ?? "") + "  " +
        (import.NcpLast.Zip4 ?? "");
      UseSpCabCreateNarrativeDetail();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      local.NcpPhoneNumber.Text3 =
        NumberToString(import.NcpLastKnown.OtherAreaCode.GetValueOrDefault(),
        13, 3);
      local.NcpPhoneNumber.Text7 =
        NumberToString(import.NcpLastKnown.OtherNumber.GetValueOrDefault(), 9, 7);
        
      local.NarrativeDetail.LineNumber = 4;
      local.NarrativeDetail.NarrativeText = "Phone # " + local
        .NcpPhoneNumber.Text3 + "  " + local.NcpPhoneNumber.Text7;
      UseSpCabCreateNarrativeDetail();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    local.Infrastructure.Assign(useExport.Infrastructure);
  }

  private void UseSpCabCreateNarrativeDetail()
  {
    var useImport = new SpCabCreateNarrativeDetail.Import();
    var useExport = new SpCabCreateNarrativeDetail.Export();

    useImport.NarrativeDetail.Assign(local.NarrativeDetail);

    Call(SpCabCreateNarrativeDetail.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", import.TodayDate.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;

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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of NcpLastKnown.
    /// </summary>
    [JsonPropertyName("ncpLastKnown")]
    public CsePerson NcpLastKnown
    {
      get => ncpLastKnown ??= new();
      set => ncpLastKnown = value;
    }

    /// <summary>
    /// A value of NcpLast.
    /// </summary>
    [JsonPropertyName("ncpLast")]
    public CsePersonAddress NcpLast
    {
      get => ncpLast ??= new();
      set => ncpLast = value;
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
    /// A value of TodayDate.
    /// </summary>
    [JsonPropertyName("todayDate")]
    public DateWorkArea TodayDate
    {
      get => todayDate ??= new();
      set => todayDate = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private CsePerson csePerson;
    private Employer employer;
    private CsePerson ncpLastKnown;
    private CsePersonAddress ncpLast;
    private CsePersonsWorkSet csePersonsWorkSet;
    private DateWorkArea todayDate;
    private Infrastructure infrastructure;
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
    /// A value of InfraCreateOk.
    /// </summary>
    [JsonPropertyName("infraCreateOk")]
    public Common InfraCreateOk
    {
      get => infraCreateOk ??= new();
      set => infraCreateOk = value;
    }

    /// <summary>
    /// A value of Hold.
    /// </summary>
    [JsonPropertyName("hold")]
    public Infrastructure Hold
    {
      get => hold ??= new();
      set => hold = value;
    }

    /// <summary>
    /// A value of Local1StChar.
    /// </summary>
    [JsonPropertyName("local1StChar")]
    public Infrastructure Local1StChar
    {
      get => local1StChar ??= new();
      set => local1StChar = value;
    }

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
    /// A value of NarrativeDetail.
    /// </summary>
    [JsonPropertyName("narrativeDetail")]
    public NarrativeDetail NarrativeDetail
    {
      get => narrativeDetail ??= new();
      set => narrativeDetail = value;
    }

    /// <summary>
    /// A value of NcpPhoneNumber.
    /// </summary>
    [JsonPropertyName("ncpPhoneNumber")]
    public WorkArea NcpPhoneNumber
    {
      get => ncpPhoneNumber ??= new();
      set => ncpPhoneNumber = value;
    }

    private Common infraCreateOk;
    private Infrastructure hold;
    private Infrastructure local1StChar;
    private Infrastructure infrastructure;
    private NarrativeDetail narrativeDetail;
    private WorkArea ncpPhoneNumber;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public Infrastructure New1
    {
      get => new1 ??= new();
      set => new1 = value;
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
    /// A value of AbsentParent.
    /// </summary>
    [JsonPropertyName("absentParent")]
    public CaseRole AbsentParent
    {
      get => absentParent ??= new();
      set => absentParent = value;
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

    private Infrastructure new1;
    private Case1 case1;
    private CaseRole absentParent;
    private CsePerson csePerson;
  }
#endregion
}
