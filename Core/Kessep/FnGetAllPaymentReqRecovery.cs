// Program: FN_GET_ALL_PAYMENT_REQ_RECOVERY, ID: 372046585, model: 746.
// Short name: SWE00052
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_GET_ALL_PAYMENT_REQ_RECOVERY.
/// </para>
/// <para>
/// RESP: Finance
/// </para>
/// </summary>
[Serializable]
public partial class FnGetAllPaymentReqRecovery: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_GET_ALL_PAYMENT_REQ_RECOVERY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnGetAllPaymentReqRecovery(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnGetAllPaymentReqRecovery.
  /// </summary>
  public FnGetAllPaymentReqRecovery(IContext context, Import import,
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
    // ***************************************************
    // A. Kinney	04/28/97	Change Current_date
    // 09/30/97	A Samuels	IDCR 347
    // 12/13/99        G Sharp         Phase2   1. changes using payment_request
    // process_ date to created_timestampon both read eachs.
    // ***************************************************
    // ****************************************************************
    // The following IF statements will set the local to and from
    // search dates for payment request.
    // ****************************************************************
    // =================================================
    // 6/15/99 - Bud Adams  -  changed logic to search on timestamp
    //   value.  This does away with the need to perform the DATETIMESTAMP
    //   function on each row retrieved from the database as was
    //   necessary when the logic was based on Date.
    // PR# 229: 8/24/99 - bud adams  -  Building of timestamp value
    //   failed due to domain of work attributes used.
    // =================================================
    // ================================================
    // 12/15/99 - SWSRKXD PR#82051
    // Performance change - Split READ EACHes to remove 'OR'
    // clauses.
    // 12/21/99 - SWSRKXD - PR#82734
    // Increase cardinality of group view to 120.
    // ================================================
    if (Equal(import.SearchFrom.ProcessDate, local.Null1.Date))
    {
      local.SearchFrom.CreatedTimestamp = new DateTime(1, 1, 1);
    }
    else
    {
      local.WorkYear.LastTran =
        NumberToString(Year(import.SearchFrom.ProcessDate), 4);
      local.WorkMonth.LastTran =
        NumberToString(Month(import.SearchFrom.ProcessDate), 4);
      local.WorkDay.LastTran =
        NumberToString(Day(import.SearchFrom.ProcessDate), 4);
      local.WorkTimestamp.DeleteText = (local.WorkYear.LastTran ?? "") + "-" + Substring
        (local.WorkMonth.LastTran, 4, 3, 2) + "-" + Substring
        (local.WorkDay.LastTran, 4, 3, 2) + "-00.00.00.000000";
      local.SearchFrom.CreatedTimestamp =
        Timestamp(Substring(
          local.WorkTimestamp.DeleteText, Standard.DeleteText_MaxLength, 1,
        26));
    }

    if (Equal(import.SearchTo.ProcessDate, local.Null1.Date))
    {
      local.SearchTo.CreatedTimestamp =
        AddMicroseconds(new DateTime(2099, 12, 31, 23, 59, 59), 999999);
    }
    else
    {
      local.WorkYear.LastTran =
        NumberToString(Year(import.SearchTo.ProcessDate), 4);
      local.WorkMonth.LastTran =
        NumberToString(Month(import.SearchTo.ProcessDate), 4);
      local.WorkDay.LastTran =
        NumberToString(Day(import.SearchTo.ProcessDate), 4);
      local.WorkTimestamp.DeleteText = (local.WorkYear.LastTran ?? "") + "-" + Substring
        (local.WorkMonth.LastTran, 4, 3, 2) + "-" + Substring
        (local.WorkDay.LastTran, 4, 3, 2) + "-23.59.59.999999";
      local.SearchTo.CreatedTimestamp =
        Timestamp(Substring(
          local.WorkTimestamp.DeleteText, Standard.DeleteText_MaxLength, 1,
        26));
    }

    if (AsChar(import.History.Flag) == 'Y')
    {
      if (IsEmpty(import.SearchObligee.Number))
      {
        export.Group.Index = 0;
        export.Group.Clear();

        foreach(var item in ReadPaymentRequestPaymentStatusPaymentStatusHistory4())
          
        {
          if (!IsEmpty(import.PaymentStatusHistory.CreatedBy) && !
            Equal(import.PaymentStatusHistory.CreatedBy,
            entities.PaymentStatusHistory.CreatedBy))
          {
            export.Group.Next();

            continue;
          }

          if (!IsEmpty(import.Search.Code) && !
            Equal(import.Search.Code, entities.PaymentStatus.Code))
          {
            export.Group.Next();

            continue;
          }

          local.CsePersonsWorkSet.Number =
            entities.PaymentRequest.CsePersonNumber ?? Spaces(10);
          UseSiReadCsePerson();

          if (IsExitState("CSE_PERSON_NF"))
          {
            ExitState = "ACO_NN0000_ALL_OK";
          }

          MoveCsePersonsWorkSet(local.CsePersonsWorkSet,
            export.Group.Update.CsePersonsWorkSet);
          export.Group.Update.PaymentRequest.Assign(entities.PaymentRequest);
          export.Group.Update.PaymentStatus.Code = entities.PaymentStatus.Code;
          MovePaymentStatusHistory(entities.PaymentStatusHistory,
            export.Group.Update.PaymentStatusHistory);
          export.Group.Update.PayHistReasonText.Text76 =
            Substring(entities.PaymentStatusHistory.ReasonText, 1, 76);
          export.Group.Next();
        }
      }
      else
      {
        export.Group.Index = 0;
        export.Group.Clear();

        foreach(var item in ReadPaymentRequestPaymentStatusPaymentStatusHistory3())
          
        {
          if (!IsEmpty(import.PaymentStatusHistory.CreatedBy) && !
            Equal(import.PaymentStatusHistory.CreatedBy,
            entities.PaymentStatusHistory.CreatedBy))
          {
            export.Group.Next();

            continue;
          }

          if (!IsEmpty(import.Search.Code) && !
            Equal(import.Search.Code, entities.PaymentStatus.Code))
          {
            export.Group.Next();

            continue;
          }

          local.CsePersonsWorkSet.Number =
            entities.PaymentRequest.CsePersonNumber ?? Spaces(10);
          UseSiReadCsePerson();

          if (IsExitState("CSE_PERSON_NF"))
          {
            ExitState = "ACO_NN0000_ALL_OK";
          }

          MoveCsePersonsWorkSet(local.CsePersonsWorkSet,
            export.Group.Update.CsePersonsWorkSet);
          export.Group.Update.PaymentRequest.Assign(entities.PaymentRequest);
          export.Group.Update.PaymentStatus.Code = entities.PaymentStatus.Code;
          MovePaymentStatusHistory(entities.PaymentStatusHistory,
            export.Group.Update.PaymentStatusHistory);
          export.Group.Update.PayHistReasonText.Text76 =
            Substring(entities.PaymentStatusHistory.ReasonText, 1, 76);
          export.Group.Next();
        }
      }
    }
    else if (IsEmpty(import.SearchObligee.Number))
    {
      export.Group.Index = 0;
      export.Group.Clear();

      foreach(var item in ReadPaymentRequestPaymentStatusPaymentStatusHistory2())
        
      {
        if (!IsEmpty(import.PaymentStatusHistory.CreatedBy) && !
          Equal(import.PaymentStatusHistory.CreatedBy,
          entities.PaymentStatusHistory.CreatedBy))
        {
          export.Group.Next();

          continue;
        }

        if (!IsEmpty(import.Search.Code) && !
          Equal(import.Search.Code, entities.PaymentStatus.Code))
        {
          export.Group.Next();

          continue;
        }

        local.CsePersonsWorkSet.Number =
          entities.PaymentRequest.CsePersonNumber ?? Spaces(10);
        UseSiReadCsePerson();

        if (IsExitState("CSE_PERSON_NF"))
        {
          ExitState = "ACO_NN0000_ALL_OK";
        }

        MoveCsePersonsWorkSet(local.CsePersonsWorkSet,
          export.Group.Update.CsePersonsWorkSet);
        export.Group.Update.PaymentRequest.Assign(entities.PaymentRequest);
        export.Group.Update.PaymentStatus.Code = entities.PaymentStatus.Code;
        MovePaymentStatusHistory(entities.PaymentStatusHistory,
          export.Group.Update.PaymentStatusHistory);
        export.Group.Update.PayHistReasonText.Text76 =
          Substring(entities.PaymentStatusHistory.ReasonText, 1, 76);
        export.Group.Next();
      }
    }
    else
    {
      export.Group.Index = 0;
      export.Group.Clear();

      foreach(var item in ReadPaymentRequestPaymentStatusPaymentStatusHistory1())
        
      {
        if (!IsEmpty(import.PaymentStatusHistory.CreatedBy) && !
          Equal(import.PaymentStatusHistory.CreatedBy,
          entities.PaymentStatusHistory.CreatedBy))
        {
          export.Group.Next();

          continue;
        }

        if (!IsEmpty(import.Search.Code) && !
          Equal(import.Search.Code, entities.PaymentStatus.Code))
        {
          export.Group.Next();

          continue;
        }

        local.CsePersonsWorkSet.Number =
          entities.PaymentRequest.CsePersonNumber ?? Spaces(10);
        UseSiReadCsePerson();

        if (IsExitState("CSE_PERSON_NF"))
        {
          ExitState = "ACO_NN0000_ALL_OK";
        }

        MoveCsePersonsWorkSet(local.CsePersonsWorkSet,
          export.Group.Update.CsePersonsWorkSet);
        export.Group.Update.PaymentRequest.Assign(entities.PaymentRequest);
        export.Group.Update.PaymentStatus.Code = entities.PaymentStatus.Code;
        MovePaymentStatusHistory(entities.PaymentStatusHistory,
          export.Group.Update.PaymentStatusHistory);
        export.Group.Update.PayHistReasonText.Text76 =
          Substring(entities.PaymentStatusHistory.ReasonText, 1, 76);
        export.Group.Next();
      }
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MovePaymentStatusHistory(PaymentStatusHistory source,
    PaymentStatusHistory target)
  {
    target.EffectiveDate = source.EffectiveDate;
    target.CreatedBy = source.CreatedBy;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
  }

  private IEnumerable<bool>
    ReadPaymentRequestPaymentStatusPaymentStatusHistory1()
  {
    return ReadEach("ReadPaymentRequestPaymentStatusPaymentStatusHistory1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "csePersonNumber", import.SearchObligee.Number);
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTimestamp1",
          local.SearchFrom.CreatedTimestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTimestamp2",
          local.SearchTo.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentStatusHistory.PrqGeneratedId = db.GetInt32(reader, 0);
        entities.PaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.PaymentRequest.Amount = db.GetDecimal(reader, 2);
        entities.PaymentRequest.CreatedBy = db.GetString(reader, 3);
        entities.PaymentRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.PaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 5);
        entities.PaymentRequest.Type1 = db.GetString(reader, 6);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 7);
        entities.PaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 8);
        entities.PaymentStatusHistory.PstGeneratedId = db.GetInt32(reader, 8);
        entities.PaymentStatus.Code = db.GetString(reader, 9);
        entities.PaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 10);
        entities.PaymentStatusHistory.EffectiveDate = db.GetDate(reader, 11);
        entities.PaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 12);
        entities.PaymentStatusHistory.CreatedBy = db.GetString(reader, 13);
        entities.PaymentStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.PaymentStatusHistory.ReasonText =
          db.GetNullableString(reader, 15);
        entities.PaymentStatus.Populated = true;
        entities.PaymentStatusHistory.Populated = true;
        entities.PaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);

        return true;
      });
  }

  private IEnumerable<bool>
    ReadPaymentRequestPaymentStatusPaymentStatusHistory2()
  {
    return ReadEach("ReadPaymentRequestPaymentStatusPaymentStatusHistory2",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTimestamp1",
          local.SearchFrom.CreatedTimestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTimestamp2",
          local.SearchTo.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentStatusHistory.PrqGeneratedId = db.GetInt32(reader, 0);
        entities.PaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.PaymentRequest.Amount = db.GetDecimal(reader, 2);
        entities.PaymentRequest.CreatedBy = db.GetString(reader, 3);
        entities.PaymentRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.PaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 5);
        entities.PaymentRequest.Type1 = db.GetString(reader, 6);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 7);
        entities.PaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 8);
        entities.PaymentStatusHistory.PstGeneratedId = db.GetInt32(reader, 8);
        entities.PaymentStatus.Code = db.GetString(reader, 9);
        entities.PaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 10);
        entities.PaymentStatusHistory.EffectiveDate = db.GetDate(reader, 11);
        entities.PaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 12);
        entities.PaymentStatusHistory.CreatedBy = db.GetString(reader, 13);
        entities.PaymentStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.PaymentStatusHistory.ReasonText =
          db.GetNullableString(reader, 15);
        entities.PaymentStatus.Populated = true;
        entities.PaymentStatusHistory.Populated = true;
        entities.PaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);

        return true;
      });
  }

  private IEnumerable<bool>
    ReadPaymentRequestPaymentStatusPaymentStatusHistory3()
  {
    return ReadEach("ReadPaymentRequestPaymentStatusPaymentStatusHistory3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "csePersonNumber", import.SearchObligee.Number);
        db.SetDateTime(
          command, "createdTimestamp1",
          local.SearchFrom.CreatedTimestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTimestamp2",
          local.SearchTo.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentStatusHistory.PrqGeneratedId = db.GetInt32(reader, 0);
        entities.PaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.PaymentRequest.Amount = db.GetDecimal(reader, 2);
        entities.PaymentRequest.CreatedBy = db.GetString(reader, 3);
        entities.PaymentRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.PaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 5);
        entities.PaymentRequest.Type1 = db.GetString(reader, 6);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 7);
        entities.PaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 8);
        entities.PaymentStatusHistory.PstGeneratedId = db.GetInt32(reader, 8);
        entities.PaymentStatus.Code = db.GetString(reader, 9);
        entities.PaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 10);
        entities.PaymentStatusHistory.EffectiveDate = db.GetDate(reader, 11);
        entities.PaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 12);
        entities.PaymentStatusHistory.CreatedBy = db.GetString(reader, 13);
        entities.PaymentStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.PaymentStatusHistory.ReasonText =
          db.GetNullableString(reader, 15);
        entities.PaymentStatus.Populated = true;
        entities.PaymentStatusHistory.Populated = true;
        entities.PaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);

        return true;
      });
  }

  private IEnumerable<bool>
    ReadPaymentRequestPaymentStatusPaymentStatusHistory4()
  {
    return ReadEach("ReadPaymentRequestPaymentStatusPaymentStatusHistory4",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp1",
          local.SearchFrom.CreatedTimestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTimestamp2",
          local.SearchTo.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentStatusHistory.PrqGeneratedId = db.GetInt32(reader, 0);
        entities.PaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.PaymentRequest.Amount = db.GetDecimal(reader, 2);
        entities.PaymentRequest.CreatedBy = db.GetString(reader, 3);
        entities.PaymentRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.PaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 5);
        entities.PaymentRequest.Type1 = db.GetString(reader, 6);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 7);
        entities.PaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 8);
        entities.PaymentStatusHistory.PstGeneratedId = db.GetInt32(reader, 8);
        entities.PaymentStatus.Code = db.GetString(reader, 9);
        entities.PaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 10);
        entities.PaymentStatusHistory.EffectiveDate = db.GetDate(reader, 11);
        entities.PaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 12);
        entities.PaymentStatusHistory.CreatedBy = db.GetString(reader, 13);
        entities.PaymentStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.PaymentStatusHistory.ReasonText =
          db.GetNullableString(reader, 15);
        entities.PaymentStatus.Populated = true;
        entities.PaymentStatusHistory.Populated = true;
        entities.PaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);

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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of SearchTo.
    /// </summary>
    [JsonPropertyName("searchTo")]
    public PaymentRequest SearchTo
    {
      get => searchTo ??= new();
      set => searchTo = value;
    }

    /// <summary>
    /// A value of SearchFrom.
    /// </summary>
    [JsonPropertyName("searchFrom")]
    public PaymentRequest SearchFrom
    {
      get => searchFrom ??= new();
      set => searchFrom = value;
    }

    /// <summary>
    /// A value of PaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("paymentStatusHistory")]
    public PaymentStatusHistory PaymentStatusHistory
    {
      get => paymentStatusHistory ??= new();
      set => paymentStatusHistory = value;
    }

    /// <summary>
    /// A value of History.
    /// </summary>
    [JsonPropertyName("history")]
    public Common History
    {
      get => history ??= new();
      set => history = value;
    }

    /// <summary>
    /// A value of SearchObligee.
    /// </summary>
    [JsonPropertyName("searchObligee")]
    public CsePerson SearchObligee
    {
      get => searchObligee ??= new();
      set => searchObligee = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public PaymentStatus Search
    {
      get => search ??= new();
      set => search = value;
    }

    private DateWorkArea current;
    private PaymentRequest searchTo;
    private PaymentRequest searchFrom;
    private PaymentStatusHistory paymentStatusHistory;
    private Common history;
    private CsePerson searchObligee;
    private PaymentStatus search;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of PaymentStatusHistory.
      /// </summary>
      [JsonPropertyName("paymentStatusHistory")]
      public PaymentStatusHistory PaymentStatusHistory
      {
        get => paymentStatusHistory ??= new();
        set => paymentStatusHistory = value;
      }

      /// <summary>
      /// A value of Zdel.
      /// </summary>
      [JsonPropertyName("zdel")]
      public Common Zdel
      {
        get => zdel ??= new();
        set => zdel = value;
      }

      /// <summary>
      /// A value of PaymentRequest.
      /// </summary>
      [JsonPropertyName("paymentRequest")]
      public PaymentRequest PaymentRequest
      {
        get => paymentRequest ??= new();
        set => paymentRequest = value;
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
      /// A value of PaymentStatus.
      /// </summary>
      [JsonPropertyName("paymentStatus")]
      public PaymentStatus PaymentStatus
      {
        get => paymentStatus ??= new();
        set => paymentStatus = value;
      }

      /// <summary>
      /// A value of PayHistReasonText.
      /// </summary>
      [JsonPropertyName("payHistReasonText")]
      public NewWorkSet PayHistReasonText
      {
        get => payHistReasonText ??= new();
        set => payHistReasonText = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 84;

      private PaymentStatusHistory paymentStatusHistory;
      private Common zdel;
      private PaymentRequest paymentRequest;
      private CsePersonsWorkSet csePersonsWorkSet;
      private PaymentStatus paymentStatus;
      private NewWorkSet payHistReasonText;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    private Array<GroupGroup> group;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of WorkTimestamp.
    /// </summary>
    [JsonPropertyName("workTimestamp")]
    public Standard WorkTimestamp
    {
      get => workTimestamp ??= new();
      set => workTimestamp = value;
    }

    /// <summary>
    /// A value of WorkYear.
    /// </summary>
    [JsonPropertyName("workYear")]
    public NextTranInfo WorkYear
    {
      get => workYear ??= new();
      set => workYear = value;
    }

    /// <summary>
    /// A value of WorkDay.
    /// </summary>
    [JsonPropertyName("workDay")]
    public NextTranInfo WorkDay
    {
      get => workDay ??= new();
      set => workDay = value;
    }

    /// <summary>
    /// A value of WorkMonth.
    /// </summary>
    [JsonPropertyName("workMonth")]
    public NextTranInfo WorkMonth
    {
      get => workMonth ??= new();
      set => workMonth = value;
    }

    /// <summary>
    /// A value of SearchFrom.
    /// </summary>
    [JsonPropertyName("searchFrom")]
    public PaymentRequest SearchFrom
    {
      get => searchFrom ??= new();
      set => searchFrom = value;
    }

    /// <summary>
    /// A value of SearchTo.
    /// </summary>
    [JsonPropertyName("searchTo")]
    public PaymentRequest SearchTo
    {
      get => searchTo ??= new();
      set => searchTo = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    private Standard workTimestamp;
    private NextTranInfo workYear;
    private NextTranInfo workDay;
    private NextTranInfo workMonth;
    private PaymentRequest searchFrom;
    private PaymentRequest searchTo;
    private CsePersonsWorkSet csePersonsWorkSet;
    private DateWorkArea null1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of PaymentStatus.
    /// </summary>
    [JsonPropertyName("paymentStatus")]
    public PaymentStatus PaymentStatus
    {
      get => paymentStatus ??= new();
      set => paymentStatus = value;
    }

    /// <summary>
    /// A value of PaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("paymentStatusHistory")]
    public PaymentStatusHistory PaymentStatusHistory
    {
      get => paymentStatusHistory ??= new();
      set => paymentStatusHistory = value;
    }

    /// <summary>
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    private PaymentStatus paymentStatus;
    private PaymentStatusHistory paymentStatusHistory;
    private PaymentRequest paymentRequest;
  }
#endregion
}
