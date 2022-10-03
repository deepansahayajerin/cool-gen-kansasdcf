// Program: FN_DHST_LST_DISB_STAT_HISTORY, ID: 371777383, model: 746.
// Short name: SWEDHSTP
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DHST_LST_DISB_STAT_HISTORY.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnDhstLstDisbStatHistory: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DHST_LST_DISB_STAT_HISTORY program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDhstLstDisbStatHistory(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDhstLstDisbStatHistory.
  /// </summary>
  public FnDhstLstDisbStatHistory(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *********************************************
    // 12/04/96	R. Marchman		Add new security and next tran
    // 04/18/97        C. Dasgupta             Changed code and screen 
    // attributes for Display command to work correctly
    // 3/23/98		Siraj Konkader			ZDEL cleanup
    // *********************************************
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    // **** begin group A ****
    export.Hidden.Assign(import.Hidden);

    // **** end   group A ****
    export.Pacc.Date = import.Pacc.Date;
    export.PayeeCsePerson.Number = import.PayeeCsePerson.Number;
    export.PayeeCsePersonAccount.Type1 = import.PayeeCsePersonAccount.Type1;
    export.PayeeWorkArea.Text33 = import.PayeeWorkArea.Text33;

    if (import.Starting.Date != null)
    {
      export.Starting.Date = import.Starting.Date;
    }
    else
    {
      export.Starting.Date = Now().Date;
      UseCabFirstAndLastDateOfMonth();
    }

    local.Starting.TextDate =
      NumberToString(DateToInt(export.Starting.Date), 8);
    local.StartingDate.Text10 =
      Substring(local.Starting.TextDate, DateWorkArea.TextDate_MaxLength, 1, 4) +
      "-" + Substring
      (local.Starting.TextDate, DateWorkArea.TextDate_MaxLength, 5, 2) + "-" + Substring
      (local.Starting.TextDate, DateWorkArea.TextDate_MaxLength, 7, 2);
    local.Starting.Timestamp = Timestamp(local.StartingDate.Text10);
    export.DisbursementTransaction.Assign(import.DisbursementTransaction);
    export.DisbursementType.Code = import.DisbursementType.Code;

    if (Equal(global.Command, "DISPLAY"))
    {
    }
    else if (!import.Import1.IsEmpty)
    {
      export.Export1.Index = 0;
      export.Export1.Clear();

      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        if (export.Export1.IsFull)
        {
          break;
        }

        export.Export1.Update.DisbursementStatusHistory.CreatedBy =
          import.Import1.Item.DisbursementStatusHistory.CreatedBy;
        MoveDateWorkArea(import.Import1.Item.DateWorkArea,
          export.Export1.Update.DateWorkArea);
        export.Export1.Update.DisbursementStatus.Code =
          import.Import1.Item.DisbursementStatus.Code;
        export.Export1.Next();
      }
    }

    local.LeftPadding.Text10 = export.PayeeCsePerson.Number;
    UseEabPadLeftWithZeros();
    export.PayeeCsePerson.Number = local.LeftPadding.Text10;

    // **** begin group B ****
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      export.Hidden.CsePersonNumberObligee = import.PayeeCsePerson.Number;

      // ****
      // ****
      export.Hidden.CsePersonNumberObligee = export.PayeeCsePerson.Number;
      export.Hidden.CsePersonNumber = export.PayeeCsePerson.Number;
      UseScCabNextTranPut();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ****
      // this is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
      // ****
      UseScCabNextTranGet();

      if (!IsEmpty(export.Hidden.CsePersonNumberObligee))
      {
        export.PayeeCsePerson.Number = export.Hidden.CsePersonNumberObligee ?? Spaces
          (10);
      }
      else
      {
        export.PayeeCsePerson.Number = export.Hidden.CsePersonNumber ?? Spaces
          (10);
      }

      ExitState = "FN0000_FLOW_ONLY_FROM_PACC";

      return;
    }

    // **** end   group B ****
    if (Equal(global.Command, "PACC"))
    {
    }
    else
    {
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        // !!!!!  Add logic to get the Payee name
        if (IsEmpty(export.PayeeCsePerson.Number))
        {
          var field = GetField(export.PayeeCsePerson, "number");

          field.Error = true;

          ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";

          return;
        }

        local.CsePersonsWorkSet.Number = export.PayeeCsePerson.Number;
        UseEabReadCsePerson();

        if (!IsEmpty(local.Ae.Flag))
        {
          local.Ae.Flag = "";
        }

        if (IsEmpty(local.Ae.Flag))
        {
          UseSiFormatCsePersonName();
          export.PayeeWorkArea.Text33 = local.CsePersonsWorkSet.FormattedName;
        }
        else
        {
        }

        if (ReadDisbursementTransactionDisbursement())
        {
          if (ReadDisbursementType())
          {
            export.DisbursementType.Code = entities.DisbursementType.Code;
          }

          if (ReadCsePerson())
          {
            export.DesigPayeeCsePerson.Number = entities.DesigPayee.Number;
            local.CsePersonsWorkSet.Number = entities.DesigPayee.Number;
            UseEabReadCsePerson();

            if (IsEmpty(local.Ae.Flag))
            {
              UseSiFormatCsePersonName();
              export.DesigPayeeWorkArea.Text33 =
                local.CsePersonsWorkSet.FormattedName;
            }
          }
          else
          {
            export.DesigPayeeCsePerson.Number = "";
            export.DesigPayeeWorkArea.Text33 = "";
          }

          // ---------------------------------------------
          // MTW-Chayan  04/18/97  Change Start
          // Made changes to display status history
          // starting from start date onwards
          // ---------------------------------------------
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadDisbursementStatusHistory())
          {
            // ---------------------------------------------
            // MTW-Chayan  04/18/97  Change End
            // ---------------------------------------------
            export.Export1.Update.DisbursementStatusHistory.CreatedBy =
              entities.DisbursementStatusHistory.CreatedBy;
            export.Export1.Update.DateWorkArea.Date =
              Date(entities.DisbursementStatusHistory.CreatedTimestamp);
            export.Export1.Update.DateWorkArea.Time =
              TimeOfDay(entities.DisbursementStatusHistory.CreatedTimestamp).
                GetValueOrDefault();

            if (ReadDisbursementStatus())
            {
              export.Export1.Update.DisbursementStatus.Code =
                entities.DisbursementStatus.Code;
            }
            else
            {
              ExitState = "FN0000_DISB_STAT_NF";
              export.Export1.Next();

              return;
            }

            export.Export1.Next();
          }

          if (export.Export1.IsEmpty)
          {
            ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";

            return;
          }

          if (export.Export1.IsFull)
          {
            ExitState = "FN0000_GROUP_VIEW_OVERFLOW";

            return;
          }

          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }
        else
        {
          ExitState = "FN0000_DISB_TRANS_NF";
        }

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "EXIT":
        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "PACC":
        // *** Transfer to "List Payee Account" screen passing the Cse_person 
        // number and command set to DISPLAY
        ExitState = "ECO_XFR_TO_LST_PAYEE_ACCT";

        break;
      default:
        if (IsEmpty(global.Command))
        {
          ExitState = "FN0000_FLOW_ONLY_FROM_PACC";

          return;
        }

        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Time = source.Time;
  }

  private static void MoveNextTranInfo(NextTranInfo source, NextTranInfo target)
  {
    target.LegalActionIdentifier = source.LegalActionIdentifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CsePersonNumberAp = source.CsePersonNumberAp;
    target.CsePersonNumberObligee = source.CsePersonNumberObligee;
    target.CsePersonNumberObligor = source.CsePersonNumberObligor;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ObligationId = source.ObligationId;
    target.StandardCrtOrdNumber = source.StandardCrtOrdNumber;
    target.InfrastructureId = source.InfrastructureId;
    target.MiscText1 = source.MiscText1;
    target.MiscText2 = source.MiscText2;
    target.MiscNum1 = source.MiscNum1;
    target.MiscNum2 = source.MiscNum2;
    target.MiscNum1V2 = source.MiscNum1V2;
    target.MiscNum2V2 = source.MiscNum2V2;
  }

  private void UseCabFirstAndLastDateOfMonth()
  {
    var useImport = new CabFirstAndLastDateOfMonth.Import();
    var useExport = new CabFirstAndLastDateOfMonth.Export();

    useImport.DateWorkArea.Date = export.Starting.Date;

    Call(CabFirstAndLastDateOfMonth.Execute, useImport, useExport);

    export.Starting.Date = useExport.First.Date;
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.LeftPadding.Text10;
    useExport.TextWorkArea.Text10 = local.LeftPadding.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.LeftPadding.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseEabReadCsePerson()
  {
    var useImport = new EabReadCsePerson.Import();
    var useExport = new EabReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    useExport.Ae.Flag = local.Ae.Flag;
    MoveCsePersonsWorkSet(local.CsePersonsWorkSet, useExport.CsePersonsWorkSet);

    Call(EabReadCsePerson.Execute, useImport, useExport);

    local.Ae.Flag = useExport.Ae.Flag;
    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.Hidden.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.Hidden, useImport.NextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabSignoff()
  {
    var useImport = new ScCabSignoff.Import();
    var useExport = new ScCabSignoff.Export();

    Call(ScCabSignoff.Execute, useImport, useExport);
  }

  private void UseScCabTestSecurity()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    useImport.CsePerson.Number = import.PayeeCsePerson.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiFormatCsePersonName()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    local.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private bool ReadCsePerson()
  {
    entities.DesigPayee.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "csePersoNum", export.PayeeCsePerson.Number);
        db.SetDate(
          command, "effectiveDate",
          entities.DisbursementTransaction.DisbursementDate.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DesigPayee.Number = db.GetString(reader, 0);
        entities.DesigPayee.Populated = true;
      });
  }

  private bool ReadDisbursementStatus()
  {
    System.Diagnostics.Debug.
      Assert(entities.DisbursementStatusHistory.Populated);
    entities.DisbursementStatus.Populated = false;

    return Read("ReadDisbursementStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbStatusId",
          entities.DisbursementStatusHistory.DbsGeneratedId);
      },
      (db, reader) =>
      {
        entities.DisbursementStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementStatus.Code = db.GetString(reader, 1);
        entities.DisbursementStatus.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDisbursementStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.Disbursement.Populated);

    return ReadEach("ReadDisbursementStatusHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtrGeneratedId",
          entities.Disbursement.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Disbursement.CspNumber);
        db.SetString(command, "cpaType", entities.Disbursement.CpaType);
        db.SetDateTime(
          command, "createdTimestamp",
          local.Starting.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.DisbursementStatusHistory.DbsGeneratedId =
          db.GetInt32(reader, 0);
        entities.DisbursementStatusHistory.DtrGeneratedId =
          db.GetInt32(reader, 1);
        entities.DisbursementStatusHistory.CspNumber = db.GetString(reader, 2);
        entities.DisbursementStatusHistory.CpaType = db.GetString(reader, 3);
        entities.DisbursementStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.DisbursementStatusHistory.CreatedBy = db.GetString(reader, 5);
        entities.DisbursementStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.DisbursementStatusHistory.Populated = true;
        CheckValid<DisbursementStatusHistory>("CpaType",
          entities.DisbursementStatusHistory.CpaType);

        return true;
      });
  }

  private bool ReadDisbursementTransactionDisbursement()
  {
    entities.Disbursement.Populated = false;
    entities.DisbursementTransaction.Populated = false;

    return Read("ReadDisbursementTransactionDisbursement",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbTranId",
          import.DisbursementTransaction.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", import.PayeeCsePersonAccount.Type1);
        db.SetString(command, "cspNumber", export.PayeeCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.DisbursementTransaction.CpaType = db.GetString(reader, 0);
        entities.DisbursementTransaction.CspNumber = db.GetString(reader, 1);
        entities.DisbursementTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbursementTransaction.Type1 = db.GetString(reader, 3);
        entities.DisbursementTransaction.Amount = db.GetDecimal(reader, 4);
        entities.DisbursementTransaction.DisbursementDate =
          db.GetNullableDate(reader, 5);
        entities.DisbursementTransaction.DbtGeneratedId =
          db.GetNullableInt32(reader, 6);
        entities.Disbursement.CpaType = db.GetString(reader, 7);
        entities.Disbursement.CspNumber = db.GetString(reader, 8);
        entities.Disbursement.SystemGeneratedIdentifier =
          db.GetInt32(reader, 9);
        entities.Disbursement.Type1 = db.GetString(reader, 10);
        entities.Disbursement.Amount = db.GetDecimal(reader, 11);
        entities.Disbursement.ProcessDate = db.GetNullableDate(reader, 12);
        entities.Disbursement.CreatedBy = db.GetString(reader, 13);
        entities.Disbursement.CreatedTimestamp = db.GetDateTime(reader, 14);
        entities.Disbursement.LastUpdatedBy = db.GetNullableString(reader, 15);
        entities.Disbursement.LastUpdateTmst =
          db.GetNullableDateTime(reader, 16);
        entities.Disbursement.DisbursementDate = db.GetNullableDate(reader, 17);
        entities.Disbursement.CashNonCashInd = db.GetString(reader, 18);
        entities.Disbursement.RecapturedInd = db.GetString(reader, 19);
        entities.Disbursement.CollectionDate = db.GetNullableDate(reader, 20);
        entities.Disbursement.DbtGeneratedId = db.GetNullableInt32(reader, 21);
        entities.Disbursement.InterstateInd = db.GetNullableString(reader, 22);
        entities.Disbursement.PassthruProcDate = db.GetNullableDate(reader, 23);
        entities.Disbursement.DesignatedPayee =
          db.GetNullableString(reader, 24);
        entities.Disbursement.ReferenceNumber =
          db.GetNullableString(reader, 25);
        entities.Disbursement.Populated = true;
        entities.DisbursementTransaction.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.DisbursementTransaction.CpaType);
        CheckValid<DisbursementTransaction>("Type1",
          entities.DisbursementTransaction.Type1);
        CheckValid<DisbursementTransaction>("CpaType",
          entities.Disbursement.CpaType);
        CheckValid<DisbursementTransaction>("Type1", entities.Disbursement.Type1);
          
        CheckValid<DisbursementTransaction>("CashNonCashInd",
          entities.Disbursement.CashNonCashInd);
      });
  }

  private bool ReadDisbursementType()
  {
    System.Diagnostics.Debug.Assert(entities.Disbursement.Populated);
    entities.DisbursementType.Populated = false;

    return Read("ReadDisbursementType",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbTypeId",
          entities.Disbursement.DbtGeneratedId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DisbursementType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementType.Code = db.GetString(reader, 1);
        entities.DisbursementType.Populated = true;
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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of DateWorkArea.
      /// </summary>
      [JsonPropertyName("dateWorkArea")]
      public DateWorkArea DateWorkArea
      {
        get => dateWorkArea ??= new();
        set => dateWorkArea = value;
      }

      /// <summary>
      /// A value of DisbursementStatus.
      /// </summary>
      [JsonPropertyName("disbursementStatus")]
      public DisbursementStatus DisbursementStatus
      {
        get => disbursementStatus ??= new();
        set => disbursementStatus = value;
      }

      /// <summary>
      /// A value of DisbursementStatusHistory.
      /// </summary>
      [JsonPropertyName("disbursementStatusHistory")]
      public DisbursementStatusHistory DisbursementStatusHistory
      {
        get => disbursementStatusHistory ??= new();
        set => disbursementStatusHistory = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private DateWorkArea dateWorkArea;
      private DisbursementStatus disbursementStatus;
      private DisbursementStatusHistory disbursementStatusHistory;
    }

    /// <summary>
    /// A value of Pacc.
    /// </summary>
    [JsonPropertyName("pacc")]
    public DateWorkArea Pacc
    {
      get => pacc ??= new();
      set => pacc = value;
    }

    /// <summary>
    /// A value of Disbursement.
    /// </summary>
    [JsonPropertyName("disbursement")]
    public DisbursementTransaction Disbursement
    {
      get => disbursement ??= new();
      set => disbursement = value;
    }

    /// <summary>
    /// A value of DesigPayeeWorkArea.
    /// </summary>
    [JsonPropertyName("desigPayeeWorkArea")]
    public WorkArea DesigPayeeWorkArea
    {
      get => desigPayeeWorkArea ??= new();
      set => desigPayeeWorkArea = value;
    }

    /// <summary>
    /// A value of DesigPayeeCsePerson.
    /// </summary>
    [JsonPropertyName("desigPayeeCsePerson")]
    public CsePerson DesigPayeeCsePerson
    {
      get => desigPayeeCsePerson ??= new();
      set => desigPayeeCsePerson = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public DateWorkArea Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of PayeeCsePerson.
    /// </summary>
    [JsonPropertyName("payeeCsePerson")]
    public CsePerson PayeeCsePerson
    {
      get => payeeCsePerson ??= new();
      set => payeeCsePerson = value;
    }

    /// <summary>
    /// A value of PayeeCsePersonAccount.
    /// </summary>
    [JsonPropertyName("payeeCsePersonAccount")]
    public CsePersonAccount PayeeCsePersonAccount
    {
      get => payeeCsePersonAccount ??= new();
      set => payeeCsePersonAccount = value;
    }

    /// <summary>
    /// A value of PayeeWorkArea.
    /// </summary>
    [JsonPropertyName("payeeWorkArea")]
    public WorkArea PayeeWorkArea
    {
      get => payeeWorkArea ??= new();
      set => payeeWorkArea = value;
    }

    /// <summary>
    /// A value of DisbursementTransaction.
    /// </summary>
    [JsonPropertyName("disbursementTransaction")]
    public DisbursementTransaction DisbursementTransaction
    {
      get => disbursementTransaction ??= new();
      set => disbursementTransaction = value;
    }

    /// <summary>
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
    }

    /// <summary>
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 => import1 ??= new(ImportGroup.Capacity);

    /// <summary>
    /// Gets a value of Import1 for json serialization.
    /// </summary>
    [JsonPropertyName("import1")]
    [Computed]
    public IList<ImportGroup> Import1_Json
    {
      get => import1;
      set => Import1.Assign(value);
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    private DateWorkArea pacc;
    private DisbursementTransaction disbursement;
    private WorkArea desigPayeeWorkArea;
    private CsePerson desigPayeeCsePerson;
    private DateWorkArea starting;
    private CsePerson payeeCsePerson;
    private CsePersonAccount payeeCsePersonAccount;
    private WorkArea payeeWorkArea;
    private DisbursementTransaction disbursementTransaction;
    private DisbursementType disbursementType;
    private Array<ImportGroup> import1;
    private NextTranInfo hidden;
    private Standard standard;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of DateWorkArea.
      /// </summary>
      [JsonPropertyName("dateWorkArea")]
      public DateWorkArea DateWorkArea
      {
        get => dateWorkArea ??= new();
        set => dateWorkArea = value;
      }

      /// <summary>
      /// A value of DisbursementStatus.
      /// </summary>
      [JsonPropertyName("disbursementStatus")]
      public DisbursementStatus DisbursementStatus
      {
        get => disbursementStatus ??= new();
        set => disbursementStatus = value;
      }

      /// <summary>
      /// A value of DisbursementStatusHistory.
      /// </summary>
      [JsonPropertyName("disbursementStatusHistory")]
      public DisbursementStatusHistory DisbursementStatusHistory
      {
        get => disbursementStatusHistory ??= new();
        set => disbursementStatusHistory = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private DateWorkArea dateWorkArea;
      private DisbursementStatus disbursementStatus;
      private DisbursementStatusHistory disbursementStatusHistory;
    }

    /// <summary>
    /// A value of Pacc.
    /// </summary>
    [JsonPropertyName("pacc")]
    public DateWorkArea Pacc
    {
      get => pacc ??= new();
      set => pacc = value;
    }

    /// <summary>
    /// A value of DesigPayeeWorkArea.
    /// </summary>
    [JsonPropertyName("desigPayeeWorkArea")]
    public WorkArea DesigPayeeWorkArea
    {
      get => desigPayeeWorkArea ??= new();
      set => desigPayeeWorkArea = value;
    }

    /// <summary>
    /// A value of DesigPayeeCsePerson.
    /// </summary>
    [JsonPropertyName("desigPayeeCsePerson")]
    public CsePerson DesigPayeeCsePerson
    {
      get => desigPayeeCsePerson ??= new();
      set => desigPayeeCsePerson = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public DateWorkArea Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of PayeeCsePerson.
    /// </summary>
    [JsonPropertyName("payeeCsePerson")]
    public CsePerson PayeeCsePerson
    {
      get => payeeCsePerson ??= new();
      set => payeeCsePerson = value;
    }

    /// <summary>
    /// A value of PayeeCsePersonAccount.
    /// </summary>
    [JsonPropertyName("payeeCsePersonAccount")]
    public CsePersonAccount PayeeCsePersonAccount
    {
      get => payeeCsePersonAccount ??= new();
      set => payeeCsePersonAccount = value;
    }

    /// <summary>
    /// A value of PayeeWorkArea.
    /// </summary>
    [JsonPropertyName("payeeWorkArea")]
    public WorkArea PayeeWorkArea
    {
      get => payeeWorkArea ??= new();
      set => payeeWorkArea = value;
    }

    /// <summary>
    /// A value of DisbursementTransaction.
    /// </summary>
    [JsonPropertyName("disbursementTransaction")]
    public DisbursementTransaction DisbursementTransaction
    {
      get => disbursementTransaction ??= new();
      set => disbursementTransaction = value;
    }

    /// <summary>
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 => export1 ??= new(ExportGroup.Capacity);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    private DateWorkArea pacc;
    private WorkArea desigPayeeWorkArea;
    private CsePerson desigPayeeCsePerson;
    private DateWorkArea starting;
    private CsePerson payeeCsePerson;
    private CsePersonAccount payeeCsePersonAccount;
    private WorkArea payeeWorkArea;
    private DisbursementTransaction disbursementTransaction;
    private DisbursementType disbursementType;
    private Array<ExportGroup> export1;
    private NextTranInfo hidden;
    private Standard standard;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of StartingDate.
    /// </summary>
    [JsonPropertyName("startingDate")]
    public TextWorkArea StartingDate
    {
      get => startingDate ??= new();
      set => startingDate = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public DateWorkArea Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of LeftPadding.
    /// </summary>
    [JsonPropertyName("leftPadding")]
    public TextWorkArea LeftPadding
    {
      get => leftPadding ??= new();
      set => leftPadding = value;
    }

    /// <summary>
    /// A value of Ae.
    /// </summary>
    [JsonPropertyName("ae")]
    public Common Ae
    {
      get => ae ??= new();
      set => ae = value;
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

    private TextWorkArea startingDate;
    private DateWorkArea starting;
    private TextWorkArea leftPadding;
    private Common ae;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DisbursementTransactionRln.
    /// </summary>
    [JsonPropertyName("disbursementTransactionRln")]
    public DisbursementTransactionRln DisbursementTransactionRln
    {
      get => disbursementTransactionRln ??= new();
      set => disbursementTransactionRln = value;
    }

    /// <summary>
    /// A value of Disbursement.
    /// </summary>
    [JsonPropertyName("disbursement")]
    public DisbursementTransaction Disbursement
    {
      get => disbursement ??= new();
      set => disbursement = value;
    }

    /// <summary>
    /// A value of CsePersonDesigPayee.
    /// </summary>
    [JsonPropertyName("csePersonDesigPayee")]
    public CsePersonDesigPayee CsePersonDesigPayee
    {
      get => csePersonDesigPayee ??= new();
      set => csePersonDesigPayee = value;
    }

    /// <summary>
    /// A value of DesigPayee.
    /// </summary>
    [JsonPropertyName("desigPayee")]
    public CsePerson DesigPayee
    {
      get => desigPayee ??= new();
      set => desigPayee = value;
    }

    /// <summary>
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    /// <summary>
    /// A value of DisbursementTransaction.
    /// </summary>
    [JsonPropertyName("disbursementTransaction")]
    public DisbursementTransaction DisbursementTransaction
    {
      get => disbursementTransaction ??= new();
      set => disbursementTransaction = value;
    }

    /// <summary>
    /// A value of DisbursementStatusHistory.
    /// </summary>
    [JsonPropertyName("disbursementStatusHistory")]
    public DisbursementStatusHistory DisbursementStatusHistory
    {
      get => disbursementStatusHistory ??= new();
      set => disbursementStatusHistory = value;
    }

    /// <summary>
    /// A value of DisbursementStatus.
    /// </summary>
    [JsonPropertyName("disbursementStatus")]
    public DisbursementStatus DisbursementStatus
    {
      get => disbursementStatus ??= new();
      set => disbursementStatus = value;
    }

    private DisbursementTransactionRln disbursementTransactionRln;
    private DisbursementTransaction disbursement;
    private CsePersonDesigPayee csePersonDesigPayee;
    private CsePerson desigPayee;
    private DisbursementType disbursementType;
    private CsePerson csePerson;
    private CsePersonAccount csePersonAccount;
    private DisbursementTransaction disbursementTransaction;
    private DisbursementStatusHistory disbursementStatusHistory;
    private DisbursementStatus disbursementStatus;
  }
#endregion
}
