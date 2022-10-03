// The source file: SCREEN_OWED_AMOUNTS_DTL, ID: 372446484, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class ScreenOwedAmountsDtl: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ScreenOwedAmountsDtl()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ScreenOwedAmountsDtl(ScreenOwedAmountsDtl that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ScreenOwedAmountsDtl Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ScreenOwedAmountsDtl that)
  {
    base.Assign(that);
    csCurrDue = that.csCurrDue;
    csCurrOwed = that.csCurrOwed;
    csCurrArrears = that.csCurrArrears;
    csCurrColl = that.csCurrColl;
    spCurrDue = that.spCurrDue;
    spCurrOwed = that.spCurrOwed;
    spCurrArrears = that.spCurrArrears;
    spCurrColl = that.spCurrColl;
    msCurrDue = that.msCurrDue;
    msCurrOwed = that.msCurrOwed;
    msCurrColl = that.msCurrColl;
    msCurrArrears = that.msCurrArrears;
    mcCurrDue = that.mcCurrDue;
    mcCurrOwed = that.mcCurrOwed;
    mcCurrArrears = that.mcCurrArrears;
    mcCurrColl = that.mcCurrColl;
    totalCurrDue = that.totalCurrDue;
    totalCurrOwed = that.totalCurrOwed;
    totalCurrColl = that.totalCurrColl;
    periodicPymntDue = that.periodicPymntDue;
    periodicPymntOwed = that.periodicPymntOwed;
    periodicPymntColl = that.periodicPymntColl;
    afiArrearsOwed = that.afiArrearsOwed;
    afiArrearsColl = that.afiArrearsColl;
    afiInterestOwed = that.afiInterestOwed;
    afiInterestColl = that.afiInterestColl;
    fciArrearsOwed = that.fciArrearsOwed;
    fciArrearsColl = that.fciArrearsColl;
    fciInterestOwed = that.fciInterestOwed;
    fciInterestColl = that.fciInterestColl;
    naiArrearsOwed = that.naiArrearsOwed;
    naiArrearsColl = that.naiArrearsColl;
    naiInterestOwed = that.naiInterestOwed;
    naiInterestColl = that.naiInterestColl;
    nfArrearsOwed = that.nfArrearsOwed;
    nfArrearsColl = that.nfArrearsColl;
    nfInterestOwed = that.nfInterestOwed;
    nfInterestColl = that.nfInterestColl;
    ncArrearsOwed = that.ncArrearsOwed;
    ncArrearsColl = that.ncArrearsColl;
    ncInterestOwed = that.ncInterestOwed;
    ncInterestColl = that.ncInterestColl;
    feesArrearsOwed = that.feesArrearsOwed;
    feesArrearsColl = that.feesArrearsColl;
    feesInterestOwed = that.feesInterestOwed;
    feesInterestColl = that.feesInterestColl;
    recoveryArrearsOwed = that.recoveryArrearsOwed;
    recoveryArrearsColl = that.recoveryArrearsColl;
    futureColl = that.futureColl;
    giftColl = that.giftColl;
    totalArrearsOwed = that.totalArrearsOwed;
    totalArrearsColl = that.totalArrearsColl;
    totalInterestOwed = that.totalInterestOwed;
    totalInterestColl = that.totalInterestColl;
    totalCurrArrIntOwed = that.totalCurrArrIntOwed;
    totalCurrArrIntColl = that.totalCurrArrIntColl;
    totalVoluntaryColl = that.totalVoluntaryColl;
    undistributedAmt = that.undistributedAmt;
    incomingInterstateObExists = that.incomingInterstateObExists;
    lastCollDt = that.lastCollDt;
    lastCollAmt = that.lastCollAmt;
    errorInformationLine = that.errorInformationLine;
    naNaArrearsOwed = that.naNaArrearsOwed;
    naUpArrearsOwed = that.naUpArrearsOwed;
    naUdArrearsOwed = that.naUdArrearsOwed;
    naCaArrearsOwed = that.naCaArrearsOwed;
    afPaArrearsOwed = that.afPaArrearsOwed;
    afTaArrearsOwed = that.afTaArrearsOwed;
    afCaArrearsOwed = that.afCaArrearsOwed;
    fcPaArrearsOwed = that.fcPaArrearsOwed;
    fcTaArrearsOwed = that.fcTaArrearsOwed;
    fcCaArrearsOwed = that.fcCaArrearsOwed;
    naNaInterestOwed = that.naNaInterestOwed;
    naUpInterestOwed = that.naUpInterestOwed;
    naUdInterestOwed = that.naUdInterestOwed;
    naCaInterestOwed = that.naCaInterestOwed;
    afPaInterestOwed = that.afPaInterestOwed;
    afTaInterestOwed = that.afTaInterestOwed;
    afCaInterestOwed = that.afCaInterestOwed;
    fcPaInterestOwed = that.fcPaInterestOwed;
    fcTaInterestOwed = that.fcTaInterestOwed;
    fcCaInterestOwed = that.fcCaInterestOwed;
    naNaArrearCollected = that.naNaArrearCollected;
    naUpArrearCollected = that.naUpArrearCollected;
    naUdArrearCollected = that.naUdArrearCollected;
    naCaArrearCollected = that.naCaArrearCollected;
    afPaArrearCollected = that.afPaArrearCollected;
    afTaArrearCollected = that.afTaArrearCollected;
    afCaArrearCollected = that.afCaArrearCollected;
    fcPaArrearCollected = that.fcPaArrearCollected;
    fcTaArrearCollected = that.fcTaArrearCollected;
    fcCaArrearCollected = that.fcCaArrearCollected;
    naNaInterestCollected = that.naNaInterestCollected;
    naUpInterestCollected = that.naUpInterestCollected;
    naUdInterestCollected = that.naUdInterestCollected;
    naCaInterestCollected = that.naCaInterestCollected;
    afPaInterestCollected = that.afPaInterestCollected;
    afTaInterestCollected = that.afTaInterestCollected;
    afCaInterestCollected = that.afCaInterestCollected;
    fcPaInterestCollected = that.fcPaInterestCollected;
    fcTaInterestCollected = that.fcTaInterestCollected;
    fcCaInterestCollected = that.fcCaInterestCollected;
  }

  /// <summary>
  /// The value of the CS_CURR_DUE attribute.
  /// Represents the Current Child Support (CS) Due for the current month.  This
  /// is the amount obligated to pay prior to taking any adjustments or
  /// payments into consideration.
  /// </summary>
  [JsonPropertyName("csCurrDue")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 1, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal CsCurrDue
  {
    get => csCurrDue;
    set => csCurrDue = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the CS_CURR_OWED attribute.
  /// This represents the Current Child Support (CS) owed for the current month.
  /// The owed amount is the amount obligated plus/minus any adjustments and/
  /// or payments.
  /// </summary>
  [JsonPropertyName("csCurrOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 2, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal CsCurrOwed
  {
    get => csCurrOwed;
    set => csCurrOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the CS_CURR_ARREARS attribute.
  /// </summary>
  [JsonPropertyName("csCurrArrears")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 3, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal CsCurrArrears
  {
    get => csCurrArrears;
    set => csCurrArrears = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the CS_CURR_COLL attribute.
  /// </summary>
  [JsonPropertyName("csCurrColl")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 4, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal CsCurrColl
  {
    get => csCurrColl;
    set => csCurrColl = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the SP_CURR_DUE attribute.
  /// </summary>
  [JsonPropertyName("spCurrDue")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 5, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal SpCurrDue
  {
    get => spCurrDue;
    set => spCurrDue = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the SP_CURR_OWED attribute.
  /// </summary>
  [JsonPropertyName("spCurrOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 6, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal SpCurrOwed
  {
    get => spCurrOwed;
    set => spCurrOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the SP_CURR_ARREARS attribute.
  /// </summary>
  [JsonPropertyName("spCurrArrears")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 7, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal SpCurrArrears
  {
    get => spCurrArrears;
    set => spCurrArrears = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the SP_CURR_COLL attribute.
  /// </summary>
  [JsonPropertyName("spCurrColl")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 8, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal SpCurrColl
  {
    get => spCurrColl;
    set => spCurrColl = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the MS_CURR_DUE attribute.
  /// </summary>
  [JsonPropertyName("msCurrDue")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 9, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal MsCurrDue
  {
    get => msCurrDue;
    set => msCurrDue = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the MS_CURR_OWED attribute.
  /// </summary>
  [JsonPropertyName("msCurrOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 10, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal MsCurrOwed
  {
    get => msCurrOwed;
    set => msCurrOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the MS_CURR_COLL attribute.
  /// </summary>
  [JsonPropertyName("msCurrColl")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 11, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal MsCurrColl
  {
    get => msCurrColl;
    set => msCurrColl = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the MS_CURR_ARREARS attribute.
  /// </summary>
  [JsonPropertyName("msCurrArrears")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 12, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal MsCurrArrears
  {
    get => msCurrArrears;
    set => msCurrArrears = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the MC_CURR_DUE attribute.
  /// </summary>
  [JsonPropertyName("mcCurrDue")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 13, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal McCurrDue
  {
    get => mcCurrDue;
    set => mcCurrDue = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the MC_CURR_OWED attribute.
  /// </summary>
  [JsonPropertyName("mcCurrOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 14, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal McCurrOwed
  {
    get => mcCurrOwed;
    set => mcCurrOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the MC_CURR_ARREARS attribute.
  /// </summary>
  [JsonPropertyName("mcCurrArrears")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 15, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal McCurrArrears
  {
    get => mcCurrArrears;
    set => mcCurrArrears = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the MC_CURR_COLL attribute.
  /// </summary>
  [JsonPropertyName("mcCurrColl")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 16, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal McCurrColl
  {
    get => mcCurrColl;
    set => mcCurrColl = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the TOTAL_CURR_DUE attribute.
  /// </summary>
  [JsonPropertyName("totalCurrDue")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 17, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal TotalCurrDue
  {
    get => totalCurrDue;
    set => totalCurrDue = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the TOTAL_CURR_OWED attribute.
  /// </summary>
  [JsonPropertyName("totalCurrOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 18, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal TotalCurrOwed
  {
    get => totalCurrOwed;
    set => totalCurrOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the TOTAL_CURR_COLL attribute.
  /// </summary>
  [JsonPropertyName("totalCurrColl")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 19, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal TotalCurrColl
  {
    get => totalCurrColl;
    set => totalCurrColl = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the PERIODIC_PYMNT_DUE attribute.
  /// </summary>
  [JsonPropertyName("periodicPymntDue")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 20, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal PeriodicPymntDue
  {
    get => periodicPymntDue;
    set => periodicPymntDue = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the PERIODIC_PYMNT_OWED attribute.
  /// </summary>
  [JsonPropertyName("periodicPymntOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 21, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal PeriodicPymntOwed
  {
    get => periodicPymntOwed;
    set => periodicPymntOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the PERIODIC_PYMNT_COLL attribute.
  /// </summary>
  [JsonPropertyName("periodicPymntColl")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 22, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal PeriodicPymntColl
  {
    get => periodicPymntColl;
    set => periodicPymntColl = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the AFI_ARREARS_OWED attribute.
  /// </summary>
  [JsonPropertyName("afiArrearsOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 23, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal AfiArrearsOwed
  {
    get => afiArrearsOwed;
    set => afiArrearsOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the AFI_ARREARS_COLL attribute.
  /// </summary>
  [JsonPropertyName("afiArrearsColl")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 24, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal AfiArrearsColl
  {
    get => afiArrearsColl;
    set => afiArrearsColl = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the AFI_INTEREST_OWED attribute.
  /// </summary>
  [JsonPropertyName("afiInterestOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 25, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal AfiInterestOwed
  {
    get => afiInterestOwed;
    set => afiInterestOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the AFI_INTEREST_COLL attribute.
  /// </summary>
  [JsonPropertyName("afiInterestColl")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 26, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal AfiInterestColl
  {
    get => afiInterestColl;
    set => afiInterestColl = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the FCI_ARREARS_OWED attribute.
  /// </summary>
  [JsonPropertyName("fciArrearsOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 27, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal FciArrearsOwed
  {
    get => fciArrearsOwed;
    set => fciArrearsOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the FCI_ARREARS_COLL attribute.
  /// </summary>
  [JsonPropertyName("fciArrearsColl")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 28, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal FciArrearsColl
  {
    get => fciArrearsColl;
    set => fciArrearsColl = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the FCI_INTEREST_OWED attribute.
  /// </summary>
  [JsonPropertyName("fciInterestOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 29, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal FciInterestOwed
  {
    get => fciInterestOwed;
    set => fciInterestOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the FCI_INTEREST_COLL attribute.
  /// </summary>
  [JsonPropertyName("fciInterestColl")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 30, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal FciInterestColl
  {
    get => fciInterestColl;
    set => fciInterestColl = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the NAI_ARREARS_OWED attribute.
  /// </summary>
  [JsonPropertyName("naiArrearsOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 31, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal NaiArrearsOwed
  {
    get => naiArrearsOwed;
    set => naiArrearsOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the NAI_ARREARS_COLL attribute.
  /// </summary>
  [JsonPropertyName("naiArrearsColl")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 32, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal NaiArrearsColl
  {
    get => naiArrearsColl;
    set => naiArrearsColl = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the NAI_INTEREST_OWED attribute.
  /// </summary>
  [JsonPropertyName("naiInterestOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 33, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal NaiInterestOwed
  {
    get => naiInterestOwed;
    set => naiInterestOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the NAI_INTEREST_COLL attribute.
  /// </summary>
  [JsonPropertyName("naiInterestColl")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 34, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal NaiInterestColl
  {
    get => naiInterestColl;
    set => naiInterestColl = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the NF_ARREARS_OWED attribute.
  /// </summary>
  [JsonPropertyName("nfArrearsOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 35, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal NfArrearsOwed
  {
    get => nfArrearsOwed;
    set => nfArrearsOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the NF_ARREARS_COLL attribute.
  /// </summary>
  [JsonPropertyName("nfArrearsColl")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 36, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal NfArrearsColl
  {
    get => nfArrearsColl;
    set => nfArrearsColl = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the NF_INTEREST_OWED attribute.
  /// </summary>
  [JsonPropertyName("nfInterestOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 37, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal NfInterestOwed
  {
    get => nfInterestOwed;
    set => nfInterestOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the NF_INTEREST_COLL attribute.
  /// </summary>
  [JsonPropertyName("nfInterestColl")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 38, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal NfInterestColl
  {
    get => nfInterestColl;
    set => nfInterestColl = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the NC_ARREARS_OWED attribute.
  /// </summary>
  [JsonPropertyName("ncArrearsOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 39, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal NcArrearsOwed
  {
    get => ncArrearsOwed;
    set => ncArrearsOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the NC_ARREARS_COLL attribute.
  /// </summary>
  [JsonPropertyName("ncArrearsColl")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 40, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal NcArrearsColl
  {
    get => ncArrearsColl;
    set => ncArrearsColl = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the NC_INTEREST_OWED attribute.
  /// </summary>
  [JsonPropertyName("ncInterestOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 41, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal NcInterestOwed
  {
    get => ncInterestOwed;
    set => ncInterestOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the NC_INTEREST_COLL attribute.
  /// </summary>
  [JsonPropertyName("ncInterestColl")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 42, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal NcInterestColl
  {
    get => ncInterestColl;
    set => ncInterestColl = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the FEES_ARREARS_OWED attribute.
  /// </summary>
  [JsonPropertyName("feesArrearsOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 43, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal FeesArrearsOwed
  {
    get => feesArrearsOwed;
    set => feesArrearsOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the FEES_ARREARS_COLL attribute.
  /// </summary>
  [JsonPropertyName("feesArrearsColl")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 44, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal FeesArrearsColl
  {
    get => feesArrearsColl;
    set => feesArrearsColl = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the FEES_INTEREST_OWED attribute.
  /// </summary>
  [JsonPropertyName("feesInterestOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 45, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal FeesInterestOwed
  {
    get => feesInterestOwed;
    set => feesInterestOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the FEES_INTEREST_COLL attribute.
  /// </summary>
  [JsonPropertyName("feesInterestColl")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 46, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal FeesInterestColl
  {
    get => feesInterestColl;
    set => feesInterestColl = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the RECOVERY_ARREARS_OWED attribute.
  /// </summary>
  [JsonPropertyName("recoveryArrearsOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 47, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal RecoveryArrearsOwed
  {
    get => recoveryArrearsOwed;
    set => recoveryArrearsOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the RECOVERY_ARREARS_COLL attribute.
  /// </summary>
  [JsonPropertyName("recoveryArrearsColl")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 48, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal RecoveryArrearsColl
  {
    get => recoveryArrearsColl;
    set => recoveryArrearsColl = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the FUTURE_COLL attribute.
  /// </summary>
  [JsonPropertyName("futureColl")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 49, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal FutureColl
  {
    get => futureColl;
    set => futureColl = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the GIFT_COLL attribute.
  /// </summary>
  [JsonPropertyName("giftColl")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 50, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal GiftColl
  {
    get => giftColl;
    set => giftColl = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the TOTAL_ARREARS_OWED attribute.
  /// </summary>
  [JsonPropertyName("totalArrearsOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 51, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal TotalArrearsOwed
  {
    get => totalArrearsOwed;
    set => totalArrearsOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the TOTAL_ARREARS_COLL attribute.
  /// </summary>
  [JsonPropertyName("totalArrearsColl")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 52, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal TotalArrearsColl
  {
    get => totalArrearsColl;
    set => totalArrearsColl = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the TOTAL_INTEREST_OWED attribute.
  /// </summary>
  [JsonPropertyName("totalInterestOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 53, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal TotalInterestOwed
  {
    get => totalInterestOwed;
    set => totalInterestOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the TOTAL_INTEREST_COLL attribute.
  /// </summary>
  [JsonPropertyName("totalInterestColl")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 54, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal TotalInterestColl
  {
    get => totalInterestColl;
    set => totalInterestColl = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the TOTAL_CURR_ARR_INT_OWED attribute.
  /// </summary>
  [JsonPropertyName("totalCurrArrIntOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 55, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal TotalCurrArrIntOwed
  {
    get => totalCurrArrIntOwed;
    set => totalCurrArrIntOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the TOTAL_CURR_ARR_INT_COLL attribute.
  /// </summary>
  [JsonPropertyName("totalCurrArrIntColl")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 56, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal TotalCurrArrIntColl
  {
    get => totalCurrArrIntColl;
    set => totalCurrArrIntColl = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the TOTAL_VOLUNTARY_COLL attribute.
  /// </summary>
  [JsonPropertyName("totalVoluntaryColl")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 57, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal TotalVoluntaryColl
  {
    get => totalVoluntaryColl;
    set => totalVoluntaryColl = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the UNDISTRIBUTED_AMT attribute.
  /// </summary>
  [JsonPropertyName("undistributedAmt")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 58, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal UndistributedAmt
  {
    get => undistributedAmt;
    set => undistributedAmt = Truncate(value, 2);
  }

  /// <summary>Length of the INCOMING_INTERSTATE_OB_EXISTS attribute.</summary>
  public const int IncomingInterstateObExists_MaxLength = 1;

  /// <summary>
  /// The value of the INCOMING_INTERSTATE_OB_EXISTS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 59, Type = MemberType.Char, Length
    = IncomingInterstateObExists_MaxLength)]
  public string IncomingInterstateObExists
  {
    get => incomingInterstateObExists ?? "";
    set => incomingInterstateObExists =
      TrimEnd(Substring(value, 1, IncomingInterstateObExists_MaxLength));
  }

  /// <summary>
  /// The json value of the IncomingInterstateObExists attribute.</summary>
  [JsonPropertyName("incomingInterstateObExists")]
  [Computed]
  public string IncomingInterstateObExists_Json
  {
    get => NullIf(IncomingInterstateObExists, "");
    set => IncomingInterstateObExists = value;
  }

  /// <summary>
  /// The value of the LAST_COLL_DT attribute.
  /// </summary>
  [JsonPropertyName("lastCollDt")]
  [Member(Index = 60, Type = MemberType.Date)]
  public DateTime? LastCollDt
  {
    get => lastCollDt;
    set => lastCollDt = value;
  }

  /// <summary>
  /// The value of the LAST_COLL_AMT attribute.
  /// </summary>
  [JsonPropertyName("lastCollAmt")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 61, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal LastCollAmt
  {
    get => lastCollAmt;
    set => lastCollAmt = Truncate(value, 2);
  }

  /// <summary>Length of the ERROR_INFORMATION_LINE attribute.</summary>
  public const int ErrorInformationLine_MaxLength = 30;

  /// <summary>
  /// The value of the ERROR_INFORMATION_LINE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 62, Type = MemberType.Char, Length
    = ErrorInformationLine_MaxLength)]
  public string ErrorInformationLine
  {
    get => errorInformationLine ?? "";
    set => errorInformationLine =
      TrimEnd(Substring(value, 1, ErrorInformationLine_MaxLength));
  }

  /// <summary>
  /// The json value of the ErrorInformationLine attribute.</summary>
  [JsonPropertyName("errorInformationLine")]
  [Computed]
  public string ErrorInformationLine_Json
  {
    get => NullIf(ErrorInformationLine, "");
    set => ErrorInformationLine = value;
  }

  /// <summary>
  /// The value of the NA_NA_ARREARS_OWED attribute.
  /// </summary>
  [JsonPropertyName("naNaArrearsOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 63, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal NaNaArrearsOwed
  {
    get => naNaArrearsOwed;
    set => naNaArrearsOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the NA_UP_ARREARS_OWED attribute.
  /// </summary>
  [JsonPropertyName("naUpArrearsOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 64, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal NaUpArrearsOwed
  {
    get => naUpArrearsOwed;
    set => naUpArrearsOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the NA_UD_ARREARS_OWED attribute.
  /// </summary>
  [JsonPropertyName("naUdArrearsOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 65, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal NaUdArrearsOwed
  {
    get => naUdArrearsOwed;
    set => naUdArrearsOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the NA_CA_ARREARS_OWED attribute.
  /// </summary>
  [JsonPropertyName("naCaArrearsOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 66, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal NaCaArrearsOwed
  {
    get => naCaArrearsOwed;
    set => naCaArrearsOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the AF_PA_ARREARS_OWED attribute.
  /// </summary>
  [JsonPropertyName("afPaArrearsOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 67, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal AfPaArrearsOwed
  {
    get => afPaArrearsOwed;
    set => afPaArrearsOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the AF_TA_ARREARS_OWED attribute.
  /// </summary>
  [JsonPropertyName("afTaArrearsOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 68, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal AfTaArrearsOwed
  {
    get => afTaArrearsOwed;
    set => afTaArrearsOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the AF_CA_ARREARS_OWED attribute.
  /// </summary>
  [JsonPropertyName("afCaArrearsOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 69, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal AfCaArrearsOwed
  {
    get => afCaArrearsOwed;
    set => afCaArrearsOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the FC_PA_ARREARS_OWED attribute.
  /// </summary>
  [JsonPropertyName("fcPaArrearsOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 70, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal FcPaArrearsOwed
  {
    get => fcPaArrearsOwed;
    set => fcPaArrearsOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the FC_TA_ARREARS_OWED attribute.
  /// </summary>
  [JsonPropertyName("fcTaArrearsOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 71, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal FcTaArrearsOwed
  {
    get => fcTaArrearsOwed;
    set => fcTaArrearsOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the FC_CA_ARREARS_OWED attribute.
  /// </summary>
  [JsonPropertyName("fcCaArrearsOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 72, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal FcCaArrearsOwed
  {
    get => fcCaArrearsOwed;
    set => fcCaArrearsOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the NA_NA_INTEREST_OWED attribute.
  /// </summary>
  [JsonPropertyName("naNaInterestOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 73, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal NaNaInterestOwed
  {
    get => naNaInterestOwed;
    set => naNaInterestOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the NA_UP_INTEREST_OWED attribute.
  /// </summary>
  [JsonPropertyName("naUpInterestOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 74, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal NaUpInterestOwed
  {
    get => naUpInterestOwed;
    set => naUpInterestOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the NA_UD_INTEREST_OWED attribute.
  /// </summary>
  [JsonPropertyName("naUdInterestOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 75, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal NaUdInterestOwed
  {
    get => naUdInterestOwed;
    set => naUdInterestOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the NA_CA_INTEREST_OWED attribute.
  /// </summary>
  [JsonPropertyName("naCaInterestOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 76, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal NaCaInterestOwed
  {
    get => naCaInterestOwed;
    set => naCaInterestOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the AF_PA_INTEREST_OWED attribute.
  /// </summary>
  [JsonPropertyName("afPaInterestOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 77, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal AfPaInterestOwed
  {
    get => afPaInterestOwed;
    set => afPaInterestOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the AF_TA_INTEREST_OWED attribute.
  /// </summary>
  [JsonPropertyName("afTaInterestOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 78, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal AfTaInterestOwed
  {
    get => afTaInterestOwed;
    set => afTaInterestOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the AF_CA_INTEREST_OWED attribute.
  /// </summary>
  [JsonPropertyName("afCaInterestOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 79, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal AfCaInterestOwed
  {
    get => afCaInterestOwed;
    set => afCaInterestOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the FC_PA_INTEREST_OWED attribute.
  /// </summary>
  [JsonPropertyName("fcPaInterestOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 80, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal FcPaInterestOwed
  {
    get => fcPaInterestOwed;
    set => fcPaInterestOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the FC_TA_INTEREST_OWED attribute.
  /// </summary>
  [JsonPropertyName("fcTaInterestOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 81, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal FcTaInterestOwed
  {
    get => fcTaInterestOwed;
    set => fcTaInterestOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the FC_CA_INTEREST_OWED attribute.
  /// </summary>
  [JsonPropertyName("fcCaInterestOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 82, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal FcCaInterestOwed
  {
    get => fcCaInterestOwed;
    set => fcCaInterestOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the NA_NA_ARREAR_COLLECTED attribute.
  /// </summary>
  [JsonPropertyName("naNaArrearCollected")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 83, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal NaNaArrearCollected
  {
    get => naNaArrearCollected;
    set => naNaArrearCollected = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the NA_UP_ARREAR_COLLECTED attribute.
  /// </summary>
  [JsonPropertyName("naUpArrearCollected")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 84, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal NaUpArrearCollected
  {
    get => naUpArrearCollected;
    set => naUpArrearCollected = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the NA_UD_ARREAR_COLLECTED attribute.
  /// </summary>
  [JsonPropertyName("naUdArrearCollected")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 85, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal NaUdArrearCollected
  {
    get => naUdArrearCollected;
    set => naUdArrearCollected = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the NA_CA_ARREAR_COLLECTED attribute.
  /// </summary>
  [JsonPropertyName("naCaArrearCollected")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 86, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal NaCaArrearCollected
  {
    get => naCaArrearCollected;
    set => naCaArrearCollected = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the AF_PA_ARREAR_COLLECTED attribute.
  /// </summary>
  [JsonPropertyName("afPaArrearCollected")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 87, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal AfPaArrearCollected
  {
    get => afPaArrearCollected;
    set => afPaArrearCollected = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the AF_TA_ARREAR_COLLECTED attribute.
  /// </summary>
  [JsonPropertyName("afTaArrearCollected")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 88, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal AfTaArrearCollected
  {
    get => afTaArrearCollected;
    set => afTaArrearCollected = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the AF_CA_ARREAR_COLLECTED attribute.
  /// </summary>
  [JsonPropertyName("afCaArrearCollected")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 89, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal AfCaArrearCollected
  {
    get => afCaArrearCollected;
    set => afCaArrearCollected = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the FC_PA_ARREAR_COLLECTED attribute.
  /// </summary>
  [JsonPropertyName("fcPaArrearCollected")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 90, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal FcPaArrearCollected
  {
    get => fcPaArrearCollected;
    set => fcPaArrearCollected = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the FC_TA_ARREAR_COLLECTED attribute.
  /// </summary>
  [JsonPropertyName("fcTaArrearCollected")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 91, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal FcTaArrearCollected
  {
    get => fcTaArrearCollected;
    set => fcTaArrearCollected = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the FC_CA_ARREAR_COLLECTED attribute.
  /// </summary>
  [JsonPropertyName("fcCaArrearCollected")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 92, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal FcCaArrearCollected
  {
    get => fcCaArrearCollected;
    set => fcCaArrearCollected = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the NA_NA_INTEREST_COLLECTED attribute.
  /// </summary>
  [JsonPropertyName("naNaInterestCollected")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 93, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal NaNaInterestCollected
  {
    get => naNaInterestCollected;
    set => naNaInterestCollected = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the NA_UP_INTEREST_COLLECTED attribute.
  /// </summary>
  [JsonPropertyName("naUpInterestCollected")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 94, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal NaUpInterestCollected
  {
    get => naUpInterestCollected;
    set => naUpInterestCollected = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the NA_UD_INTEREST_COLLECTED attribute.
  /// </summary>
  [JsonPropertyName("naUdInterestCollected")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 95, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal NaUdInterestCollected
  {
    get => naUdInterestCollected;
    set => naUdInterestCollected = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the NA_CA_INTEREST_COLLECTED attribute.
  /// </summary>
  [JsonPropertyName("naCaInterestCollected")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 96, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal NaCaInterestCollected
  {
    get => naCaInterestCollected;
    set => naCaInterestCollected = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the AF_PA_INTEREST_COLLECTED attribute.
  /// </summary>
  [JsonPropertyName("afPaInterestCollected")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 97, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal AfPaInterestCollected
  {
    get => afPaInterestCollected;
    set => afPaInterestCollected = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the AF_TA_INTEREST_COLLECTED attribute.
  /// </summary>
  [JsonPropertyName("afTaInterestCollected")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 98, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal AfTaInterestCollected
  {
    get => afTaInterestCollected;
    set => afTaInterestCollected = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the AF_CA_INTEREST_COLLECTED attribute.
  /// </summary>
  [JsonPropertyName("afCaInterestCollected")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 99, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal AfCaInterestCollected
  {
    get => afCaInterestCollected;
    set => afCaInterestCollected = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the FC_PA_INTEREST_COLLECTED attribute.
  /// </summary>
  [JsonPropertyName("fcPaInterestCollected")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 100, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal FcPaInterestCollected
  {
    get => fcPaInterestCollected;
    set => fcPaInterestCollected = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the FC_TA_INTEREST_COLLECTED attribute.
  /// </summary>
  [JsonPropertyName("fcTaInterestCollected")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 101, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal FcTaInterestCollected
  {
    get => fcTaInterestCollected;
    set => fcTaInterestCollected = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the FC_CA_INTEREST_COLLECTED attribute.
  /// </summary>
  [JsonPropertyName("fcCaInterestCollected")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 102, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal FcCaInterestCollected
  {
    get => fcCaInterestCollected;
    set => fcCaInterestCollected = Truncate(value, 2);
  }

  private decimal csCurrDue;
  private decimal csCurrOwed;
  private decimal csCurrArrears;
  private decimal csCurrColl;
  private decimal spCurrDue;
  private decimal spCurrOwed;
  private decimal spCurrArrears;
  private decimal spCurrColl;
  private decimal msCurrDue;
  private decimal msCurrOwed;
  private decimal msCurrColl;
  private decimal msCurrArrears;
  private decimal mcCurrDue;
  private decimal mcCurrOwed;
  private decimal mcCurrArrears;
  private decimal mcCurrColl;
  private decimal totalCurrDue;
  private decimal totalCurrOwed;
  private decimal totalCurrColl;
  private decimal periodicPymntDue;
  private decimal periodicPymntOwed;
  private decimal periodicPymntColl;
  private decimal afiArrearsOwed;
  private decimal afiArrearsColl;
  private decimal afiInterestOwed;
  private decimal afiInterestColl;
  private decimal fciArrearsOwed;
  private decimal fciArrearsColl;
  private decimal fciInterestOwed;
  private decimal fciInterestColl;
  private decimal naiArrearsOwed;
  private decimal naiArrearsColl;
  private decimal naiInterestOwed;
  private decimal naiInterestColl;
  private decimal nfArrearsOwed;
  private decimal nfArrearsColl;
  private decimal nfInterestOwed;
  private decimal nfInterestColl;
  private decimal ncArrearsOwed;
  private decimal ncArrearsColl;
  private decimal ncInterestOwed;
  private decimal ncInterestColl;
  private decimal feesArrearsOwed;
  private decimal feesArrearsColl;
  private decimal feesInterestOwed;
  private decimal feesInterestColl;
  private decimal recoveryArrearsOwed;
  private decimal recoveryArrearsColl;
  private decimal futureColl;
  private decimal giftColl;
  private decimal totalArrearsOwed;
  private decimal totalArrearsColl;
  private decimal totalInterestOwed;
  private decimal totalInterestColl;
  private decimal totalCurrArrIntOwed;
  private decimal totalCurrArrIntColl;
  private decimal totalVoluntaryColl;
  private decimal undistributedAmt;
  private string incomingInterstateObExists;
  private DateTime? lastCollDt;
  private decimal lastCollAmt;
  private string errorInformationLine;
  private decimal naNaArrearsOwed;
  private decimal naUpArrearsOwed;
  private decimal naUdArrearsOwed;
  private decimal naCaArrearsOwed;
  private decimal afPaArrearsOwed;
  private decimal afTaArrearsOwed;
  private decimal afCaArrearsOwed;
  private decimal fcPaArrearsOwed;
  private decimal fcTaArrearsOwed;
  private decimal fcCaArrearsOwed;
  private decimal naNaInterestOwed;
  private decimal naUpInterestOwed;
  private decimal naUdInterestOwed;
  private decimal naCaInterestOwed;
  private decimal afPaInterestOwed;
  private decimal afTaInterestOwed;
  private decimal afCaInterestOwed;
  private decimal fcPaInterestOwed;
  private decimal fcTaInterestOwed;
  private decimal fcCaInterestOwed;
  private decimal naNaArrearCollected;
  private decimal naUpArrearCollected;
  private decimal naUdArrearCollected;
  private decimal naCaArrearCollected;
  private decimal afPaArrearCollected;
  private decimal afTaArrearCollected;
  private decimal afCaArrearCollected;
  private decimal fcPaArrearCollected;
  private decimal fcTaArrearCollected;
  private decimal fcCaArrearCollected;
  private decimal naNaInterestCollected;
  private decimal naUpInterestCollected;
  private decimal naUdInterestCollected;
  private decimal naCaInterestCollected;
  private decimal afPaInterestCollected;
  private decimal afTaInterestCollected;
  private decimal afCaInterestCollected;
  private decimal fcPaInterestCollected;
  private decimal fcTaInterestCollected;
  private decimal fcCaInterestCollected;
}
