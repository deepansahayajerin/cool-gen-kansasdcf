// Program: ASSIGN_RECEIPT_RESEARCH, ID: 371724996, model: 746.
// Short name: SWE00017
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: ASSIGN_RECEIPT_RESEARCH.
/// </para>
/// <para>
/// RESP: Finance
/// This process action block assigns CASH_RECEIPT_SOURCE_TYPE to 
/// OFFICE_SERVICE_PROVIDERS.
/// </para>
/// </summary>
[Serializable]
public partial class AssignReceiptResearch: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the ASSIGN_RECEIPT_RESEARCH program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new AssignReceiptResearch(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of AssignReceiptResearch.
  /// </summary>
  public AssignReceiptResearch(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ************************************************
    // *This Common ACtion Block is being used        *
    // *primarily in the Finance Cash Management      *
    // ************************************************
    local.ForCompare.EffectiveDate =
      import.ReceiptResearchAssignment.EffectiveDate;
    local.ForCompare.ExpirationDate =
      import.ReceiptResearchAssignment.DiscontinueDate;
    UseCabCompareEffecAndDiscDates();

    if (AsChar(local.ForCompare.EffectiveDateIsZero) == 'Y')
    {
      ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

      return;
    }
    else if (AsChar(local.ForCompare.EffectiveDateIsLtCurrent) == 'Y')
    {
      ExitState = "EFFECTIVE_DATE_PRIOR_TO_CURRENT";

      return;
    }
    else if (AsChar(local.ForCompare.ExpirationDateIsZero) == 'N' && AsChar
      (local.ForCompare.ExpirationDateLtEffectiveDat) == 'Y')
    {
      ExitState = "EXPIRE_DATE_PRIOR_TO_EFFECTIVE";

      return;
    }

    if (Equal(import.ReceiptResearchAssignment.DiscontinueDate, null))
    {
      local.ReceiptResearchAssignment.DiscontinueDate =
        UseCabSetMaximumDiscontinueDate();
    }
    else
    {
      local.ReceiptResearchAssignment.DiscontinueDate =
        import.ReceiptResearchAssignment.DiscontinueDate;
    }

    // ********	WORK AREA	********
    if (ReadCashReceiptSourceType())
    {
      if (ReadServiceProvider())
      {
        try
        {
          CreateReceiptResearchAssignment();
          ExitState = "ACO_NN0000_ALL_OK";
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_RCPT_RESEARCH_ASSGNMNT_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_RCPT_RESEARCH_ASSGNMNT_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
      else
      {
        ExitState = "SERVICE_PROVIDER_NF";
      }
    }
    else
    {
      ExitState = "FN0097_CASH_RCPT_SOURCE_TYPE_NF";
    }
  }

  private static void MoveExpireEffectiveDateAttributes1(
    ExpireEffectiveDateAttributes source, ExpireEffectiveDateAttributes target)
  {
    target.ExpirationDate = source.ExpirationDate;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MoveExpireEffectiveDateAttributes2(
    ExpireEffectiveDateAttributes source, ExpireEffectiveDateAttributes target)
  {
    target.EffectiveDateIsZero = source.EffectiveDateIsZero;
    target.ExpirationDateIsZero = source.ExpirationDateIsZero;
    target.EffectiveDateIsLtCurrent = source.EffectiveDateIsLtCurrent;
    target.ExpirationDateIsLtCurrent = source.ExpirationDateIsLtCurrent;
    target.ExpirationDateLtEffectiveDat = source.ExpirationDateLtEffectiveDat;
  }

  private void UseCabCompareEffecAndDiscDates()
  {
    var useImport = new CabCompareEffecAndDiscDates.Import();
    var useExport = new CabCompareEffecAndDiscDates.Export();

    MoveExpireEffectiveDateAttributes1(local.ForCompare,
      useImport.ExpireEffectiveDateAttributes);

    Call(CabCompareEffecAndDiscDates.Execute, useImport, useExport);

    MoveExpireEffectiveDateAttributes2(useExport.ExpireEffectiveDateAttributes,
      local.ForCompare);
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void CreateReceiptResearchAssignment()
  {
    var spdIdentifier = entities.ServiceProvider.SystemGeneratedId;
    var cstIdentifier =
      entities.CashReceiptSourceType.SystemGeneratedIdentifier;
    var effectiveDate = import.ReceiptResearchAssignment.EffectiveDate;
    var discontinueDate = local.ReceiptResearchAssignment.DiscontinueDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();

    entities.ReceiptResearchAssignment.Populated = false;
    Update("CreateReceiptResearchAssignment",
      (db, command) =>
      {
        db.SetInt32(command, "spdIdentifier", spdIdentifier);
        db.SetInt32(command, "cstIdentifier", cstIdentifier);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
      });

    entities.ReceiptResearchAssignment.SpdIdentifier = spdIdentifier;
    entities.ReceiptResearchAssignment.CstIdentifier = cstIdentifier;
    entities.ReceiptResearchAssignment.EffectiveDate = effectiveDate;
    entities.ReceiptResearchAssignment.DiscontinueDate = discontinueDate;
    entities.ReceiptResearchAssignment.CreatedBy = createdBy;
    entities.ReceiptResearchAssignment.CreatedTimestamp = createdTimestamp;
    entities.ReceiptResearchAssignment.Populated = true;
  }

  private bool ReadCashReceiptSourceType()
  {
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType",
      (db, command) =>
      {
        db.SetInt32(
          command, "crSrceTypeId",
          import.CashReceiptSourceType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.Populated = true;
      });
  }

  private bool ReadServiceProvider()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId", import.ServiceProvider.SystemGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.Populated = true;
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
    /// A value of ReceiptResearchAssignment.
    /// </summary>
    [JsonPropertyName("receiptResearchAssignment")]
    public ReceiptResearchAssignment ReceiptResearchAssignment
    {
      get => receiptResearchAssignment ??= new();
      set => receiptResearchAssignment = value;
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

    /// <summary>
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    private ReceiptResearchAssignment receiptResearchAssignment;
    private ServiceProvider serviceProvider;
    private CashReceiptSourceType cashReceiptSourceType;
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
    /// A value of ReceiptResearchAssignment.
    /// </summary>
    [JsonPropertyName("receiptResearchAssignment")]
    public ReceiptResearchAssignment ReceiptResearchAssignment
    {
      get => receiptResearchAssignment ??= new();
      set => receiptResearchAssignment = value;
    }

    /// <summary>
    /// A value of ForCompare.
    /// </summary>
    [JsonPropertyName("forCompare")]
    public ExpireEffectiveDateAttributes ForCompare
    {
      get => forCompare ??= new();
      set => forCompare = value;
    }

    private ReceiptResearchAssignment receiptResearchAssignment;
    private ExpireEffectiveDateAttributes forCompare;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ReceiptResearchAssignment.
    /// </summary>
    [JsonPropertyName("receiptResearchAssignment")]
    public ReceiptResearchAssignment ReceiptResearchAssignment
    {
      get => receiptResearchAssignment ??= new();
      set => receiptResearchAssignment = value;
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

    /// <summary>
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    private ReceiptResearchAssignment receiptResearchAssignment;
    private ServiceProvider serviceProvider;
    private CashReceiptSourceType cashReceiptSourceType;
  }
#endregion
}
