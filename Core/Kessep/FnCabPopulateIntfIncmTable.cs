// Program: FN_CAB_POPULATE_INTF_INCM_TABLE, ID: 372708318, model: 746.
// Short name: SWE02191
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CAB_POPULATE_INTF_INCM_TABLE.
/// </para>
/// <para>
/// This CAB will read the Monthly Obligee Summary table for the last month/
/// Year. It will populate the Interface_Income_Table with the Passthru data for
/// each Obligee.
/// </para>
/// </summary>
[Serializable]
public partial class FnCabPopulateIntfIncmTable: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CAB_POPULATE_INTF_INCM_TABLE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCabPopulateIntfIncmTable(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCabPopulateIntfIncmTable.
  /// </summary>
  public FnCabPopulateIntfIncmTable(IContext context, Import import,
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
    // ---------------------------------------------------
    // Date	By	IDCR#	Description  
    // ---------------------------------------------------
    // 011598	RBM		Initial code
    // 011998	govind		Changed READ EACH to READ
    // Set DISTRIBUTION DATE to Program Processing Info                      
    // Process Date
    // 013098	govind		Set Distribution Date to Current Date
    // 081999  N.Engoor        Added an if stmnt to keep track of the count.
    // ---------------------------------------------------
    local.Current.Date = Now().Date;

    if (IsEmpty(import.Payee.Number))
    {
      ExitState = "CSE_PERSON_NO_REQUIRED";

      return;
    }

    if (!ReadCsePersonObligee())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    local.MonthlyObligeeSummary.Year = Year(import.CreateIntfRec.Date);
    local.MonthlyObligeeSummary.Month = Month(import.CreateIntfRec.Date);

    if (ReadMonthlyObligeeSummary())
    {
      if (entities.MonthlyObligeeSummary.PassthruAmount > 0)
      {
        local.Dummy.Flag = "Y";

        if (AsChar(local.Dummy.Flag) == 'Y')
        {
          for(local.RetryCount.Count = 1; local.RetryCount.Count <= 10; ++
            local.RetryCount.Count)
          {
            try
            {
              CreateInterfaceIncomeNotification();

              // OK.... Continue
              export.ExNoOfIncmIntfRecsCreated.Value =
                export.ExNoOfIncmIntfRecsCreated.Value.GetValueOrDefault() + 1;

              return;
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0000_INCOME_INTF_NOTF_PV_RB";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }

          ++local.RetryCount.Count;

          if (local.RetryCount.Count == 10)
          {
            ExitState = "LE0000_RETRY_ADD_4_AVAIL_RANDOM";
          }
        }
      }
    }
    else
    {
      ExitState = "FN0000_MTH_OBLIGEE_SUMM_NF";
    }
  }

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void CreateInterfaceIncomeNotification()
  {
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var supportedCsePersonNumber = entities.Payee.Number;
    var collectionDate = local.Zero.Date;
    var collectionAmount = entities.MonthlyObligeeSummary.PassthruAmount;
    var appliedToCode = "P";
    var distributionDate = local.Current.Date;
    var createdTimestamp = Now();
    var createdBy = global.UserId;

    entities.InterfaceIncomeNotification.Populated = false;
    Update("CreateInterfaceIncomeNotification",
      (db, command) =>
      {
        db.SetInt32(command, "intrfcIncNtfId", systemGeneratedIdentifier);
        db.SetString(command, "suppCspNumber", supportedCsePersonNumber);
        db.SetString(command, "obligorCspNumber", "");
        db.SetString(command, "caseNumb", "");
        db.SetDate(command, "collectionDate", collectionDate);
        db.SetDecimal(command, "collectionAmount", collectionAmount);
        db.SetString(command, "personProgram", "");
        db.SetString(command, "programAppliedTo", "");
        db.SetString(command, "appliedToCode", appliedToCode);
        db.SetDate(command, "distributionDate", distributionDate);
        db.SetDateTime(command, "createdTmst", createdTimestamp);
        db.SetString(command, "createdBy", createdBy);
        db.SetDate(command, "processDt", null);
      });

    entities.InterfaceIncomeNotification.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.InterfaceIncomeNotification.SupportedCsePersonNumber =
      supportedCsePersonNumber;
    entities.InterfaceIncomeNotification.ObligorCsePersonNumber = "";
    entities.InterfaceIncomeNotification.CaseNumber = "";
    entities.InterfaceIncomeNotification.CollectionDate = collectionDate;
    entities.InterfaceIncomeNotification.CollectionAmount = collectionAmount;
    entities.InterfaceIncomeNotification.PersonProgram = "";
    entities.InterfaceIncomeNotification.ProgramAppliedTo = "";
    entities.InterfaceIncomeNotification.AppliedToCode = appliedToCode;
    entities.InterfaceIncomeNotification.DistributionDate = distributionDate;
    entities.InterfaceIncomeNotification.CreatedTimestamp = createdTimestamp;
    entities.InterfaceIncomeNotification.CreatedBy = createdBy;
    entities.InterfaceIncomeNotification.ProcessDate = null;
    entities.InterfaceIncomeNotification.Populated = true;
  }

  private bool ReadCsePersonObligee()
  {
    entities.Obligee.Populated = false;
    entities.Payee.Populated = false;

    return Read("ReadCsePersonObligee",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Payee.Number);
      },
      (db, reader) =>
      {
        entities.Payee.Number = db.GetString(reader, 0);
        entities.Obligee.CspNumber = db.GetString(reader, 0);
        entities.Obligee.Type1 = db.GetString(reader, 1);
        entities.Obligee.Populated = true;
        entities.Payee.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Obligee.Type1);
      });
  }

  private bool ReadMonthlyObligeeSummary()
  {
    System.Diagnostics.Debug.Assert(entities.Obligee.Populated);
    entities.MonthlyObligeeSummary.Populated = false;

    return Read("ReadMonthlyObligeeSummary",
      (db, command) =>
      {
        db.SetString(command, "cpaSType", entities.Obligee.Type1);
        db.SetString(command, "cspSNumber", entities.Obligee.CspNumber);
        db.SetInt32(command, "yer", local.MonthlyObligeeSummary.Year);
        db.SetInt32(command, "mnth", local.MonthlyObligeeSummary.Month);
      },
      (db, reader) =>
      {
        entities.MonthlyObligeeSummary.Year = db.GetInt32(reader, 0);
        entities.MonthlyObligeeSummary.Month = db.GetInt32(reader, 1);
        entities.MonthlyObligeeSummary.PassthruAmount =
          db.GetDecimal(reader, 2);
        entities.MonthlyObligeeSummary.CreatedBy = db.GetString(reader, 3);
        entities.MonthlyObligeeSummary.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.MonthlyObligeeSummary.CpaSType = db.GetString(reader, 5);
        entities.MonthlyObligeeSummary.CspSNumber = db.GetString(reader, 6);
        entities.MonthlyObligeeSummary.Populated = true;
        CheckValid<MonthlyObligeeSummary>("CpaSType",
          entities.MonthlyObligeeSummary.CpaSType);
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of CreateIntfRec.
    /// </summary>
    [JsonPropertyName("createIntfRec")]
    public DateWorkArea CreateIntfRec
    {
      get => createIntfRec ??= new();
      set => createIntfRec = value;
    }

    /// <summary>
    /// A value of Payee.
    /// </summary>
    [JsonPropertyName("payee")]
    public CsePerson Payee
    {
      get => payee ??= new();
      set => payee = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea createIntfRec;
    private CsePerson payee;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ExNoOfIncmIntfRecsCreated.
    /// </summary>
    [JsonPropertyName("exNoOfIncmIntfRecsCreated")]
    public ProgramControlTotal ExNoOfIncmIntfRecsCreated
    {
      get => exNoOfIncmIntfRecsCreated ??= new();
      set => exNoOfIncmIntfRecsCreated = value;
    }

    /// <summary>
    /// A value of NoOfIncmIntfRecsCr.
    /// </summary>
    [JsonPropertyName("noOfIncmIntfRecsCr")]
    public Common NoOfIncmIntfRecsCr
    {
      get => noOfIncmIntfRecsCr ??= new();
      set => noOfIncmIntfRecsCr = value;
    }

    private ProgramControlTotal exNoOfIncmIntfRecsCreated;
    private Common noOfIncmIntfRecsCr;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
    }

    /// <summary>
    /// A value of Dummy.
    /// </summary>
    [JsonPropertyName("dummy")]
    public Common Dummy
    {
      get => dummy ??= new();
      set => dummy = value;
    }

    /// <summary>
    /// A value of MonthlyObligeeSummary.
    /// </summary>
    [JsonPropertyName("monthlyObligeeSummary")]
    public MonthlyObligeeSummary MonthlyObligeeSummary
    {
      get => monthlyObligeeSummary ??= new();
      set => monthlyObligeeSummary = value;
    }

    /// <summary>
    /// A value of RetryCount.
    /// </summary>
    [JsonPropertyName("retryCount")]
    public Common RetryCount
    {
      get => retryCount ??= new();
      set => retryCount = value;
    }

    private DateWorkArea current;
    private DateWorkArea zero;
    private Common dummy;
    private MonthlyObligeeSummary monthlyObligeeSummary;
    private Common retryCount;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePersonAccount Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
    }

    /// <summary>
    /// A value of Payee.
    /// </summary>
    [JsonPropertyName("payee")]
    public CsePerson Payee
    {
      get => payee ??= new();
      set => payee = value;
    }

    /// <summary>
    /// A value of InterfaceIncomeNotification.
    /// </summary>
    [JsonPropertyName("interfaceIncomeNotification")]
    public InterfaceIncomeNotification InterfaceIncomeNotification
    {
      get => interfaceIncomeNotification ??= new();
      set => interfaceIncomeNotification = value;
    }

    /// <summary>
    /// A value of MonthlyObligeeSummary.
    /// </summary>
    [JsonPropertyName("monthlyObligeeSummary")]
    public MonthlyObligeeSummary MonthlyObligeeSummary
    {
      get => monthlyObligeeSummary ??= new();
      set => monthlyObligeeSummary = value;
    }

    private CsePersonAccount obligee;
    private CsePerson payee;
    private InterfaceIncomeNotification interfaceIncomeNotification;
    private MonthlyObligeeSummary monthlyObligeeSummary;
  }
#endregion
}
