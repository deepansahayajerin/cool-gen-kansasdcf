// Program: UNASSIGN_RECEIPT_RESEARCH, ID: 371726605, model: 746.
// Short name: SWE01460
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: UNASSIGN_RECEIPT_RESEARCH.
/// </para>
/// <para>
/// RESP:  CASHMGMT
/// This process action block unassigns service provider research of cash 
/// receipt sources.
/// </para>
/// </summary>
[Serializable]
public partial class UnassignReceiptResearch: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the UNASSIGN_RECEIPT_RESEARCH program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new UnassignReceiptResearch(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of UnassignReceiptResearch.
  /// </summary>
  public UnassignReceiptResearch(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ********	EDIT AREA	********
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
    else if (AsChar(local.ForCompare.ExpirationDateIsZero) == 'Y')
    {
      ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

      return;
    }
    else if (AsChar(local.ForCompare.ExpirationDateIsLtCurrent) == 'Y')
    {
      ExitState = "EXPIRATION_DATE_PRIOR_TO_CURRENT";

      return;
    }

    // ********	WORK AREA	********
    if (ReadReceiptResearchAssignment())
    {
      try
      {
        UpdateReceiptResearchAssignment();
        ExitState = "ACO_NN0000_ALL_OK";
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_RCPT_RESEARCH_ASSGNMNT_NU";

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
      ExitState = "FN0000_RCPT_RESEARCH_ASSGNMNT_NF";
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

  private bool ReadReceiptResearchAssignment()
  {
    entities.ReceiptResearchAssignment.Populated = false;

    return Read("ReadReceiptResearchAssignment",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          import.ReceiptResearchAssignment.EffectiveDate.GetValueOrDefault());
        db.SetInt32(
          command, "cstIdentifier",
          import.CashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetString(command, "userId", import.ServiceProvider.UserId);
      },
      (db, reader) =>
      {
        entities.ReceiptResearchAssignment.SpdIdentifier =
          db.GetInt32(reader, 0);
        entities.ReceiptResearchAssignment.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ReceiptResearchAssignment.EffectiveDate =
          db.GetDate(reader, 2);
        entities.ReceiptResearchAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.ReceiptResearchAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 4);
        entities.ReceiptResearchAssignment.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 5);
        entities.ReceiptResearchAssignment.Populated = true;
      });
  }

  private void UpdateReceiptResearchAssignment()
  {
    System.Diagnostics.Debug.
      Assert(entities.ReceiptResearchAssignment.Populated);

    var discontinueDate = import.ReceiptResearchAssignment.DiscontinueDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();

    entities.ReceiptResearchAssignment.Populated = false;
    Update("UpdateReceiptResearchAssignment",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetInt32(
          command, "spdIdentifier",
          entities.ReceiptResearchAssignment.SpdIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.ReceiptResearchAssignment.CstIdentifier);
        db.SetDate(
          command, "effectiveDate",
          entities.ReceiptResearchAssignment.EffectiveDate.GetValueOrDefault());
          
      });

    entities.ReceiptResearchAssignment.DiscontinueDate = discontinueDate;
    entities.ReceiptResearchAssignment.LastUpdatedBy = lastUpdatedBy;
    entities.ReceiptResearchAssignment.LastUpdatedTmst = lastUpdatedTmst;
    entities.ReceiptResearchAssignment.Populated = true;
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

    /// <summary>
    /// A value of ReceiptResearchAssignment.
    /// </summary>
    [JsonPropertyName("receiptResearchAssignment")]
    public ReceiptResearchAssignment ReceiptResearchAssignment
    {
      get => receiptResearchAssignment ??= new();
      set => receiptResearchAssignment = value;
    }

    private ServiceProvider serviceProvider;
    private CashReceiptSourceType cashReceiptSourceType;
    private ReceiptResearchAssignment receiptResearchAssignment;
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
    /// A value of ForCompare.
    /// </summary>
    [JsonPropertyName("forCompare")]
    public ExpireEffectiveDateAttributes ForCompare
    {
      get => forCompare ??= new();
      set => forCompare = value;
    }

    private ExpireEffectiveDateAttributes forCompare;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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

    /// <summary>
    /// A value of ReceiptResearchAssignment.
    /// </summary>
    [JsonPropertyName("receiptResearchAssignment")]
    public ReceiptResearchAssignment ReceiptResearchAssignment
    {
      get => receiptResearchAssignment ??= new();
      set => receiptResearchAssignment = value;
    }

    private ServiceProvider serviceProvider;
    private CashReceiptSourceType cashReceiptSourceType;
    private ReceiptResearchAssignment receiptResearchAssignment;
  }
#endregion
}
