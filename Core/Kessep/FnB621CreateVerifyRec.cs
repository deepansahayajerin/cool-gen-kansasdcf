// Program: FN_B621_CREATE_VERIFY_REC, ID: 373454871, model: 746.
// Short name: SWE00100
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B621_CREATE_VERIFY_REC.
/// </summary>
[Serializable]
public partial class FnB621CreateVerifyRec: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B621_CREATE_VERIFY_REC program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB621CreateVerifyRec(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB621CreateVerifyRec.
  /// </summary>
  public FnB621CreateVerifyRec(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    for(local.LoopCount.Count = 1; local.LoopCount.Count <= 100; ++
      local.LoopCount.Count)
    {
      try
      {
        CreateStatsVerifi();

        return;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            break;
          case ErrorCode.PermittedValueViolation:
            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
  }

  private void CreateStatsVerifi()
  {
    var yearMonth = import.StatsVerifi.YearMonth.GetValueOrDefault();
    var firstRunNumber = import.StatsVerifi.FirstRunNumber.GetValueOrDefault();
    var lineNumber = import.StatsVerifi.LineNumber.GetValueOrDefault();
    var programType = import.StatsVerifi.ProgramType ?? "";
    var createdTimestamp = Now();
    var servicePrvdrId = import.StatsVerifi.ServicePrvdrId.GetValueOrDefault();
    var officeId = import.StatsVerifi.OfficeId.GetValueOrDefault();
    var caseWrkRole = import.StatsVerifi.CaseWrkRole ?? "";
    var parentId = import.StatsVerifi.ParentId.GetValueOrDefault();
    var chiefId = import.StatsVerifi.ChiefId.GetValueOrDefault();
    var caseNumber = import.StatsVerifi.CaseNumber ?? "";
    var suppPersonNumber = import.StatsVerifi.SuppPersonNumber ?? "";
    var obligorPersonNbr = import.StatsVerifi.ObligorPersonNbr ?? "";
    var datePaternityEst = import.StatsVerifi.DatePaternityEst;
    var courtOrderNumber = import.StatsVerifi.CourtOrderNumber ?? "";
    var tranAmount = import.StatsVerifi.TranAmount.GetValueOrDefault();
    var dddd = import.StatsVerifi.Dddd;
    var debtDetailBaldue = import.StatsVerifi.DebtDetailBaldue;
    var obligationType1 = import.StatsVerifi.ObligationType ?? "";
    var collectionAmount =
      import.StatsVerifi.CollectionAmount.GetValueOrDefault();
    var collectionDate = import.StatsVerifi.CollectionDate;
    var collCreatedDate = import.StatsVerifi.CollCreatedDate;
    var caseRoleType = import.StatsVerifi.CaseRoleType ?? "";
    var caseAsinEffDte = import.StatsVerifi.CaseAsinEffDte;
    var caseAsinEndDte = import.StatsVerifi.CaseAsinEndDte;
    var personProgCode = import.StatsVerifi.PersonProgCode ?? "";
    var comment = import.StatsVerifi.Comment ?? "";

    entities.StatsVerifi.Populated = false;
    Update("CreateStatsVerifi",
      (db, command) =>
      {
        db.SetNullableInt32(command, "yearMonth", yearMonth);
        db.SetNullableInt32(command, "firstRunNumber", firstRunNumber);
        db.SetNullableInt32(command, "lineNumber", lineNumber);
        db.SetNullableString(command, "programType", programType);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableInt32(command, "servicePrvdrId", servicePrvdrId);
        db.SetNullableInt32(command, "officeId", officeId);
        db.SetNullableString(command, "caseWrkRole", caseWrkRole);
        db.SetNullableInt32(command, "parentId", parentId);
        db.SetNullableInt32(command, "chiefId", chiefId);
        db.SetNullableString(command, "caseNumber", caseNumber);
        db.SetNullableString(command, "suppPersonNumber", suppPersonNumber);
        db.SetNullableString(command, "obligorPersonNbr", obligorPersonNbr);
        db.SetNullableDate(command, "datePaternityEst", datePaternityEst);
        db.SetNullableString(command, "courtOrderNumber", courtOrderNumber);
        db.SetNullableDecimal(command, "tranAmount", tranAmount);
        db.SetNullableDate(command, "dddd", dddd);
        db.SetDecimal(command, "debtDetailBaldue", debtDetailBaldue);
        db.SetNullableString(command, "obligationType", obligationType1);
        db.SetNullableDecimal(command, "collectionAmount", collectionAmount);
        db.SetNullableDate(command, "collectionDate", collectionDate);
        db.SetNullableDate(command, "collCreatedDate", collCreatedDate);
        db.SetNullableString(command, "caseRoleType", caseRoleType);
        db.SetNullableDate(command, "caseAsinEffDte", caseAsinEffDte);
        db.SetNullableDate(command, "caseAsinEndDte", caseAsinEndDte);
        db.SetNullableString(command, "personProgCode", personProgCode);
        db.SetNullableString(command, "comment0", comment);
      });

    entities.StatsVerifi.YearMonth = yearMonth;
    entities.StatsVerifi.FirstRunNumber = firstRunNumber;
    entities.StatsVerifi.LineNumber = lineNumber;
    entities.StatsVerifi.ProgramType = programType;
    entities.StatsVerifi.CreatedTimestamp = createdTimestamp;
    entities.StatsVerifi.ServicePrvdrId = servicePrvdrId;
    entities.StatsVerifi.OfficeId = officeId;
    entities.StatsVerifi.CaseWrkRole = caseWrkRole;
    entities.StatsVerifi.ParentId = parentId;
    entities.StatsVerifi.ChiefId = chiefId;
    entities.StatsVerifi.CaseNumber = caseNumber;
    entities.StatsVerifi.SuppPersonNumber = suppPersonNumber;
    entities.StatsVerifi.ObligorPersonNbr = obligorPersonNbr;
    entities.StatsVerifi.DatePaternityEst = datePaternityEst;
    entities.StatsVerifi.CourtOrderNumber = courtOrderNumber;
    entities.StatsVerifi.TranAmount = tranAmount;
    entities.StatsVerifi.Dddd = dddd;
    entities.StatsVerifi.DebtDetailBaldue = debtDetailBaldue;
    entities.StatsVerifi.ObligationType = obligationType1;
    entities.StatsVerifi.CollectionAmount = collectionAmount;
    entities.StatsVerifi.CollectionDate = collectionDate;
    entities.StatsVerifi.CollCreatedDate = collCreatedDate;
    entities.StatsVerifi.CaseRoleType = caseRoleType;
    entities.StatsVerifi.CaseAsinEffDte = caseAsinEffDte;
    entities.StatsVerifi.CaseAsinEndDte = caseAsinEndDte;
    entities.StatsVerifi.PersonProgCode = personProgCode;
    entities.StatsVerifi.Comment = comment;
    entities.StatsVerifi.Populated = true;
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
    /// A value of StatsVerifi.
    /// </summary>
    [JsonPropertyName("statsVerifi")]
    public StatsVerifi StatsVerifi
    {
      get => statsVerifi ??= new();
      set => statsVerifi = value;
    }

    private StatsVerifi statsVerifi;
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
    /// A value of LoopCount.
    /// </summary>
    [JsonPropertyName("loopCount")]
    public Common LoopCount
    {
      get => loopCount ??= new();
      set => loopCount = value;
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    private Common loopCount;
    private Program program;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of StatsVerifi.
    /// </summary>
    [JsonPropertyName("statsVerifi")]
    public StatsVerifi StatsVerifi
    {
      get => statsVerifi ??= new();
      set => statsVerifi = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    private StatsVerifi statsVerifi;
    private DebtDetail debtDetail;
    private ObligationType obligationType;
  }
#endregion
}
