// The source file: CASH_RECEIPT_LITERALS, ID: 371774257, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// RESP: FNCLMGMNT
/// This work attribute set contains a set of
///  literals used to the cash receipting screens.
/// </summary>
[Serializable]
public partial class CashReceiptLiterals: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CashReceiptLiterals()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CashReceiptLiterals(CashReceiptLiterals that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CashReceiptLiterals Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(CashReceiptLiterals that)
  {
    base.Assign(that);
    debitCredit = that.debitCredit;
    adjustmentsExist = that.adjustmentsExist;
    notesExist = that.notesExist;
    feesExist = that.feesExist;
    moreOnPage2 = that.moreOnPage2;
  }

  /// <summary>Length of the DEBIT_CREDIT attribute.</summary>
  public const int DebitCredit_MaxLength = 2;

  /// <summary>
  /// The value of the DEBIT_CREDIT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = DebitCredit_MaxLength)]
  public string DebitCredit
  {
    get => debitCredit ?? "";
    set => debitCredit = TrimEnd(Substring(value, 1, DebitCredit_MaxLength));
  }

  /// <summary>
  /// The json value of the DebitCredit attribute.</summary>
  [JsonPropertyName("debitCredit")]
  [Computed]
  public string DebitCredit_Json
  {
    get => NullIf(DebitCredit, "");
    set => DebitCredit = value;
  }

  /// <summary>Length of the ADJUSTMENTS_EXIST attribute.</summary>
  public const int AdjustmentsExist_MaxLength = 17;

  /// <summary>
  /// The value of the ADJUSTMENTS_EXIST attribute.
  /// INDICATES THAT ADJUSTMENTS EXIST AND MAY BE VIEWED ON THE LIST ADJUSTMENTS
  /// SCREEN.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = AdjustmentsExist_MaxLength)
    ]
  public string AdjustmentsExist
  {
    get => adjustmentsExist ?? "";
    set => adjustmentsExist =
      TrimEnd(Substring(value, 1, AdjustmentsExist_MaxLength));
  }

  /// <summary>
  /// The json value of the AdjustmentsExist attribute.</summary>
  [JsonPropertyName("adjustmentsExist")]
  [Computed]
  public string AdjustmentsExist_Json
  {
    get => NullIf(AdjustmentsExist, "");
    set => AdjustmentsExist = value;
  }

  /// <summary>Length of the NOTES_EXIST attribute.</summary>
  public const int NotesExist_MaxLength = 12;

  /// <summary>
  /// The value of the NOTES_EXIST attribute.
  /// INDICATES THAT NOTES EXISTS AND  CAN BE VIEWED BY FLOWING TO THE NOTES 
  /// MAINTENANCE SCREEN.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = NotesExist_MaxLength)]
  public string NotesExist
  {
    get => notesExist ?? "";
    set => notesExist = TrimEnd(Substring(value, 1, NotesExist_MaxLength));
  }

  /// <summary>
  /// The json value of the NotesExist attribute.</summary>
  [JsonPropertyName("notesExist")]
  [Computed]
  public string NotesExist_Json
  {
    get => NullIf(NotesExist, "");
    set => NotesExist = value;
  }

  /// <summary>Length of the FEES_EXIST attribute.</summary>
  public const int FeesExist_MaxLength = 12;

  /// <summary>
  /// The value of the FEES_EXIST attribute.
  /// iNDICATES THAT FEES EXISTS AND MAY BE VIEWED BY FLOWING TO THE FEES 
  /// MAINTENANCE SCREEN.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = FeesExist_MaxLength)]
  public string FeesExist
  {
    get => feesExist ?? "";
    set => feesExist = TrimEnd(Substring(value, 1, FeesExist_MaxLength));
  }

  /// <summary>
  /// The json value of the FeesExist attribute.</summary>
  [JsonPropertyName("feesExist")]
  [Computed]
  public string FeesExist_Json
  {
    get => NullIf(FeesExist, "");
    set => FeesExist = value;
  }

  /// <summary>Length of the MORE_ON_PAGE_2 attribute.</summary>
  public const int MoreOnPage2_MaxLength = 12;

  /// <summary>
  /// The value of the MORE_ON_PAGE_2 attribute.
  /// INDICATES THAT DATA EXISTS ON PAGE 2 OF A TWO-PAGE SCREEN.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = MoreOnPage2_MaxLength)]
  public string MoreOnPage2
  {
    get => moreOnPage2 ?? "";
    set => moreOnPage2 = TrimEnd(Substring(value, 1, MoreOnPage2_MaxLength));
  }

  /// <summary>
  /// The json value of the MoreOnPage2 attribute.</summary>
  [JsonPropertyName("moreOnPage2")]
  [Computed]
  public string MoreOnPage2_Json
  {
    get => NullIf(MoreOnPage2, "");
    set => MoreOnPage2 = value;
  }

  private string debitCredit;
  private string adjustmentsExist;
  private string notesExist;
  private string feesExist;
  private string moreOnPage2;
}
