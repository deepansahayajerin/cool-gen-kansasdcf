// Program: FN_NOTIFY_AE_OF_COLLECTIONS, ID: 372656433, model: 746.
// Short name: SWE01768
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_NOTIFY_AE_OF_COLLECTIONS.
/// </para>
/// <para>
/// This process builds an interface record for AE for collections applied to 
/// debts with a supported person active on a public assistance program.  If the
/// collection was applied to a qualifying debt that was an interstate debt or
/// a concurrent obligation the collection is bypassed.  If the collection has
/// been adjusted off or already been sent to AE, it is also bypassed.
/// Qualifying programs are:  AF, MA, MK, MS, MP, NF, FS, SI
/// </para>
/// </summary>
[Serializable]
public partial class FnNotifyAeOfCollections: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_NOTIFY_AE_OF_COLLECTIONS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnNotifyAeOfCollections(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnNotifyAeOfCollections.
  /// </summary>
  public FnNotifyAeOfCollections(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // =================================================
    // 5/28/99 - bud adams  -  starved views and deleted unused
    //   views; deactivated 3 reads and a USE statement;
    // 11/14/00 - swsrkxd - pr 102420 - B640 will now report AF
    // payments only.
    // =================================================
    // ===============================================
    // 10/05/2000 -  Case No is no longer required. Remove logic
    // to retrieve case no.
    // ===============================================
    local.CreateTries.Count = 0;

    // *** Write interface record ***
    do
    {
      // *** Get another random number for interface record id
      UseGenerate9DigitRandomNumber();
      ++local.CreateTries.Count;

      try
      {
        CreateInterfaceIncomeNotification();
        ExitState = "ACO_NI0000_CREATE_OK";
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            if (local.CreateTries.Count >= 5)
            {
              ExitState = "FN0000_INTF_INCOME_NOT_CREATED";
              export.EabReportSend.RptDetail =
                "Income record not created.  " + "Obligor Person No " + import
                .Obligor.Number + ".  Supported Person No " + import
                .Supported.Number + ".  Collection Id " + NumberToString
                (import.P.SystemGeneratedIdentifier, 15) + ".";

              return;
            }

            break;
          case ErrorCode.PermittedValueViolation:
            // *** There are no permitted values on the Interface Income 
            // Notification table
            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    while(!IsExitState("ACO_NI0000_CREATE_OK"));

    ExitState = "ACO_NN0000_ALL_OK";

    // *****Update collection records ae_notified_date to current date so record
    // will never be processed again by this program.
    try
    {
      UpdateCollection();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0000_COLLECTION_NU";
          export.EabReportSend.RptDetail = "Collection not unique.  " + "Obligor Person No " +
            import.Obligor.Number + ".  Supported Person No " + import
            .Supported.Number + ".  Collection Id " + NumberToString
            (import.P.SystemGeneratedIdentifier, 15) + ".";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0000_COLLECTION_PV";
          export.EabReportSend.RptDetail =
            "Coll permitted value violation.  " + "Obligor Person No " + import
            .Obligor.Number + ".  Supported Person No " + import
            .Supported.Number + ".  Collection Id " + NumberToString
            (import.P.SystemGeneratedIdentifier, 15) + ".";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    local.SystemGenerated.Attribute9DigitRandomNumber =
      useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void CreateInterfaceIncomeNotification()
  {
    var systemGeneratedIdentifier =
      local.SystemGenerated.Attribute9DigitRandomNumber;
    var supportedCsePersonNumber = import.Supported.Number;
    var obligorCsePersonNumber = import.Obligor.Number;
    var collectionDate = import.P.CollectionDt;
    var collectionAmount = import.P.Amount;
    var personProgram = "AF";
    var programAppliedTo = Substring(import.P.ProgramAppliedTo, 1, 2);
    var appliedToCode = import.P.AppliedToCode;
    var distributionDate = Date(import.P.CreatedTmst);
    var createdTimestamp = Now();
    var createdBy = global.UserId;
    var processDate = local.Initialize.Date;

    entities.InterfaceIncomeNotification.Populated = false;
    Update("CreateInterfaceIncomeNotification",
      (db, command) =>
      {
        db.SetInt32(command, "intrfcIncNtfId", systemGeneratedIdentifier);
        db.SetString(command, "suppCspNumber", supportedCsePersonNumber);
        db.SetString(command, "obligorCspNumber", obligorCsePersonNumber);
        db.SetString(command, "caseNumb", "");
        db.SetDate(command, "collectionDate", collectionDate);
        db.SetDecimal(command, "collectionAmount", collectionAmount);
        db.SetString(command, "personProgram", personProgram);
        db.SetString(command, "programAppliedTo", programAppliedTo);
        db.SetString(command, "appliedToCode", appliedToCode);
        db.SetDate(command, "distributionDate", distributionDate);
        db.SetDateTime(command, "createdTmst", createdTimestamp);
        db.SetString(command, "createdBy", createdBy);
        db.SetDate(command, "processDt", processDate);
      });

    entities.InterfaceIncomeNotification.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.InterfaceIncomeNotification.SupportedCsePersonNumber =
      supportedCsePersonNumber;
    entities.InterfaceIncomeNotification.ObligorCsePersonNumber =
      obligorCsePersonNumber;
    entities.InterfaceIncomeNotification.CaseNumber = "";
    entities.InterfaceIncomeNotification.CollectionDate = collectionDate;
    entities.InterfaceIncomeNotification.CollectionAmount = collectionAmount;
    entities.InterfaceIncomeNotification.PersonProgram = personProgram;
    entities.InterfaceIncomeNotification.ProgramAppliedTo = programAppliedTo;
    entities.InterfaceIncomeNotification.AppliedToCode = appliedToCode;
    entities.InterfaceIncomeNotification.DistributionDate = distributionDate;
    entities.InterfaceIncomeNotification.CreatedTimestamp = createdTimestamp;
    entities.InterfaceIncomeNotification.CreatedBy = createdBy;
    entities.InterfaceIncomeNotification.ProcessDate = processDate;
    entities.InterfaceIncomeNotification.Populated = true;
  }

  private void UpdateCollection()
  {
    System.Diagnostics.Debug.Assert(import.P.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var aeNotifiedDate = import.Current.Date;

    import.P.Populated = false;
    Update("UpdateCollection",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableDate(command, "aeNotifiedDt", aeNotifiedDate);
        db.SetInt32(command, "collId", import.P.SystemGeneratedIdentifier);
        db.SetInt32(command, "crtType", import.P.CrtType);
        db.SetInt32(command, "cstId", import.P.CstId);
        db.SetInt32(command, "crvId", import.P.CrvId);
        db.SetInt32(command, "crdId", import.P.CrdId);
        db.SetInt32(command, "obgId", import.P.ObgId);
        db.SetString(command, "cspNumber", import.P.CspNumber);
        db.SetString(command, "cpaType", import.P.CpaType);
        db.SetInt32(command, "otrId", import.P.OtrId);
        db.SetString(command, "otrType", import.P.OtrType);
        db.SetInt32(command, "otyId", import.P.OtyId);
      });

    import.P.LastUpdatedBy = lastUpdatedBy;
    import.P.LastUpdatedTmst = lastUpdatedTmst;
    import.P.AeNotifiedDate = aeNotifiedDate;
    import.P.Populated = true;
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
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePerson Supported
    {
      get => supported ??= new();
      set => supported = value;
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

    /// <summary>
    /// A value of P.
    /// </summary>
    [JsonPropertyName("p")]
    public Collection P
    {
      get => p ??= new();
      set => p = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    private CsePerson supported;
    private DateWorkArea current;
    private Collection p;
    private CsePerson obligor;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    private EabReportSend eabReportSend;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Initialize.
    /// </summary>
    [JsonPropertyName("initialize")]
    public DateWorkArea Initialize
    {
      get => initialize ??= new();
      set => initialize = value;
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
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of SystemGenerated.
    /// </summary>
    [JsonPropertyName("systemGenerated")]
    public SystemGenerated SystemGenerated
    {
      get => systemGenerated ??= new();
      set => systemGenerated = value;
    }

    /// <summary>
    /// A value of CreateTries.
    /// </summary>
    [JsonPropertyName("createTries")]
    public Common CreateTries
    {
      get => createTries ??= new();
      set => createTries = value;
    }

    private DateWorkArea initialize;
    private ObligationType obligationType;
    private Program program;
    private SystemGenerated systemGenerated;
    private Common createTries;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InterfaceIncomeNotification.
    /// </summary>
    [JsonPropertyName("interfaceIncomeNotification")]
    public InterfaceIncomeNotification InterfaceIncomeNotification
    {
      get => interfaceIncomeNotification ??= new();
      set => interfaceIncomeNotification = value;
    }

    private InterfaceIncomeNotification interfaceIncomeNotification;
  }
#endregion
}
